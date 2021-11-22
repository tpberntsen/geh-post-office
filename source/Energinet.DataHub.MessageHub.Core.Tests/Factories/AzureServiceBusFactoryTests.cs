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
using Energinet.DataHub.MessageHub.Core.Factories;
using Energinet.DataHub.MessageHub.Core.Tests.Peek;
using Energinet.DataHub.MessageHub.Model.Protobuf;
using Google.Protobuf;
using Moq;
using Xunit;
using Xunit.Categories;

namespace Energinet.DataHub.MessageHub.Core.Tests.Factories
{
    [UnitTest]
    public sealed class AzureServiceBusFactoryTests
    {
        [Fact]
        public async Task Create_ReturnsServiceBusClientSender_FromExisting()
        {
            // arrange
            var connectionString = "Endpoint=sb://sbn-postoffice.servicebus.windows.net/;SharedAccessKeyName=Hello;SharedAccessKey=there";
            var queueName = "test";

            var serviceBusClientFactory = new ServiceBusClientFactory(connectionString);
            await using var messageBusFactory = new AzureServiceBusFactory(serviceBusClientFactory);

            // act
            var actualAdd = messageBusFactory.GetSenderClient(queueName);

            var actualGet = messageBusFactory.GetSenderClient(queueName);

            // assert
            Assert.NotNull(actualAdd);
            Assert.NotNull(actualGet);
        }

        [Fact]
        public async Task Create_ReturnsServiceBusClientSessionReceiver()
        {
            // arrange
            var queueName = $"sbq-test";
            var replyQueue = $"sbq-test-reply";
            var serviceBusSenderMock = new Mock<ServiceBusSender>();
            var bytes = new DataBundleResponseContract
            {
                RequestId = "93764CCB-7474-4234-908B-C84E73F571F7",
                Success = new DataBundleResponseContract.Types.FileResource
                {
                    ContentUri = "http://localhost"
                }
            }.ToByteArray();

            var serviceBusReceivedMessage = MockedServiceBusReceivedMessage.Create(bytes);
            var serviceBusSessionReceiverMock = new Mock<ServiceBusSessionReceiver>();
            serviceBusSessionReceiverMock
                .Setup(x => x.ReceiveMessageAsync(It.IsAny<TimeSpan>(), default))
                .ReturnsAsync(serviceBusReceivedMessage);

            await using var serviceBusClient = new MockedServiceBusClient(
                queueName,
                replyQueue,
                serviceBusSenderMock.Object,
                serviceBusSessionReceiverMock.Object);

            var serviceBusClientFactory = new Mock<IServiceBusClientFactory>();
            serviceBusClientFactory
                .Setup(x => x.Create())
                .Returns(serviceBusClient);

            await using var messageBusFactory = new AzureServiceBusFactory(serviceBusClientFactory.Object);

            // act
            var actualAdd = await messageBusFactory.GetSessionReceiverClientAsync(replyQueue, It.IsAny<string>()).ConfigureAwait(false);

            // assert
            Assert.NotNull(actualAdd);
        }

        [Fact]
        public void Returns_SessionClientArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => new AzureServiceBusFactory(null!));
        }
    }
}
