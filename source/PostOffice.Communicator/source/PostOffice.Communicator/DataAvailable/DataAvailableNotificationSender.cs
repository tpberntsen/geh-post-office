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
using GreenEnergyHub.PostOffice.Communicator.Model;

namespace GreenEnergyHub.PostOffice.Communicator.DataAvailable
{
    public class DataAvailableNotificationSender : IDataAvailableNotificationSender, IAsyncDisposable
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
            var msg = new Contracts.DataAvailableNotificationContract().ToByteArray();
            if (!messageBatch.TryAddMessage(new ServiceBusMessage(new BinaryData(msg))))
            {
                throw new InvalidOperationException("The message is too large to fit in the batch.");
            }

            Console.WriteLine(
                $"Message added to batch, uuid: {dataAvailableNotificationDto.UUID}, recipient: {dataAvailableNotificationDto.Recipient} ");

            await sender.SendMessagesAsync(messageBatch).ConfigureAwait(false);
        }

        public async ValueTask DisposeAsync()
        {
            GC.SuppressFinalize(this);
            await _serviceBusClient.DisposeAsync().ConfigureAwait(false);
        }
    }
}
