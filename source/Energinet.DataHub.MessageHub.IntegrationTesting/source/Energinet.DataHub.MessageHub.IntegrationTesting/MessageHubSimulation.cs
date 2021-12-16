// Copyright 2020 Energinet DataHub A/S
//
// Licensed under the Apache License, Version 2.0 (the "License2");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//     http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Azure.Messaging.ServiceBus;
using Energinet.DataHub.MessageHub.Core.Dequeue;
using Energinet.DataHub.MessageHub.Core.Factories;
using Energinet.DataHub.MessageHub.Core.Peek;
using Energinet.DataHub.MessageHub.Model.DataAvailable;
using Energinet.DataHub.MessageHub.Model.Model;
using Energinet.DataHub.MessageHub.Model.Peek;

namespace Energinet.DataHub.MessageHub.IntegrationTesting
{
    /// <summary>
    /// Facilitates integration testing between MessageHub and sub-domains.
    /// This class is not thread-safe.
    /// </summary>
    public sealed class MessageHubSimulation : IAsyncDisposable
    {
        private readonly TimeSpan _waitTimeout;

        private readonly IDataAvailableNotificationParser _dataAvailableNotificationParser = new DataAvailableNotificationParser();
        private readonly IDataBundleRequestSender _dataBundleRequestSender;
        private readonly IDequeueNotificationSender? _dequeueNotificationSender;

        private readonly ServiceBusReceiver _dataAvailableReceiver;
        private readonly AzureServiceBusFactory _messageBusFactory;
        private readonly StorageHandlerSimulation _storageHandlerSimulation;
        private readonly List<DataAvailableNotificationDto> _notifications = new();

        public MessageHubSimulation(MessageHubSimulationConfig configuration)
        {
            if (configuration == null)
                throw new ArgumentNullException(nameof(configuration));

            Notifications = new ReadOnlyCollection<DataAvailableNotificationDto>(_notifications);

            var serviceBusSingleton = new ServiceBusClientFactory(configuration.ServiceBusReadWriteConnectionString);
            _messageBusFactory = new AzureServiceBusFactory(serviceBusSingleton);

            _dataAvailableReceiver = serviceBusSingleton
                .Create()
                .CreateReceiver(configuration.DataAvailableQueueName);

            _dataBundleRequestSender = new DataBundleRequestSender(
                new RequestBundleParser(),
                new ResponseBundleParser(),
                _messageBusFactory,
                configuration.CreateSimulatedPeekRequestConfig());

            if (configuration.DomainDequeueQueueName != null)
            {
                _dequeueNotificationSender = new DequeueNotificationSender(
                    _messageBusFactory,
                    configuration.CreateSimulatedDequeueConfig());
            }

            _waitTimeout = configuration.WaitTimeout;
            _storageHandlerSimulation = new StorageHandlerSimulation(configuration);
        }

        /// <summary>
        /// Gets the list of notifications ready for PeekAsync.
        /// </summary>
        public IEnumerable<DataAvailableNotificationDto> Notifications { get; }

