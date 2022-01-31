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
using Energinet.DataHub.MessageHub.Client.Extensions;
using Energinet.DataHub.MessageHub.Client.Factories;
using Energinet.DataHub.MessageHub.Model.Model;
using Energinet.DataHub.MessageHub.Model.Protobuf;
using Google.Protobuf;

namespace Energinet.DataHub.MessageHub.Client.DataAvailable
{
    public sealed class DataAvailableNotificationSender : IDataAvailableNotificationSender
    {
        private readonly IMessageBusFactory _messageBusFactory;
        private readonly MessageHubConfig _messageHubConfig;

        public DataAvailableNotificationSender(IMessageBusFactory messageBusFactory, MessageHubConfig messageHubConfig)
        {
            _messageBusFactory = messageBusFactory;
            _messageHubConfig = messageHubConfig;
        }

        public Task SendAsync(string correlationId, DataAvailableNotificationDto dataAvailableNotificationDto)
        {
            Guard.ThrowIfNull(correlationId, nameof(correlationId));
            Guard.ThrowIfNull(dataAvailableNotificationDto, nameof(dataAvailableNotificationDto));

            var sender = _messageBusFactory.GetSenderClient(_messageHubConfig.DataAvailableQueue);

            var contract = new DataAvailableNotificationContract
            {
                UUID = dataAvailableNotificationDto.Uuid.ToString(),
                MessageType = dataAvailableNotificationDto.MessageType.Value,
                Origin = dataAvailableNotificationDto.Origin.ToString(),
                Recipient = dataAvailableNotificationDto.Recipient.Value,
                SupportsBundling = dataAvailableNotificationDto.SupportsBundling,
                RelativeWeight = dataAvailableNotificationDto.RelativeWeight,
                DocumentType = dataAvailableNotificationDto.DocumentType
            };

            var messageId = Guid.NewGuid().ToString();

            var message = new ServiceBusMessage(new BinaryData(contract.ToByteArray())) { MessageId = messageId, PartitionKey = dataAvailableNotificationDto.Origin.ToString() }
                .AddDataAvailableIntegrationEvents(correlationId);

            return sender.PublishMessageAsync<ServiceBusMessage>(message);
        }
    }
}
