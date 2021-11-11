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
using Energinet.DataHub.MessageHub.Core.Peek;
using Energinet.DataHub.MessageHub.Model.Model;
using Energinet.DataHub.MessageHub.Model.Peek;
using Energinet.DataHub.MessageHub.Model.Protobuf;
using Google.Protobuf;
using Moq;
using Xunit;
using Xunit.Categories;

namespace Energinet.DataHub.MessageHub.Core.Tests.Peek
{
    [UnitTest]
    public class DataBundleRequestSenderTests
    {
        private static readonly PeekRequestConfig _peekRequestConfig = new(
            $"sbq-{DomainOrigin.TimeSeries}",
            $"sbq-{DomainOrigin.TimeSeries}-reply",
            $"sbq-{DomainOrigin.Charges}",
            $"sbq-{DomainOrigin.Charges}-reply",
            $"sbq-{DomainOrigin.MarketRoles}",
            $"sbq-{DomainOrigin.MarketRoles}-reply",
            $"sbq-{DomainOrigin.MeteringPoints}",
            $"sbq-{DomainOrigin.MeteringPoints}-reply",
            $"sbq-{DomainOrigin.Aggregations}",
            $"sbq-{DomainOrigin.Aggregations}-reply");

        [Fact]
        public async Task Send_DtoIsNull_ThrowsArgumentNullException()
        {
            // arrange
            var requestBundleParserMock = new Mock<IRequestBundleParser>();
            var responseBundleParserMock = new Mock<IResponseBundleParser>();
            var messageBusFactory = new Mock<IMessageBusFactory>();
            var target = new DataBundleRequestSender(
                requestBundleParserMock.Object,
                responseBundleParserMock.Object,
                messageBusFactory.Object,
                _peekRequestConfig);

            // act, assert
            await Assert.ThrowsAsync<ArgumentNullException>(() => target.SendAsync(null!, DomainOrigin.Aggregations)).ConfigureAwait(false);
        }

