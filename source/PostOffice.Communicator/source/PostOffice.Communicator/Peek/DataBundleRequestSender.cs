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
using Google.Protobuf;
using GreenEnergyHub.PostOffice.Communicator.Contracts;
using GreenEnergyHub.PostOffice.Communicator.Model;

namespace GreenEnergyHub.PostOffice.Communicator.Peek
{
    public sealed class DataBundleRequestSender : IDataBundleRequestSender, IAsyncDisposable
    {
        private readonly IRequestBundleRequestParser _requestBundleRequestParser;
        private readonly ServiceBusClient _serviceBusClient;
        private readonly string _queue;
        private readonly string _replyQueue;
        private readonly TimeSpan _requestBundleTimout;

        public DataBundleRequestSender(IRequestBundleRequestParser requestBundleRequestParser, string connectionString, DomainOrigin domainOrigin, TimeSpan requestBundleTimout)
        {
            _requestBundleRequestParser = requestBundleRequestParser;
            _serviceBusClient = new ServiceBusClient(connectionString);
            _queue = $"sbq-{domainOrigin.ToString()}";
            _replyQueue = $"sbq-{domainOrigin.ToString()}-reply";
            _requestBundleTimout = requestBundleTimout;
        }

        public async ValueTask DisposeAsync()
        {
            await _serviceBusClient.DisposeAsync().ConfigureAwait(false);
        }

        public async Task<RequestDataBundleResponseDto?> SendAsync(DataBundleRequestDto dataBundleRequestDto)
        {
            if (dataBundleRequestDto == null)
                throw new ArgumentNullException(nameof(dataBundleRequestDto));

            var sessionId = Guid.NewGuid().ToString();
            var message = new RequestBundleRequest { IdempotencyId = dataBundleRequestDto.IdempotencyId, UUID = { dataBundleRequestDto.DataAvailableNotificationIds } };
            var serviceBusMessage = new ServiceBusMessage(message.ToByteArray()) { SessionId = sessionId, ReplyToSessionId = sessionId, ReplyTo = _replyQueue };

            await using var sender = _serviceBusClient.CreateSender(_queue);
            await sender.SendMessageAsync(serviceBusMessage).ConfigureAwait(false);

            await using var receiver = await _serviceBusClient.AcceptSessionAsync(_replyQueue, sessionId).ConfigureAwait(false);
            var response = await receiver.ReceiveMessageAsync(_requestBundleTimout).ConfigureAwait(false);

            if (response is null)
                return null;

            var bundleResponse = RequestBundleResponse.Parser.ParseFrom(response.Body.ToArray());

            return bundleResponse.ReplyCase != RequestBundleResponse.ReplyOneofCase.Success
                ? null
                : new RequestDataBundleResponseDto(new Uri(bundleResponse.Success.Uri));
        }
    }
}
