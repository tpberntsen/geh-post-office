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
using Energinet.DataHub.PostOffice.Application.GetMessage.Interfaces;
using Energinet.DataHub.PostOffice.Contracts;
using Energinet.DataHub.PostOffice.Domain;
using Energinet.DataHub.PostOffice.Domain.Enums;

namespace Energinet.DataHub.PostOffice.Infrastructure.GetMessage
{
    public class GetPathToDataFromServiceBus : IGetPathToDataFromServiceBus
    {
        private readonly ServiceBusClient _serviceBusClient;

        public GetPathToDataFromServiceBus(ServiceBusClient serviceBusClient)
        {
            _serviceBusClient = serviceBusClient;
        }

        public async Task<MessageReply> GetPathAsync(string queueName, string sessionId)
        {
            var receiver = await _serviceBusClient.AcceptSessionAsync(queueName, "sbs-messagereply", sessionId).ConfigureAwait(false);
            var received = await receiver.ReceiveMessageAsync(TimeSpan.FromSeconds(3)).ConfigureAwait(false);
            var replyMessage = DatasetReply.Parser.ParseFrom(received.Body.ToArray());

            return replyMessage.ReplyCase == DatasetReply.ReplyOneofCase.Success
                ? SuccessReply(replyMessage.Success)
                : FailureReply(replyMessage.Failure);
        }

        private static MessageReply SuccessReply(DatasetReply.Types.FileResource fileResource)
        {
            return new()
            {
                DataPath = fileResource.Uri,
                Uuids = fileResource.UUID,
            };
        }

        private static MessageReply FailureReply(DatasetReply.Types.RequestFailure requestFailure)
        {
            return new()
            {
                FailureReason = (MessageReplyFailureReason)(int)requestFailure.Reason,
                FailureDescription = requestFailure.FailureDescription,
                Uuids = requestFailure.UUID,
            };
        }
    }
}
