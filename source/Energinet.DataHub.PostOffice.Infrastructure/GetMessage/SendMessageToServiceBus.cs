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
using Energinet.DataHub.PostOffice.Domain;
using Google.Protobuf;

namespace Energinet.DataHub.PostOffice.Infrastructure.GetMessage
{
    public class SendMessageToServiceBus : ISendMessageToServiceBus
    {
        private readonly ServiceBusClient? _serviceBusClient;
        private readonly string? _returnTopic = Environment.GetEnvironmentVariable("MessageReplyTopic");
        private ServiceBusSender? _sender;

        public SendMessageToServiceBus(ServiceBusClient serviceBusClient)
        {
            _serviceBusClient = serviceBusClient;
        }

        public async Task RequestDataAsync(RequestData requestData, string sessionId)
        {
            if (requestData is null) throw new ArgumentNullException(nameof(requestData));

            var originReceiver = FindQueueOrTopicNameFromOrigin(requestData.Origin ?? string.Empty);
            if (_serviceBusClient is not null) _sender = _serviceBusClient.CreateSender(originReceiver);

            var requestDatasetMessage = new Contracts.RequestDataset() { UUID = { requestData.Uuids } };
            var message = new ServiceBusMessage(requestDatasetMessage.ToByteArray()) { SessionId = sessionId };

            message.ReplyToSessionId = message.SessionId;
            message.ReplyTo = _returnTopic;

            // What if _sender is null?
            if (_sender is not null) await _sender.SendMessageAsync(message).ConfigureAwait(false);
        }

        private static string FindQueueOrTopicNameFromOrigin(string origin)
        {
            switch (origin)
            {
                case "charges":
                    return "sbt-outbund-charges";
                case "ts" or "timeseries":
                    return "ts";
                default:
                    #if DEBUG
                    return "sbt-outbund-charges";
                    #else
                    throw new Exception("Unknown origin name");
                    #endif
            }
        }
    }
}
