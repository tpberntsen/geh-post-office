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
using System.Collections.Generic;
using System.Threading.Tasks;
using Azure.Messaging.ServiceBus;
using Energinet.DataHub.MessageHub.Client.Factories;
using Energinet.DataHub.MessageHub.Client.Peek;
using Energinet.DataHub.MessageHub.Model.Extensions;
using Energinet.DataHub.MessageHub.Model.Model;
using Energinet.DataHub.MessageHub.Model.Peek;
using Moq;
using Xunit;
using Xunit.Categories;

namespace Energinet.DataHub.MessageHub.Client.Tests.Peek
{
    [UnitTest]
    public sealed class DataBundleResponseSenderTests
    {
        [Fact]
        public async Task SendAsync_NullDto_ThrowsException()
        {
            // Arrange
            var serviceBusSenderMock = new Mock<ServiceBusSender>();
            var serviceBusSessionReceiverMock = new Mock<ServiceBusSessionReceiver>();

            await using var mockedServiceBusClient = new MockedServiceBusClient(
                string.Empty,
                string.Empty,
                serviceBusSenderMock.Object,
                serviceBusSessionReceiverMock.Object);

            var serviceBusClientFactory = new Mock<IServiceBusClientFactory>();
            serviceBusClientFactory.Setup(x => x.Create()).Returns(mockedServiceBusClient);
            await using var messageBusFactory = new AzureServiceBusFactory(serviceBusClientFactory.Object);

            var config = new MessageHubConfig("fake_value", "fake_value");
            var target = new DataBundleResponseSender(
                new ResponseBundleParser(),
                messageBusFactory,
                config);

            // Act + Assert
            await Assert
                .ThrowsAsync<ArgumentNullException>(() => target.SendAsync(null!))
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
            await using var messageBusFactory = new AzureServiceBusFactory(serviceBusClientFactory.Object);

            var config = new MessageHubConfig("fake_value", queueName);

            var target = new DataBundleResponseSender(
                new ResponseBundleParser(),
                messageBusFactory,
                config);

            var requestMock = new DataBundleRequestDto(
                Guid.NewGuid(),
                "7E9D048D-F0D8-476D-8739-AAA83284C9C6",
                "80BB9BB8-CDE8-4C77-BE76-FDC886FD75A3");

            var response = requestMock.CreateResponse(new Uri("https://test.dk/test"), new List<Guid>());

            // Act
            await target.SendAsync(response).ConfigureAwait(false);

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
            await using var messageBusFactory = new AzureServiceBusFactory(serviceBusClientFactory.Object);

            var config = new MessageHubConfig("fake_value", sbqTimeseriesReply);

            // ServiceBusMessage
            var target = new DataBundleResponseSender(
                new ResponseBundleParser(),
                messageBusFactory,
                config);

            var requestMock = new DataBundleRequestDto(
                Guid.NewGuid(),
                "42D509CB-1D93-430D-A2D4-7DBB9AE56771",
                "80BB9BB8-CDE8-4C77-BE76-FDC886FD75A3");

            var response = requestMock.CreateResponse(new Uri("https://test.dk/test"), new[] { Guid.NewGuid(), Guid.NewGuid() });

            // Act
            await target.SendAsync(response).ConfigureAwait(false);

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
