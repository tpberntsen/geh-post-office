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
using GreenEnergyHub.PostOffice.Communicator.Factories;
using GreenEnergyHub.PostOffice.Communicator.Model;

namespace GreenEnergyHub.PostOffice.Communicator.Peek
{
    public sealed class DataBundleRequestSender : IDataBundleRequestSender, IAsyncDisposable
    {
        private readonly IRequestBundleParser _requestBundleParser;
        private readonly ServiceBusClient _serviceBusClient;
        private readonly TimeSpan _requestBundleTimout;

        public DataBundleRequestSender(IRequestBundleParser requestBundleParser, IServiceBusClientFactory serviceBusClientFactory, TimeSpan requestBundleTimout)
        {
            if (serviceBusClientFactory == null)
                throw new ArgumentNullException(nameof(serviceBusClientFactory));

            _requestBundleParser = requestBundleParser;
            _serviceBusClient = serviceBusClientFactory.Create();
            _requestBundleTimout = requestBundleTimout;
        }

        public async ValueTask DisposeAsync()
        {
            await _serviceBusClient.DisposeAsync().ConfigureAwait(false);
        }

        public async Task<RequestDataBundleResponseDto?> SendAsync(
            DataBundleRequestDto dataBundleRequestDto,
            DomainOrigin domainOrigin)
        {
            if (dataBundleRequestDto == null)
                throw new ArgumentNullException(nameof(dataBundleRequestDto));

            if (!_requestBundleParser.TryParse(dataBundleRequestDto, out var bytes))
                throw new InvalidOperationException("Could not parse Bundle request");

            var sessionId = Guid.NewGuid().ToString();
            var serviceBusMessage = new ServiceBusMessage(bytes)
            {
                SessionId = sessionId,
                ReplyToSessionId = sessionId,
                ReplyTo = $"sbq-{domainOrigin.ToString()}-reply"
            };

            await using var sender = _serviceBusClient.CreateSender($"sbq-{domainOrigin.ToString()}");
            await sender.SendMessageAsync(serviceBusMessage).ConfigureAwait(false);

            await using var receiver = await _serviceBusClient.AcceptSessionAsync(
                    $"sbq-{domainOrigin.ToString()}-reply",
                    sessionId)
                .ConfigureAwait(false);
            var response = await receiver.ReceiveMessageAsync(_requestBundleTimout).ConfigureAwait(false);

            if (response == null)
                return null;

            if (!_requestBundleParser.TryParse(response.Body.ToArray(), out RequestDataBundleResponseDto? dto))
                throw new InvalidOperationException("Could not parse Bundle response");

            return dto;
        }
    }
}