        [Theory]
        [InlineData(DomainOrigin.Aggregations)]
        [InlineData(DomainOrigin.Charges)]
        [InlineData(DomainOrigin.MarketRoles)]
        [InlineData(DomainOrigin.MeteringPoints)]
        [InlineData(DomainOrigin.TimeSeries)]
        public async Task Send_AllIsOk_ReturnsResponse(DomainOrigin domainOrigin)
        {
            // arrange
            var queue = $"sbq-{domainOrigin}";
            var replyQueue = $"sbq-{domainOrigin}-reply";
            var serviceBusSenderMock = new Mock<ServiceBusSender>();
            var requestBundleResponse = new DataBundleResponseContract
            {
                RequestId = "B679EFB9-70E9-4BC8-8C79-EAA9918C83C8",
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

            var messageBusFactory = new Mock<IMessageBusFactory>();
            messageBusFactory
                .Setup(x => x.GetSenderClient(queue))
                .Returns(AzureSenderServiceBus.Wrap(serviceBusClient.CreateSender(queue)));

            await using var sessionReceiver = await serviceBusClient.AcceptSessionAsync(replyQueue, It.IsAny<string>()).ConfigureAwait(false);
            await using var azureSessionReceiver = AzureSessionReceiverServiceBus.Wrap(sessionReceiver);
            messageBusFactory
                .Setup(x => x.GetSessionReceiverClientAsync(replyQueue, It.IsAny<string>()))
                .ReturnsAsync(azureSessionReceiver);

            var target = new DataBundleRequestSender(
                new RequestBundleParser(),
                new ResponseBundleParser(),
                messageBusFactory.Object,
                _peekRequestConfig);

            // act
            var result = await target.SendAsync(
                    new DataBundleRequestDto(
                        new Guid("B679EFB9-70E9-4BC8-8C79-EAA9918C83C8"),
                        "80BB9BB8-CDE8-4C77-BE76-FDC886FD75A3",
                        "message_type",
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

            var messageBusFactory = new Mock<IMessageBusFactory>();
            messageBusFactory
                .Setup(x => x.GetSenderClient(queue))
                .Returns(AzureSenderServiceBus.Wrap(serviceBusClient.CreateSender(queue)));

            await using var sessionReceiver = await serviceBusClient.AcceptSessionAsync(replyQueue, It.IsAny<string>()).ConfigureAwait(false);
            await using var azureSessionReceiver = AzureSessionReceiverServiceBus.Wrap(sessionReceiver);
            messageBusFactory
                .Setup(x => x.GetSessionReceiverClientAsync(replyQueue, It.IsAny<string>()))
                .ReturnsAsync(azureSessionReceiver);

            var target = new DataBundleRequestSender(
                new RequestBundleParser(),
                new ResponseBundleParser(),
                messageBusFactory.Object,
                _peekRequestConfig);

            // act
            var result = await target.SendAsync(
                    new DataBundleRequestDto(
                        Guid.NewGuid(),
                        "80BB9BB8-CDE8-4C77-BE76-FDC886FD75A3",
                        "message_type",
                        new[] { Guid.NewGuid(), Guid.NewGuid() }),
                    domainOrigin)
                .ConfigureAwait(false);

            // assert
            Assert.Null(result);
        }

        [Fact]
        public async Task Send_AllIsOk_AddsCorrectIntegrationEvents()
        {
            // arrange
            const DomainOrigin domainOrigin = DomainOrigin.Charges;
            var queue = $"sbq-{domainOrigin}";
            var replyQueue = $"sbq-{domainOrigin}-reply";
            var serviceBusSenderMock = new Mock<ServiceBusSender>();
            var requestBundleResponse = new DataBundleResponseContract
            {
                RequestId = "C163828E-08C0-4D97-93A3-B647B2B657FB",
                Success = new DataBundleResponseContract.Types.FileResource
                {
                    ContentUri = "http://localhost",
                    DataAvailableNotificationIds =
                    {
                        new[] { "A8A6EAA8-DAF3-4E82-910F-A30260CEFDC5" }
                    }
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

            var messageBusFactory = new Mock<IMessageBusFactory>();
            messageBusFactory
                .Setup(x => x.GetSenderClient(queue))
                .Returns(AzureSenderServiceBus.Wrap(serviceBusClient.CreateSender(queue)));

            await using var sessionReceiver = await serviceBusClient.AcceptSessionAsync(replyQueue, It.IsAny<string>()).ConfigureAwait(false);
            await using var azureSessionReceiver = AzureSessionReceiverServiceBus.Wrap(sessionReceiver);
            messageBusFactory
                .Setup(x => x.GetSessionReceiverClientAsync(replyQueue, It.IsAny<string>()))
                .ReturnsAsync(azureSessionReceiver);

            var target = new DataBundleRequestSender(
                new RequestBundleParser(),
                new ResponseBundleParser(),
                messageBusFactory.Object,
                _peekRequestConfig);

            // act
            await target.SendAsync(
                    new DataBundleRequestDto(
                        new Guid("C163828E-08C0-4D97-93A3-B647B2B657FB"),
                        "80BB9BB8-CDE8-4C77-BE76-FDC886FD75A3",
                        "message_type",
                        new[] { Guid.NewGuid(), Guid.NewGuid() }),
                    domainOrigin)
                .ConfigureAwait(false);

            // assert
            serviceBusSenderMock.Verify(
                x => x.SendMessageAsync(
                    It.Is<ServiceBusMessage>(
                        message =>
                            message.ApplicationProperties.ContainsKey("OperationTimestamp")
                            && message.ApplicationProperties.ContainsKey("OperationCorrelationId")
                            && message.ApplicationProperties.ContainsKey("MessageVersion")
                            && message.ApplicationProperties.ContainsKey("MessageType")
                            && message.ApplicationProperties.ContainsKey("EventIdentification")),
                    default),
                Times.Once);
        }
    }
}
