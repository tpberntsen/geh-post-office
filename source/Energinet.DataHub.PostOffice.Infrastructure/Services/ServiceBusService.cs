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
using Energinet.DataHub.PostOffice.Contracts;
using Energinet.DataHub.PostOffice.Domain.Model;
using Energinet.DataHub.PostOffice.Domain.Services.Model;
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

        public async Task<RequestDataSession> RequestBundledDataFromSubDomainAsync(IEnumerable<DataAvailableNotification> notifications, SubDomain subDomain)
        {
            var sender = GetServiceBusSender(subDomain);
            var requestDataSession = new RequestDataSession() { Id = new Uuid(System.Guid.NewGuid().ToString()) };
            var requestDatasetMessage = new Contracts.RequestDataset()
                {
                    UUID = { notifications.Select(x => x.NotificationId.Value) }
                };
            var message =
                new ServiceBusMessage(requestDatasetMessage.ToByteArray()) { SessionId = requestDataSession.Id.Value };

            message.ReplyToSessionId = message.SessionId;
            message.ReplyTo = $"sbq-{subDomain.ToString()}-reply";
            await sender.SendMessageAsync(message).ConfigureAwait(false);
            return requestDataSession;
        }

        public async Task<SubDomainReply> WaitForReplyFromSubDomainAsync(RequestDataSession session, SubDomain subDomain)
        {
            if (session is null)
                throw new ArgumentNullException(nameof(session));

            var receiver = await GetServiceBusRecieverAsync(session, subDomain).ConfigureAwait(false);

            var received = await receiver
                .ReceiveMessageAsync(TimeSpan.FromSeconds(3))
                .ConfigureAwait(false);
            var replyMessage = DatasetReply.Parser.ParseFrom(received.Body.ToArray());

            return new SubDomainReply()
            {
                Success = replyMessage.ReplyCase == DatasetReply.ReplyOneofCase.Success,
                UriToContent = new Uri(replyMessage.Success.Uri)
            };
        }

        private ServiceBusSender GetServiceBusSender(SubDomain subDomain) => subDomain switch
        {
            SubDomain.Aggregations => _serviceBusClient.CreateSender($"sbq-{nameof(SubDomain.Aggregations)}"),
            SubDomain.Charges => _serviceBusClient.CreateSender($"sbq-{nameof(SubDomain.Charges)}"),
            SubDomain.TimeSeries =>_serviceBusClient.CreateSender($"sbq-{nameof(SubDomain.TimeSeries)}"),
            _ => throw new ArgumentException($"Unknown Origin: {subDomain}", nameof(subDomain)),
        };

        private async Task<ServiceBusSessionReceiver> GetServiceBusRecieverAsync(RequestDataSession session, SubDomain subDomain) => subDomain switch
        {
            SubDomain.Aggregations => await _serviceBusClient.AcceptSessionAsync(
                $"sbq-{nameof(SubDomain.Aggregations)}-reply",
                session.Id.Value)
                .ConfigureAwait(false),
            SubDomain.Charges => await _serviceBusClient.AcceptSessionAsync(
                $"sbq-{nameof(SubDomain.Charges)}-reply",
                session.Id.Value)
                .ConfigureAwait(false),
            SubDomain.TimeSeries => await _serviceBusClient.AcceptSessionAsync(
                $"sbq-{nameof(SubDomain.TimeSeries)}-reply",
                session.Id.Value)
                .ConfigureAwait(false),
            _ => throw new ArgumentException($"Unknown Origin: {subDomain}", nameof(subDomain)),
        };
    }
}