        /// <summary>
        /// Waits for the specified correlation ids to arrive on the dataavailable queue
        /// and adds their notifications to the current simulation.
        /// Can throw a TimeoutException or TaskCanceledException.
        /// </summary>
        /// <param name="correlationIds">The list of correlation ids to wait for.</param>
        public async Task WaitForNotificationsInDataAvailableQueueAsync(params string[] correlationIds)
        {
            using var cancellationTokenSource = new CancellationTokenSource(_waitTimeout);
            var cancellationToken = cancellationTokenSource.Token;

            var expectedIds = new HashSet<string>(correlationIds, StringComparer.OrdinalIgnoreCase);

            while (expectedIds.Count > 0)
            {
                var message = await _dataAvailableReceiver
                    .ReceiveMessageAsync(cancellationToken: cancellationToken)
                    .ConfigureAwait(false);

                if (message == null)
                    throw new TimeoutException("MessageHubSimulation: The expected dataavailable messages did not arrive.");

                if (message.ApplicationProperties.TryGetValue("OperationCorrelationId", out var correlationObj))
                {
                    if (correlationObj is string correlationId && expectedIds.Remove(correlationId))
                    {
                        var dataAvailableNotificationDto = _dataAvailableNotificationParser.Parse(message.Body.ToArray());
                        _notifications.Add(dataAvailableNotificationDto);
                    }
                }

                await _dataAvailableReceiver
                    .CompleteMessageAsync(message, cancellationToken)
                    .ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Creates a bundle using the current notifications and simulates a Peek from a market operator.
        /// Can throw TimeoutException.
        /// </summary>
        /// <returns>Returns information about the Peek request.</returns>
        public async Task<PeekSimulationResponseDto> PeekAsync()
        {
            var requestId = Guid.NewGuid();
            var idempotencyId = Guid.NewGuid();

            if (_notifications.Count == 0)
                throw new InvalidOperationException("MessageHubSimulation: No dataavailable was provided for Peek.");

            var messageType = _notifications
                .Select(x => x.MessageType.Value)
                .Distinct()
                .Single();

            var guids = _notifications.Select(x => x.Uuid);
            var referenceId = idempotencyId.ToString();

            await _storageHandlerSimulation
                .StorageHandler
                .AddDataAvailableNotificationIdsToStorageAsync(referenceId, guids)
                .ConfigureAwait(false);

            var request = new DataBundleRequestDto(
                requestId,
                referenceId,
                idempotencyId.ToString(),
                messageType);

            // This domain origin must be valid, but it is not used.
            // All the queue names point to the same domain.
            const DomainOrigin domainOrigin = DomainOrigin.Aggregations;

            var peekResponse = await _dataBundleRequestSender
                .SendAsync(request, domainOrigin)
                .ConfigureAwait(false);

            if (peekResponse == null)
                throw new TimeoutException("MessageHubSimulation: Waiting for Peek reply timed out.");

            return peekResponse.IsErrorResponse
                ? new PeekSimulationResponseDto()
                : new PeekSimulationResponseDto(requestId, referenceId, new AzureBlobContentDto(peekResponse.ContentUri));
        }

        /// <summary>
        /// Simulates a dequeue of the previous Peek request.
        /// </summary>
        /// <param name="simulatedPeek">The response of a simulated Peek to dequeue.</param>
        public Task DequeueAsync(PeekSimulationResponseDto simulatedPeek)
        {
            if (simulatedPeek == null)
                throw new ArgumentNullException(nameof(simulatedPeek));

            if (_dequeueNotificationSender == null)
                throw new InvalidOperationException("MessageHubSimulation: Simulation was not configured for Dequeue.");

            if (simulatedPeek is not { IsSuccess: true })
                throw new InvalidOperationException("MessageHubSimulation: Cannot dequeue, as the simulated Peek failed.");

            var marketOperator = _notifications
                .Select(x => x.Recipient.Value)
                .Distinct()
                .Single();

            var dequeue = new DequeueNotificationDto(
                simulatedPeek.DataAvailableNotificationReferenceId,
                new GlobalLocationNumberDto(marketOperator));

            // This domain origin must be valid, but it is not used.
            // All the queue names point to the same domain.
            const DomainOrigin domainOrigin = DomainOrigin.Aggregations;

            return _dequeueNotificationSender.SendAsync(simulatedPeek.CorrelationId.Value.ToString(), dequeue, domainOrigin);
        }

        /// <summary>
        /// Clears the state between simulations.
        /// </summary>
        public void Clear()
        {
            _notifications.Clear();
        }

        public async ValueTask DisposeAsync()
        {
            await _dataAvailableReceiver.DisposeAsync().ConfigureAwait(false);
            await _messageBusFactory.DisposeAsync().ConfigureAwait(false);
        }
    }
}
