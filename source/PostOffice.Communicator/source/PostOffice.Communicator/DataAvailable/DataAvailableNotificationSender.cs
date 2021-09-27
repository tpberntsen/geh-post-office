// // Copyright 2020 Energinet DataHub A/S
// //
// // Licensed under the Apache License, Version 2.0 (the "License2");
// // you may not use this file except in compliance with the License.
// // You may obtain a copy of the License at
// //
// //     http://www.apache.org/licenses/LICENSE-2.0
// //
// // Unless required by applicable law or agreed to in writing, software
// // distributed under the License is distributed on an "AS IS" BASIS,
// // WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// // See the License for the specific language governing permissions and
// // limitations under the License.

using System;
using System.Threading.Tasks;
using Azure.Messaging.ServiceBus;
using Google.Protobuf;
using GreenEnergyHub.PostOffice.Communicator.Contracts;
using GreenEnergyHub.PostOffice.Communicator.Model;

namespace GreenEnergyHub.PostOffice.Communicator.DataAvailable
{
    public sealed class DataAvailableNotificationSender : IDataAvailableNotificationSender, IAsyncDisposable
    {
        private readonly ServiceBusClient _serviceBusClient;

        public DataAvailableNotificationSender(string connectionString)
        {
            _serviceBusClient = new ServiceBusClient(connectionString);
        }

        public async Task SendAsync(DataAvailableNotificationDto dataAvailableNotificationDto)
        {
            if (dataAvailableNotificationDto == null)
                throw new ArgumentNullException(nameof(dataAvailableNotificationDto));

            await using var sender = _serviceBusClient.CreateSender("sbq-dataavailable");
            using var messageBatch = await sender.CreateMessageBatchAsync().ConfigureAwait(false);

            var contract = new DataAvailableNotificationContract()
            {
                UUID = dataAvailableNotificationDto.Uuid,
                MessageType = dataAvailableNotificationDto.MessageType,
                Origin = dataAvailableNotificationDto.Origin,
                Recipient = dataAvailableNotificationDto.Recipient,
                SupportsBundling = dataAvailableNotificationDto.SupportsBundling,
                RelativeWeight = dataAvailableNotificationDto.RelativeWeight
            };

            var msgBytes = contract.ToByteArray();
            if (!messageBatch.TryAddMessage(new ServiceBusMessage(new BinaryData(msgBytes))))
                throw new InvalidOperationException("The message is too large to fit in the batch.");

            await sender.SendMessagesAsync(messageBatch).ConfigureAwait(false);
        }

        public async ValueTask DisposeAsync()
        {
            await _serviceBusClient.DisposeAsync().ConfigureAwait(false);
        }
    }
}
