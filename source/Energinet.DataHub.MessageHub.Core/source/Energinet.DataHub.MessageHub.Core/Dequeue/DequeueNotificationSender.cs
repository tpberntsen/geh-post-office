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
using System.Linq;
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

        public DequeueNotificationSender(IMessageBusFactory messageBusFactory)
        {
            _messageBusFactory = messageBusFactory;
        }

        public Task SendAsync(DequeueNotificationDto dequeueNotificationDto, DomainOrigin domainOrigin)
        {
            if (dequeueNotificationDto is null)
                throw new ArgumentNullException(nameof(dequeueNotificationDto));

            var serviceBusSender = _messageBusFactory.GetSenderClient($"sbq-{domainOrigin}-dequeue");

            var contract = new DequeueContract
            {
                DataAvailableNotificationIds = { dequeueNotificationDto.DataAvailableNotificationIds.Select(x => x.ToString()) },
                MarketOperator = dequeueNotificationDto.MarketOperator.Value
            };

            var dequeueMessage = new ServiceBusMessage(new BinaryData(contract.ToByteArray())).AddDequeueIntegrationEvents();
            return serviceBusSender.PublishMessageAsync<ServiceBusMessage>(dequeueMessage);
        }
    }
}
