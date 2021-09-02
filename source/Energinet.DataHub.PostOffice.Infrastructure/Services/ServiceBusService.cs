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
using Energinet.DataHub.PostOffice.Domain.Services;
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

        public async Task<RequestDataSession> RequestBundledDataFromSubDomainAsync(IEnumerable<DataAvailableNotification> notifications, DomainOrigin domainOrigin)
        {
            var sender = GetServiceBusSender(domainOrigin);
            var requestDataSession = new RequestDataSession() { Id = new Uuid(Guid.NewGuid().ToString()) };
            var requestDatasetMessage = new RequestDataset()
                {
                    UUID = { notifications.Select(x => x.NotificationId.ToString()) }
                };
            var message =
                new ServiceBusMessage(requestDatasetMessage.ToByteArray()) { SessionId = requestDataSession.Id.ToString() };

            message.ReplyToSessionId = message.SessionId;
            message.ReplyTo = $"sbq-{domainOrigin.ToString()}-reply";
            await sender.SendMessageAsync(message).ConfigureAwait(false);
            return requestDataSession;
        }

        public async Task<SubDomainReply> WaitForReplyFromSubDomainAsync(RequestDataSession session, DomainOrigin domainOrigin)
        {
            if (session is null)
                throw new ArgumentNullException(nameof(session));

            var receiver = await GetServiceBusReceiverAsync(session, domainOrigin).ConfigureAwait(false);

            var received = await receiver
                .ReceiveMessageAsync(TimeSpan.FromSeconds(3))
                .ConfigureAwait(false);

            if (received is null)
                return new SubDomainReply() { Success = false };

            var replyMessage = DatasetReply.Parser.ParseFrom(received.Body.ToArray());
            if (replyMessage.ReplyCase == DatasetReply.ReplyOneofCase.Success)
            {
                return new SubDomainReply()
                {
                    Success = true,
                    UriToContent = new Uri(replyMessage.Success.Uri)
                };
            }

            return new SubDomainReply()
            {
                Success = false
            };
        }

        private ServiceBusSender GetServiceBusSender(DomainOrigin domainOrigin) => domainOrigin switch
        {
            DomainOrigin.Aggregations => _serviceBusClient.CreateSender($"sbq-{nameof(DomainOrigin.Aggregations)}"),
            DomainOrigin.Charges => _serviceBusClient.CreateSender($"sbq-{nameof(DomainOrigin.Charges)}"),
            DomainOrigin.TimeSeries =>_serviceBusClient.CreateSender($"sbq-{nameof(DomainOrigin.TimeSeries)}"),
            _ => throw new ArgumentException($"Unknown Origin: {domainOrigin}", nameof(domainOrigin)),
        };

        private async Task<ServiceBusSessionReceiver> GetServiceBusReceiverAsync(RequestDataSession session, DomainOrigin domainOrigin) => domainOrigin switch
        {
            DomainOrigin.Aggregations => await _serviceBusClient.AcceptSessionAsync(
                $"sbq-{nameof(DomainOrigin.Aggregations)}-reply",
                session.Id.ToString())
                .ConfigureAwait(false),
            DomainOrigin.Charges => await _serviceBusClient.AcceptSessionAsync(
                $"sbq-{nameof(DomainOrigin.Charges)}-reply",
                session.Id.ToString())
                .ConfigureAwait(false),
            DomainOrigin.TimeSeries => await _serviceBusClient.AcceptSessionAsync(
                $"sbq-{nameof(DomainOrigin.TimeSeries)}-reply",
                session.Id.ToString())
                .ConfigureAwait(false),
            DomainOrigin.Unknown => throw new ArgumentException($"Unknown Origin: {domainOrigin}", nameof(domainOrigin)),
            _ => throw new ArgumentException($"Unknown Origin: {domainOrigin}", nameof(domainOrigin)),
        };
    }
}
