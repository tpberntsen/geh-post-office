﻿// Copyright 2020 Energinet DataHub A/S
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
using Energinet.DataHub.MessageHub.Client.DataAvailable;
using Energinet.DataHub.MessageHub.Client.Factories;
using Energinet.DataHub.MessageHub.Model.Model;
using Moq;
using Xunit;
using Xunit.Categories;

namespace Energinet.DataHub.MessageHub.Client.Tests.DataAvailable
{
    [UnitTest]
    public sealed class DataAvailableNotificationSenderTests
    {
        [Fact]
        public async Task SendAsync_NullNotification_ThrowsException()
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
            var target = new DataAvailableNotificationSender(messageBusFactory, config);

            // Act + Assert
            await Assert
                .ThrowsAsync<ArgumentNullException>(() => target.SendAsync("F9A5115D-44EB-4AD4-BC7E-E8E8A0BC425E", null!))
                .ConfigureAwait(false);
        }

        [Fact]
        public async Task SendAsync_NullCorrelationId_ThrowsException()
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
            var target = new DataAvailableNotificationSender(messageBusFactory, config);

            var dataAvailable = new DataAvailableNotificationDto(
                Guid.Parse("F9A5115D-44EB-4AD4-BC7E-E8E8A0BC425E"),
                new GlobalLocationNumberDto("fake_value"),
                new MessageTypeDto("fake_value"),
                DomainOrigin.TimeSeries,
                true,
                1,
                "RSM??");

            // Act + Assert
            await Assert
                .ThrowsAsync<ArgumentNullException>(() => target.SendAsync(null!, dataAvailable))
                .ConfigureAwait(false);
        }

        [Fact]
        public async Task SendAsync_ValidInput_SendsMessage()
        {
            // Arrange
            var serviceBusSenderMock = new Mock<ServiceBusSender>();
            var serviceBusSessionReceiverMock = new Mock<ServiceBusSessionReceiver>();
            const string dataAvailableQueue = "sbq-dataavailable";

            await using var mockedServiceBusClient = new MockedServiceBusClient(
                dataAvailableQueue,
                string.Empty,
                serviceBusSenderMock.Object,
                serviceBusSessionReceiverMock.Object);

            var serviceBusClientFactory = new Mock<IServiceBusClientFactory>();
            serviceBusClientFactory.Setup(x => x.Create()).Returns(mockedServiceBusClient);
            await using var messageBusFactory = new AzureServiceBusFactory(serviceBusClientFactory.Object);

            var config = new MessageHubConfig(dataAvailableQueue, "fake_value");
            var target = new DataAvailableNotificationSender(messageBusFactory, config);

            var dataAvailable = new DataAvailableNotificationDto(
                Guid.Parse("F9A5115D-44EB-4AD4-BC7E-E8E8A0BC425E"),
                new GlobalLocationNumberDto("fake_value"),
                new MessageTypeDto("fake_value"),
                DomainOrigin.TimeSeries,
                true,
                1,
                "RSM??");

            // Act
            await target.SendAsync("F9A5115D-44EB-4AD4-BC7E-E8E8A0BC425E", dataAvailable).ConfigureAwait(false);

            // Assert
            serviceBusSenderMock.Verify(x => x.SendMessageAsync(It.IsAny<ServiceBusMessage>(), default), Times.Once);
        }

        [Fact]
        public async Task SendAsync_ValidInput_AddsCorrectIntegrationEvents()
        {
            // Arrange
            var serviceBusSenderMock = new Mock<ServiceBusSender>();
            var serviceBusSessionReceiverMock = new Mock<ServiceBusSessionReceiver>();
            const string dataAvailableQueue = "sbq-dataavailable";

            await using var mockedServiceBusClient = new MockedServiceBusClient(
                dataAvailableQueue,
                string.Empty,
                serviceBusSenderMock.Object,
                serviceBusSessionReceiverMock.Object);

            var serviceBusClientFactory = new Mock<IServiceBusClientFactory>();
            serviceBusClientFactory.Setup(x => x.Create()).Returns(mockedServiceBusClient);
            await using var messageBusFactory = new AzureServiceBusFactory(serviceBusClientFactory.Object);
            var config = new MessageHubConfig(dataAvailableQueue, "fake_value");

            var target = new DataAvailableNotificationSender(messageBusFactory, config);

            var dataAvailable = new DataAvailableNotificationDto(
                Guid.Parse("F9A5115D-44EB-4AD4-BC7E-E8E8A0BC425E"),
                new GlobalLocationNumberDto("fake_value"),
                new MessageTypeDto("fake_value"),
                DomainOrigin.TimeSeries,
                true,
                1,
                "RSM??");

            // Act
            await target.SendAsync("F9A5115D-44EB-4AD4-BC7E-E8E8A0BC425E", dataAvailable).ConfigureAwait(false);

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
