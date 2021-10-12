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
using Energinet.DataHub.MessageHub.Client.Model;

namespace Energinet.DataHub.MessageHub.Client.Peek
{
    public sealed class DataBundleResponseSender : IDataBundleResponseSender, IAsyncDisposable
    {
        private readonly IResponseBundleParser _responseBundleParser;
        private readonly IServiceBusClientFactory _serviceBusClientFactory;
        private readonly DomainConfig _domainConfig;
        private ServiceBusClient? _serviceBusClient;

        public DataBundleResponseSender(
            IResponseBundleParser responseBundleParser,
            IServiceBusClientFactory serviceBusClientFactory,
            DomainConfig domainConfig)
        {
            _responseBundleParser = responseBundleParser;
            _serviceBusClientFactory = serviceBusClientFactory;
            _domainConfig = domainConfig;
        }

        public async Task SendAsync(
            DataBundleResponseDto dataBundleResponseDto,
            DataBundleRequestDto requestDto,
            string sessionId)
        {
            if (dataBundleResponseDto is null)
                throw new ArgumentNullException(nameof(dataBundleResponseDto));

            if (sessionId is null)
                throw new ArgumentNullException(nameof(sessionId));

            if (requestDto is null)
                throw new ArgumentNullException(nameof(requestDto));

            var contractBytes = _responseBundleParser.Parse(dataBundleResponseDto);
            var serviceBusReplyMessage = new ServiceBusMessage(contractBytes)
            {
                SessionId = sessionId,
            }.AddDataBundleResponseIntegrationEvents(requestDto.IdempotencyId);

            _serviceBusClient ??= _serviceBusClientFactory.Create();
            await using var sender = _serviceBusClient.CreateSender(_domainConfig.ReplyQueue);
            await sender.SendMessageAsync(serviceBusReplyMessage).ConfigureAwait(false);
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
