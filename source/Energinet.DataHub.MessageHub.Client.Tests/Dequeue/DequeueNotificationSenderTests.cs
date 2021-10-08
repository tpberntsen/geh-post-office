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
using Energinet.DataHub.MessageHub.Client.Dequeue;
using Energinet.DataHub.MessageHub.Client.Factories;
using Energinet.DataHub.MessageHub.Client.Model;
using Moq;
using Xunit;
using Xunit.Categories;

namespace Energinet.DataHub.MessageHub.Client.Tests.Dequeue
{
    [UnitTest]
    public class DequeueNotificationSenderTests
    {
        [Fact]
        public async Task SendAsync_NullArgument_ThrowsException()
        {
            // Arrange
            var serviceBusClientFactory = new Mock<IServiceBusClientFactory>();
            await using var target = new DequeueNotificationSender(serviceBusClientFactory.Object);

            // Act + Assert
            await Assert
                .ThrowsAsync<ArgumentNullException>(() => target.SendAsync(null!, DomainOrigin.TimeSeries))
                .ConfigureAwait(false);
        }

        [Theory]
        [InlineData(DomainOrigin.TimeSeries, "sbq-TimeSeries-dequeue")]
        [InlineData(DomainOrigin.Charges, "sbq-Charges-dequeue")]
        [InlineData(DomainOrigin.Aggregations, "sbq-Aggregations-dequeue")]
        [InlineData(DomainOrigin.MarketRoles, "sbq-MarketRoles-dequeue")]
        [InlineData(DomainOrigin.MeteringPoints, "sbq-MeteringPoints-dequeue")]
        public async Task SendAsync_ValidInputForDomain_SendsMessage(DomainOrigin domainOrigin, string queueName)
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

            await using var target = new DequeueNotificationSender(serviceBusClientFactory.Object);

            var dataAvailable = new DequeueNotificationDto(
                new[] { Guid.NewGuid(), Guid.NewGuid() },
                new GlobalLocationNumberDto("fake_value"));

            // Act
            await target.SendAsync(dataAvailable, domainOrigin).ConfigureAwait(false);

            // Assert
            serviceBusSenderMock.Verify(x => x.SendMessageAsync(It.IsAny<ServiceBusMessage>(), default), Times.Once);
        }
    }
}
