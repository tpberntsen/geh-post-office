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
using Energinet.DataHub.MessageHub.Client.Factories;
using Energinet.DataHub.MessageHub.Client.Model;
using Energinet.DataHub.MessageHub.Client.Peek;
using Moq;
using Xunit;
using Xunit.Categories;
using static System.Guid;

namespace Energinet.DataHub.MessageHub.Client.Tests.Peek
{
    [UnitTest]
    public sealed class DataBundleResponseSenderTests
    {
        [Fact]
        public async Task SendAsync_NullDto_ThrowsException()
        {
            // Arrange
            var serviceBusClientFactory = new Mock<IServiceBusClientFactory>();
            await using var target = new DataBundleResponseSender(
                new ResponseBundleParser(),
                serviceBusClientFactory.Object);
            var requestMock = new DataBundleRequestDto(
                "80BB9BB8-CDE8-4C77-BE76-FDC886FD75A3",
                new[] { Guid.NewGuid(), Guid.NewGuid() });

            // Act + Assert
            await Assert
                .ThrowsAsync<ArgumentNullException>(() =>
                    target.SendAsync(
                        null!,
                        requestMock,
                        "sessionId",
                        DomainOrigin.TimeSeries))
                .ConfigureAwait(false);
        }

        [Fact]
        public async Task SendAsync_NullSessionId_ThrowsException()
        {
            // Arrange
            var serviceBusClientFactory = new Mock<IServiceBusClientFactory>();
            await using var target = new DataBundleResponseSender(
                new ResponseBundleParser(),
                serviceBusClientFactory.Object);

            var response = new RequestDataBundleResponseDto(
                new Uri("https://test.dk/test"),
                new[] { NewGuid(), NewGuid() });

            var requestMock = new DataBundleRequestDto(
                "80BB9BB8-CDE8-4C77-BE76-FDC886FD75A3",
                new[] { Guid.NewGuid(), Guid.NewGuid() });

            // Act + Assert
            await Assert
                .ThrowsAsync<ArgumentNullException>(() =>
                    target.SendAsync(
                        response,
                        requestMock,
                        null!,
                        DomainOrigin.TimeSeries))
                .ConfigureAwait(false);
        }

        [Fact]
        public async Task SendAsync_NullRequestDto_ThrowsException()
        {
            // Arrange
            var serviceBusClientFactory = new Mock<IServiceBusClientFactory>();
            await using var target = new DataBundleResponseSender(
                new ResponseBundleParser(),
                serviceBusClientFactory.Object);

            var response = new RequestDataBundleResponseDto(
                new Uri("https://test.dk/test"),
                new[] { NewGuid(), NewGuid() });

            // Act + Assert
            await Assert
                .ThrowsAsync<ArgumentNullException>(() =>
                    target.SendAsync(
                        response,
                        null!,
                        NewGuid().ToString(),
                        DomainOrigin.TimeSeries))
                .ConfigureAwait(false);
        }

        [Theory]
        [InlineData(DomainOrigin.TimeSeries, "sbq-TimeSeries-reply")]
        [InlineData(DomainOrigin.Charges, "sbq-Charges-reply")]
        [InlineData(DomainOrigin.Aggregations, "sbq-Aggregations-reply")]
        [InlineData(DomainOrigin.MarketRoles, "sbq-MarketRoles-reply")]
        [InlineData(DomainOrigin.MeteringPoints, "sbq-MeteringPoints-reply")]
        public async Task SendAsync_ValidInput_SendsMessage(DomainOrigin domainOrigin, string queueName)
        {
            // Arrange
            var serviceBusSenderMock = new Mock<ServiceBusSender>();
            var serviceBusSessionReceiverMock = new Mock<ServiceBusSessionReceiver>();

            await using var mockedServiceBusClient = new MockedServiceBusClient(
                queueName,
                string.Empty,
                serviceBusSenderMock.Object,
                serviceBusSessionReceiverMock.Object);

            var serviceBusClientFactory = new Mock<IServiceBusClientFactory>();
            serviceBusClientFactory.Setup(x => x.Create()).Returns(mockedServiceBusClient);

            await using var target = new DataBundleResponseSender(
                new ResponseBundleParser(),
                serviceBusClientFactory.Object);

            var response = new RequestDataBundleResponseDto(
                new Uri("https://test.dk/test"),
                new[] { NewGuid(), NewGuid() });

            var requestMock = new DataBundleRequestDto(
                "80BB9BB8-CDE8-4C77-BE76-FDC886FD75A3",
                new[] { Guid.NewGuid(), Guid.NewGuid() });

            // Act
            await target.SendAsync(
                response,
                requestMock,
                "session",
                domainOrigin)
            .ConfigureAwait(false);

            // Assert
            serviceBusSenderMock.Verify(x => x.SendMessageAsync(It.IsAny<ServiceBusMessage>(), default), Times.Once);
        }

        [Fact]
        public async Task SendAsync_ValidInput_AddsCorrectIntegrationEvents()
        {
            // Arrange
            var serviceBusSenderMock = new Mock<ServiceBusSender>();
            var serviceBusSessionReceiverMock = new Mock<ServiceBusSessionReceiver>();

            await using var mockedServiceBusClient = new MockedServiceBusClient(
                "sbq-TimeSeries-reply",
                string.Empty,
                serviceBusSenderMock.Object,
                serviceBusSessionReceiverMock.Object);

            var serviceBusClientFactory = new Mock<IServiceBusClientFactory>();
            serviceBusClientFactory.Setup(x => x.Create()).Returns(mockedServiceBusClient);

            // var applicationPropertiesMock = new Dictionary<string, string>();
            // var serviceBusMessageMock = new Mock<ServiceBusMessage>();
            // ServiceBusMessage
            await using var target = new DataBundleResponseSender(
                new ResponseBundleParser(),
                serviceBusClientFactory.Object);

            var response = new RequestDataBundleResponseDto(
                new Uri("https://test.dk/test"),
                new[] { NewGuid(), NewGuid() });

            var requestMock = new DataBundleRequestDto(
                "80BB9BB8-CDE8-4C77-BE76-FDC886FD75A3",
                new[] { Guid.NewGuid(), Guid.NewGuid() });

            // Act
            await target.SendAsync(
                    response,
                    requestMock,
                    "session",
                    DomainOrigin.TimeSeries)
                .ConfigureAwait(false);

            // Assert
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
