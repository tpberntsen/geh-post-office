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
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Azure.Messaging.ServiceBus;
using Energinet.DataHub.PostOffice.Domain.Model;
using Google.Protobuf;

namespace Energinet.DataHub.PostOffice.Infrastructure.Services
{
    public class ServiceBusService : IServiceBusService
    {
        private readonly ServiceBusClient _serviceBusClient;

        public ServiceBusService(ServiceBusClient serviceBusClient)
        {
            _serviceBusClient = serviceBusClient;
        }

        public async Task RequestDataFromSubDomainAsync(IEnumerable<DataAvailableNotification> notifications, Origin origin)
        {
            var sender = GetServiceBusSender(origin);
            var sessionId = System.Guid.NewGuid().ToString();
            var requestDatasetMessage = new Contracts.RequestDataset() { UUID = { notifications.Select(x => x.Id.Value) } };
            var message = new ServiceBusMessage(requestDatasetMessage.ToByteArray()) { SessionId = sessionId };

            message.ReplyToSessionId = message.SessionId;
            message.ReplyTo = "returnQueueName";
            await sender.SendMessageAsync(message).ConfigureAwait(false);
        }

        internal ServiceBusSender GetServiceBusSender(Origin origin) => origin switch
        {
            Origin.Aggregations => _serviceBusClient.CreateSender($"sbq-{nameof(Origin.Aggregations)}"),
            Origin.Charges => _serviceBusClient.CreateSender($"sbq-{nameof(Origin.Charges)}"),
            Origin.TimeSeries =>_serviceBusClient.CreateSender($"sbq-{nameof(Origin.TimeSeries)}"),
            _ => throw new ArgumentException($"Unknown Origin: {origin}", nameof(origin)),
        };
    }
}
