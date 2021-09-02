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
using System.Threading;
using System.Threading.Tasks;
using Azure.Messaging.ServiceBus;
using Energinet.DataHub.PostOffice.Contracts;
using Google.Protobuf;

namespace Energinet.DataHub.PostOffice.IntegrationTests.Common
{
    internal sealed class MockedServiceBusClient : ServiceBusClient
    {
        private static readonly MockedServiceBusSender _globalSender = new();
        private static readonly ConcurrentDictionary<string, MockedServiceBusSessionReceiver> _receivers = new();

        public override ServiceBusSender CreateSender(string queueOrTopicName)
        {
            return _globalSender;
        }

        public override Task<ServiceBusSessionReceiver> AcceptSessionAsync(
            string queueName,
            string sessionId,
            ServiceBusSessionReceiverOptions options = null!,
            CancellationToken cancellationToken = default)
        {
            var compositeKey = $"{queueName}-{sessionId}";
            var receiver = _receivers.GetOrAdd(compositeKey, _ => new MockedServiceBusSessionReceiver());
            return Task.FromResult<ServiceBusSessionReceiver>(receiver);
        }

        public override async ValueTask DisposeAsync()
        {
            try
            {
                await base.DisposeAsync().ConfigureAwait(false);
            }
            catch (NullReferenceException)
            {
                // Mocked ServiceBusClient does not have a Connection, which crashes DisposeAsync().
            }
        }

        private sealed class MockedServiceBusSender : ServiceBusSender
        {
            public override Task SendMessageAsync(ServiceBusMessage message, CancellationToken cancellationToken = default)
            {
                var compositeKey = $"{message.ReplyTo}-{message.SessionId}";
                var receiver = _receivers.GetOrAdd(compositeKey, _ => new MockedServiceBusSessionReceiver());
                receiver.EnqueueMockedMessage();
                return Task.CompletedTask;
            }
        }

        private sealed class MockedServiceBusSessionReceiver : ServiceBusSessionReceiver
        {
            private readonly ConcurrentQueue<ServiceBusReceivedMessage> _queue = new();

            public void EnqueueMockedMessage()
            {
                const string pathToMockedContent = "https://localhost:8000/path/to/content";

                var protobufMessage = new DatasetReply
                {
                    Success = new DatasetReply.Types.FileResource
                    {
                        Uri = pathToMockedContent
                    }
                };

                var binaryMessage = new BinaryData(protobufMessage.ToByteArray());
                var mockedMessage = ServiceBusModelFactory.ServiceBusReceivedMessage(binaryMessage);
                _queue.Enqueue(mockedMessage);
            }

            public override Task<ServiceBusReceivedMessage?> ReceiveMessageAsync(TimeSpan? maxWaitTime = default, CancellationToken cancellationToken = default)
            {
                return _queue.TryDequeue(out var message)
                    ? Task.FromResult<ServiceBusReceivedMessage?>(message)
                    : Task.FromResult<ServiceBusReceivedMessage?>(null);
            }
        }
    }
}
