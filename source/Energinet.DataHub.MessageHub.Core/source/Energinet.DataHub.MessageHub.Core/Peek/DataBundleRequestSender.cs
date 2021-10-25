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
using System.Threading.Tasks;
using Azure.Messaging.ServiceBus;
using Energinet.DataHub.MessageHub.Core.Extensions;
using Energinet.DataHub.MessageHub.Core.Factories;
using Energinet.DataHub.MessageHub.Model.Model;
using Energinet.DataHub.MessageHub.Model.Peek;

namespace Energinet.DataHub.MessageHub.Core.Peek
{
    public sealed class DataBundleRequestSender : IDataBundleRequestSender, IAsyncDisposable
    {
        private readonly IRequestBundleParser _requestBundleParser;
        private readonly IResponseBundleParser _responseBundleParser;
        private readonly IServiceBusClientFactory _serviceBusClientFactory;
        private readonly PeekRequestConfig _peekRequestConfig;
        private readonly TimeSpan _defaultTimeout = TimeSpan.FromSeconds(30);
        private ServiceBusClient? _serviceBusClient;

        public DataBundleRequestSender(
            IRequestBundleParser requestBundleParser,
            IResponseBundleParser responseBundleParser,
            IServiceBusClientFactory serviceBusClientFactory,
            PeekRequestConfig peekRequestConfig)
        {
            _requestBundleParser = requestBundleParser;
            _responseBundleParser = responseBundleParser;
            _serviceBusClientFactory = serviceBusClientFactory;
            _peekRequestConfig = peekRequestConfig;
        }

        public async ValueTask DisposeAsync()
        {
            if (_serviceBusClient != null)
            {
                await _serviceBusClient.DisposeAsync().ConfigureAwait(false);
                _serviceBusClient = null;
            }
        }

        public async Task<DataBundleResponseDto?> SendAsync(
            DataBundleRequestDto dataBundleRequestDto,
            DomainOrigin domainOrigin)
        {
            if (dataBundleRequestDto == null)
                throw new ArgumentNullException(nameof(dataBundleRequestDto));

            var bytes = _requestBundleParser.Parse(dataBundleRequestDto);

            var sessionId = Guid.NewGuid().ToString();
            var replyQueue = GetReplyQueueName(domainOrigin);
            var targetQueue = GetQueueName(domainOrigin);

            var serviceBusMessage = new ServiceBusMessage(bytes)
            {
                SessionId = sessionId,
                ReplyToSessionId = sessionId,
                ReplyTo = replyQueue
            }.AddRequestDataBundleIntegrationEvents(dataBundleRequestDto.IdempotencyId);

            _serviceBusClient ??= _serviceBusClientFactory.Create();

            await using var sender = _serviceBusClient.CreateSender(targetQueue);
            await sender.SendMessageAsync(serviceBusMessage).ConfigureAwait(false);

            await using var receiver = await _serviceBusClient
                .AcceptSessionAsync(replyQueue, sessionId)
                .ConfigureAwait(false);

            var response = await receiver.ReceiveMessageAsync(_defaultTimeout).ConfigureAwait(false);
            if (response == null)
                return null;

            return _responseBundleParser.Parse(response.Body.ToArray());
        }

        private string GetQueueName(DomainOrigin domainOrigin)
        {
            switch (domainOrigin)
            {
                case DomainOrigin.Charges:
                    return _peekRequestConfig.ChargesQueue;
                case DomainOrigin.TimeSeries:
                    return _peekRequestConfig.TimeSeriesQueue;
                case DomainOrigin.Aggregations:
                    return _peekRequestConfig.AggregationsQueue;
                case DomainOrigin.MarketRoles:
                    return _peekRequestConfig.MarketRolesQueue;
                case DomainOrigin.MeteringPoints:
                    return _peekRequestConfig.MeteringPointsQueue;
                default:
                    throw new ArgumentOutOfRangeException(nameof(domainOrigin), domainOrigin, null);
            }
        }

        private string GetReplyQueueName(DomainOrigin domainOrigin)
        {
            switch (domainOrigin)
            {
                case DomainOrigin.Charges:
                    return _peekRequestConfig.ChargesReplyQueue;
                case DomainOrigin.TimeSeries:
                    return _peekRequestConfig.TimeSeriesReplyQueue;
                case DomainOrigin.Aggregations:
                    return _peekRequestConfig.AggregationsReplyQueue;
                case DomainOrigin.MarketRoles:
                    return _peekRequestConfig.MarketRolesReplyQueue;
                case DomainOrigin.MeteringPoints:
                    return _peekRequestConfig.MeteringPointsReplyQueue;
                default:
                    throw new ArgumentOutOfRangeException(nameof(domainOrigin), domainOrigin, null);
            }
        }
    }
}
