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
using Energinet.DataHub.MessageHub.Model.Peek;

namespace Energinet.DataHub.MessageHub.Client.Peek
{
    public sealed class DataBundleResponseSender : IDataBundleResponseSender, IAsyncDisposable
    {
        private readonly IResponseBundleParser _responseBundleParser;
        private readonly IMessageBusFactory _messageBusFactory;
        private readonly MessageHubConfig _messageHubConfig;
        private ServiceBusClient? _serviceBusClient;

        public DataBundleResponseSender(
            IResponseBundleParser responseBundleParser,
            IMessageBusFactory messageBusFactory,
            MessageHubConfig messageHubConfig)
        {
            _responseBundleParser = responseBundleParser;
            _messageBusFactory = messageBusFactory;
            _messageHubConfig = messageHubConfig;
        }

        public async Task SendAsync(
            DataBundleResponseDto dataBundleResponseDto,
            DataBundleRequestDto requestDto)
        {
            if (dataBundleResponseDto is null)
                throw new ArgumentNullException(nameof(dataBundleResponseDto));

            if (requestDto is null)
                throw new ArgumentNullException(nameof(requestDto));

            var contractBytes = _responseBundleParser.Parse(dataBundleResponseDto);
            var serviceBusReplyMessage = new ServiceBusMessage(contractBytes)
            {
                SessionId = requestDto.IdempotencyId,
            }.AddDataBundleResponseIntegrationEvents(requestDto.IdempotencyId);

            var sender = _messageBusFactory.GetSenderClient(_messageHubConfig.DomainReplyQueue);
            await sender.PublishMessageAsync<ServiceBusMessage>(serviceBusReplyMessage).ConfigureAwait(false);
        }

        public async ValueTask DisposeAsync()
        {
            if (_serviceBusClient is not null)
            {
                await _serviceBusClient.DisposeAsync().ConfigureAwait(false);
                _serviceBusClient = null;
            }
        }
    }
}
