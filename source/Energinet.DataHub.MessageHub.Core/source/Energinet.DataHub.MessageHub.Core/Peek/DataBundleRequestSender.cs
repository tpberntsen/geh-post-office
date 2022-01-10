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
    public sealed class DataBundleRequestSender : IDataBundleRequestSender
    {
        private readonly IRequestBundleParser _requestBundleParser;
        private readonly IResponseBundleParser _responseBundleParser;
        private readonly IMessageBusFactory _messageBusFactory;
        private readonly PeekRequestConfig _peekRequestConfig;
        private readonly TimeSpan _defaultTimeout = TimeSpan.FromSeconds(30);

        public DataBundleRequestSender(
            IRequestBundleParser requestBundleParser,
            IResponseBundleParser responseBundleParser,
            IMessageBusFactory serviceBusClientFactory,
            PeekRequestConfig peekRequestConfig)
        {
            _requestBundleParser = requestBundleParser;
            _responseBundleParser = responseBundleParser;
            _messageBusFactory = serviceBusClientFactory;
            _peekRequestConfig = peekRequestConfig;
        }

        public async Task<DataBundleResponseDto?> SendAsync(
            DataBundleRequestDto dataBundleRequestDto,
            DomainOrigin domainOrigin)
        {
            Guard.ThrowIfNull(dataBundleRequestDto, nameof(dataBundleRequestDto));

            var bytes = _requestBundleParser.Parse(dataBundleRequestDto);

            var sessionId = dataBundleRequestDto.RequestId.ToString();

            var replyQueue = GetReplyQueueName(domainOrigin);
            var targetQueue = GetQueueName(domainOrigin);

            var serviceBusMessage = new ServiceBusMessage(bytes)
            {
                SessionId = sessionId,
                ReplyToSessionId = sessionId,
                ReplyTo = replyQueue
            }.AddRequestDataBundleIntegrationEvents(dataBundleRequestDto.IdempotencyId);

            var serviceBusClient = _messageBusFactory.GetSenderClient(targetQueue);

            await serviceBusClient
                .PublishMessageAsync<ServiceBusMessage>(serviceBusMessage)
                .ConfigureAwait(false);

            await using var receiverMessageBus = await _messageBusFactory
                .GetSessionReceiverClientAsync(replyQueue, sessionId)
                .ConfigureAwait(false);

            var response = await receiverMessageBus
                .ReceiveMessageAsync<ServiceBusMessage>(_peekRequestConfig.PeekTimeout ?? _defaultTimeout)
                .ConfigureAwait(false);

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
