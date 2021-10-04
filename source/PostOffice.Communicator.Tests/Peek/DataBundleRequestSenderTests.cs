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
using GreenEnergyHub.PostOffice.Communicator.Factories;
using GreenEnergyHub.PostOffice.Communicator.Model;
using GreenEnergyHub.PostOffice.Communicator.Peek;
using Moq;
using Xunit;
using Xunit.Categories;

namespace PostOffice.Communicator.Tests.Peek
{
    [UnitTest]
    public class DataBundleRequestSenderTests
    {
        [Fact]
        public async Task Send_DtoIsNull_ThrowsArgumentNullException()
        {
            // arrange
            var requestBundleParserMock = new Mock<IRequestBundleParser>();
            var responseBundleParserMock = new Mock<IResponseBundleParser>();
            var serviceBusClientFactoryMock = new Mock<IServiceBusClientFactory>();
            await using var target = new DataBundleRequestSender(
                requestBundleParserMock.Object,
                responseBundleParserMock.Object,
                serviceBusClientFactoryMock.Object);

            // act, assert
            await Assert.ThrowsAsync<ArgumentNullException>(() => target.SendAsync(null!, DomainOrigin.Aggregations)).ConfigureAwait(false);
        }

        [Fact]
        public async Task Send_AllIsOk_ReturnsResponse()
        {
            // arrange
            const DomainOrigin domainOrigin = DomainOrigin.Charges;
            var queue = $"sbq-{domainOrigin}";
            var replyQueue = $"sbq-{domainOrigin}-reply";
            var serviceBusSenderMock = new Mock<ServiceBusSender>();
            var requestBundleResponse = new DataBundleResponseContract
            {
                Success = new DataBundleResponseContract.Types.FileResource
                    {
                        ContentUri = "http://localhost",
                        DataAvailableNotificationIds = { new[] { "A8A6EAA8-DAF3-4E82-910F-A30260CEFDC5" } }
                    }
            };
            var bytes = requestBundleResponse.ToByteArray();

            var serviceBusReceivedMessage = MockedServiceBusReceivedMessage.Create(bytes);
            var serviceBusSessionReceiverMock = new Mock<ServiceBusSessionReceiver>();
            serviceBusSessionReceiverMock
                .Setup(x => x.ReceiveMessageAsync(It.IsAny<TimeSpan>(), default))
                .ReturnsAsync(serviceBusReceivedMessage);

            await using var serviceBusClient = new MockedServiceBusClient(
                queue,
                replyQueue,
                serviceBusSenderMock.Object,
                serviceBusSessionReceiverMock.Object);

            var serviceBusClientFactoryMock = new Mock<IServiceBusClientFactory>();
            serviceBusClientFactoryMock
                .Setup(x => x.Create())
                .Returns(serviceBusClient);

            await using var target = new DataBundleRequestSender(
                new RequestBundleParser(),
                new ResponseBundleParser(),
                serviceBusClientFactoryMock.Object);

            // act
            var result = await target.SendAsync(
                    new DataBundleRequestDto(
                        "80BB9BB8-CDE8-4C77-BE76-FDC886FD75A3",
                        new[] { Guid.NewGuid(), Guid.NewGuid() }),
                    domainOrigin)
                .ConfigureAwait(false);

            // assert
            Assert.NotNull(result);
            Assert.Equal(new Uri(requestBundleResponse.Success.ContentUri), result.ContentUri);
        }

        [Fact]
        public async Task Send_BusReturnsNull_ReturnsNull()
        {
            // arrange
            const DomainOrigin domainOrigin = DomainOrigin.Charges;
            var queue = $"sbq-{domainOrigin}";
            var replyQueue = $"sbq-{domainOrigin}-reply";
            var serviceBusSenderMock = new Mock<ServiceBusSender>();

            var serviceBusSessionReceiverMock = new Mock<ServiceBusSessionReceiver>();
            serviceBusSessionReceiverMock
                .Setup(x => x.ReceiveMessageAsync(It.IsAny<TimeSpan>(), default))
                .ReturnsAsync((ServiceBusReceivedMessage)null);

            await using var serviceBusClient = new MockedServiceBusClient(
                queue,
                replyQueue,
                serviceBusSenderMock.Object,
                serviceBusSessionReceiverMock.Object);

            var serviceBusClientFactoryMock = new Mock<IServiceBusClientFactory>();
            serviceBusClientFactoryMock
                .Setup(x => x.Create())
                .Returns(serviceBusClient);

            await using var target = new DataBundleRequestSender(
                new RequestBundleParser(),
                new ResponseBundleParser(),
                serviceBusClientFactoryMock.Object);

            // act
            var result = await target.SendAsync(
                    new DataBundleRequestDto(
                        "80BB9BB8-CDE8-4C77-BE76-FDC886FD75A3",
                        new[] { Guid.NewGuid(), Guid.NewGuid() }),
                    domainOrigin)
                .ConfigureAwait(false);

            // assert
            Assert.Null(result);
        }
    }
}
