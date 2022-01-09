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

using System.Threading.Tasks;
using Azure.Messaging.ServiceBus;
using Energinet.DataHub.MessageHub.Client.Extensions;
using Energinet.DataHub.MessageHub.Client.Factories;
using Energinet.DataHub.MessageHub.Model.Model;
using Energinet.DataHub.MessageHub.Model.Peek;

namespace Energinet.DataHub.MessageHub.Client.Peek
{
    public sealed class DataBundleResponseSender : IDataBundleResponseSender
    {
        private readonly IResponseBundleParser _responseBundleParser;
        private readonly IMessageBusFactory _messageBusFactory;
        private readonly MessageHubConfig _messageHubConfig;

        public DataBundleResponseSender(
            IResponseBundleParser responseBundleParser,
            IMessageBusFactory messageBusFactory,
            MessageHubConfig messageHubConfig)
        {
            _responseBundleParser = responseBundleParser;
            _messageBusFactory = messageBusFactory;
            _messageHubConfig = messageHubConfig;
        }

        public Task SendAsync(DataBundleResponseDto dataBundleResponseDto)
        {
            Guard.ThrowIfNull(dataBundleResponseDto);

            var sessionId = dataBundleResponseDto.RequestId.ToString();

            var contractBytes = _responseBundleParser.Parse(dataBundleResponseDto);
            var serviceBusReplyMessage = new ServiceBusMessage(contractBytes)
            {
                SessionId = sessionId,
            }.AddDataBundleResponseIntegrationEvents(dataBundleResponseDto.RequestIdempotencyId);

            var sender = _messageBusFactory.GetSenderClient(_messageHubConfig.DomainReplyQueue);
            return sender.PublishMessageAsync<ServiceBusMessage>(serviceBusReplyMessage);
        }
    }
}
