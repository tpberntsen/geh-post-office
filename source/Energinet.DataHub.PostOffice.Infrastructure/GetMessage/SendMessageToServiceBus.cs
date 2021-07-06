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
using System.Collections.Generic;
using System.Threading.Tasks;
using Azure.Messaging.ServiceBus;
using Energinet.DataHub.PostOffice.Application.GetMessage;

namespace Energinet.DataHub.PostOffice.Infrastructure.GetMessage
{
    public class SendMessageToServiceBus : ISendMessageToServiceBus
    {
        private readonly ServiceBusClient? _serviceBusClient;
        private ServiceBusSender? _sender;

        public SendMessageToServiceBus(ServiceBusClient serviceBusClient)
        {
            _serviceBusClient = serviceBusClient;
        }

        public async Task SendMessageAsync(IList<string> collection, string queueName, string sessionId)
        {
            if (collection is null) throw new ArgumentNullException(nameof(collection));

            // What if _serviceBusClient is null?
            if (_serviceBusClient is not null) _sender = _serviceBusClient.CreateSender(queueName);

            var message = new ServiceBusMessage(collection.ToString()) { SessionId = sessionId };

            message.ReplyToSessionId = message.SessionId;
            message.ReplyTo = queueName;

            // What if _sender is null?
            if (_sender is not null) await _sender.SendMessageAsync(message).ConfigureAwait(false);
        }
    }
}
