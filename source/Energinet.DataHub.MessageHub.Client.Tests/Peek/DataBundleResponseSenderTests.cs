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
            var config = new DomainConfig("fake_value", "fake_value", "fake_value", "fake_value", "fake_value", "fake_value");
            await using var target = new DataBundleResponseSender(
                new ResponseBundleParser(),
                serviceBusClientFactory.Object,
                config);
            var requestMock = new DataBundleRequestDto(
                "80BB9BB8-CDE8-4C77-BE76-FDC886FD75A3",
                new[] { Guid.NewGuid(), Guid.NewGuid() });

            // Act + Assert
            await Assert
                .ThrowsAsync<ArgumentNullException>(() =>
                    target.SendAsync(
                        null!,
                        requestMock,
                        "sessionId"))
                .ConfigureAwait(false);
        }

        [Fact]
        public async Task SendAsync_NullSessionId_ThrowsException()
        {
            // Arrange
            var serviceBusClientFactory = new Mock<IServiceBusClientFactory>();
            var config = new DomainConfig("fake_value", "fake_value", "fake_value", "fake_value", "fake_value", "fake_value");
            await using var target = new DataBundleResponseSender(
                new ResponseBundleParser(),
                serviceBusClientFactory.Object,
                config);

            var response = new DataBundleResponseDto(
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
                        null!))
                .ConfigureAwait(false);
        }

        [Fact]
        public async Task SendAsync_NullRequestDto_ThrowsException()
        {
            // Arrange
            var serviceBusClientFactory = new Mock<IServiceBusClientFactory>();
            var config = new DomainConfig("fake_value", "fake_value", "fake_value", "fake_value", "fake_value", "fake_value");
            await using var target = new DataBundleResponseSender(
                new ResponseBundleParser(),
                serviceBusClientFactory.Object,
                config);

            var response = new DataBundleResponseDto(
                new Uri("https://test.dk/test"),
                new[] { NewGuid(), NewGuid() });

            // Act + Assert
            await Assert
                .ThrowsAsync<ArgumentNullException>(() =>
                    target.SendAsync(
                        response,
                        null!,
                        NewGuid().ToString()))
                .ConfigureAwait(false);
        }

        [Fact]
        public async Task SendAsync_ValidInput_SendsMessage()
        {
            // Arrange
            const string queueName = "sbq-MeteringPoints-reply";
            var serviceBusSenderMock = new Mock<ServiceBusSender>();
            var serviceBusSessionReceiverMock = new Mock<ServiceBusSessionReceiver>();

            await using var mockedServiceBusClient = new MockedServiceBusClient(
                queueName,
                string.Empty,
                serviceBusSenderMock.Object,
                serviceBusSessionReceiverMock.Object);

            var serviceBusClientFactory = new Mock<IServiceBusClientFactory>();
            serviceBusClientFactory.Setup(x => x.Create()).Returns(mockedServiceBusClient);

            var config = new DomainConfig("fake_value", queueName, "fake_value", "fake_value", "fake_value", "fake_value");

            await using var target = new DataBundleResponseSender(
                new ResponseBundleParser(),
                serviceBusClientFactory.Object,
                config);

            var response = new DataBundleResponseDto(
                new Uri("https://test.dk/test"),
                new[] { NewGuid(), NewGuid() });

            var requestMock = new DataBundleRequestDto(
                "80BB9BB8-CDE8-4C77-BE76-FDC886FD75A3",
                new[] { Guid.NewGuid(), Guid.NewGuid() });

            // Act
            await target.SendAsync(
                response,
                requestMock,
                "session")
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
            const string sbqTimeseriesReply = "sbq-TimeSeries-reply";

            await using var mockedServiceBusClient = new MockedServiceBusClient(
                sbqTimeseriesReply,
                string.Empty,
                serviceBusSenderMock.Object,
                serviceBusSessionReceiverMock.Object);

            var serviceBusClientFactory = new Mock<IServiceBusClientFactory>();
            serviceBusClientFactory.Setup(x => x.Create()).Returns(mockedServiceBusClient);

            var config = new DomainConfig("fake_value", sbqTimeseriesReply, "fake_value", "fake_value", "fake_value", "fake_value");

            // var applicationPropertiesMock = new Dictionary<string, string>();
            // var serviceBusMessageMock = new Mock<ServiceBusMessage>();
            // ServiceBusMessage
            await using var target = new DataBundleResponseSender(
                new ResponseBundleParser(),
                serviceBusClientFactory.Object,
                config);

            var response = new DataBundleResponseDto(
                new Uri("https://test.dk/test"),
                new[] { NewGuid(), NewGuid() });

            var requestMock = new DataBundleRequestDto(
                "80BB9BB8-CDE8-4C77-BE76-FDC886FD75A3",
                new[] { Guid.NewGuid(), Guid.NewGuid() });

            // Act
            await target.SendAsync(
                    response,
                    requestMock,
                    "session")
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
