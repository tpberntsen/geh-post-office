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
using System.Collections.Concurrent;
using System.Threading.Tasks;
using Azure.Messaging.ServiceBus;

namespace Energinet.DataHub.MessageHub.Client.Factories
{
    public sealed class AzureServiceBusFactory : IMessageBusFactory, IAsyncDisposable
    {
        private readonly ServiceBusClient _client;

        private readonly ConcurrentDictionary<string, ServiceBusSender> _senders = new();

        public AzureServiceBusFactory(IServiceBusClientFactory serviceBusClientFactory)
        {
            if (serviceBusClientFactory == null) throw new ArgumentNullException(nameof(serviceBusClientFactory));

            _client = serviceBusClientFactory.Create();
        }

        public ISenderMessageBus GetSenderClient(string queueOrTopicName)
        {
            var key = $"{queueOrTopicName}";

            var sender = _senders.GetOrAdd(key, _ => _client.CreateSender(queueOrTopicName));

            return AzureSenderServiceBus.Wrap(sender);
        }

        public async Task<IReceiverMessageBus> GetSessionReceiverClientAsync(string queueOrTopicName, string sessionId)
        {
            var receiver = await _client.AcceptSessionAsync(queueOrTopicName, sessionId).ConfigureAwait(false);

            return AzureSessionReceiverServiceBus.Wrap(receiver);
        }

        public async ValueTask DisposeAsync()
        {
            foreach (var senderKeyValuePair in _senders)
            {
                await senderKeyValuePair.Value.DisposeAsync().ConfigureAwait(false);
            }

            await _client.DisposeAsync().ConfigureAwait(false);

            _senders.Clear();
        }
    }
}
