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
using Energinet.DataHub.MessageHub.Model.Protobuf;
using Google.Protobuf;

namespace Energinet.DataHub.MessageHub.Core.Dequeue
{
    public sealed class DequeueNotificationSender : IDequeueNotificationSender
    {
        private readonly IMessageBusFactory _messageBusFactory;
        private readonly DequeueConfig _dequeueConfig;

        public DequeueNotificationSender(IMessageBusFactory messageBusFactory, DequeueConfig dequeueConfig)
        {
            _messageBusFactory = messageBusFactory;
            _dequeueConfig = dequeueConfig;
        }

        public Task SendAsync(string correlationId, DequeueNotificationDto dequeueNotificationDto, DomainOrigin domainOrigin)
        {
            Guard.ThrowIfNull(dequeueNotificationDto, nameof(dequeueNotificationDto));

            var queueName = GetQueueName(domainOrigin);
            var serviceBusSender = _messageBusFactory.GetSenderClient(queueName);

            var contract = new DequeueContract
            {
                DataAvailableNotificationReferenceId = dequeueNotificationDto.DataAvailableNotificationReferenceId,
                MarketOperator = dequeueNotificationDto.MarketOperator.Value
            };

            var dequeueMessage = new ServiceBusMessage(new BinaryData(contract.ToByteArray()))
                .AddDequeueIntegrationEvents(correlationId);

            return serviceBusSender.PublishMessageAsync<ServiceBusMessage>(dequeueMessage);
        }

        private string GetQueueName(DomainOrigin domainOrigin)
        {
            switch (domainOrigin)
            {
                case DomainOrigin.Charges:
                    return _dequeueConfig.ChargesDequeueQueue;
                case DomainOrigin.TimeSeries:
                    return _dequeueConfig.TimeSeriesDequeueQueue;
                case DomainOrigin.Aggregations:
                    return _dequeueConfig.AggregationsDequeueQueue;
                case DomainOrigin.MarketRoles:
                    return _dequeueConfig.MarketRolesDequeueQueue;
                case DomainOrigin.MeteringPoints:
                    return _dequeueConfig.MeteringPointsDequeueQueue;
                default:
                    throw new ArgumentOutOfRangeException(nameof(domainOrigin), domainOrigin, null);
            }
        }
    }
}
