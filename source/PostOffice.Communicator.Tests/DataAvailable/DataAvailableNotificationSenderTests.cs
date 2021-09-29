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
using GreenEnergyHub.PostOffice.Communicator.DataAvailable;
using GreenEnergyHub.PostOffice.Communicator.Factories;
using GreenEnergyHub.PostOffice.Communicator.Model;
using Moq;
using Xunit;
using Xunit.Categories;

namespace PostOffice.Communicator.Tests.DataAvailable
{
    [UnitTest]
    public sealed class DataAvailableNotificationSenderTests
    {
        [Fact]
        public async Task SendAsync_NullArgument_ThrowsException()
        {
            // Arrange
            var serviceBusClientFactory = new Mock<IServiceBusClientFactory>();
            await using var target = new DataAvailableNotificationSender(serviceBusClientFactory.Object);

            // Act + Assert
            await Assert.ThrowsAsync<ArgumentNullException>(() => target.SendAsync(null!)).ConfigureAwait(false);
        }

        [Fact]
        public async Task SendAsync_ValidInput_SendsMessage()
        {
            // Arrange
            var serviceBusSenderMock = new Mock<ServiceBusSender>();
            var serviceBusSessionReceiverMock = new Mock<ServiceBusSessionReceiver>();

            await using var mockedServiceBusClient = new MockedServiceBusClient(
                "sbq-dataavailable",
                string.Empty,
                serviceBusSenderMock.Object,
                serviceBusSessionReceiverMock.Object);

            var serviceBusClientFactory = new Mock<IServiceBusClientFactory>();
            serviceBusClientFactory.Setup(x => x.Create()).Returns(mockedServiceBusClient);

            await using var target = new DataAvailableNotificationSender(serviceBusClientFactory.Object);

            var dataAvailable = new DataAvailableNotificationDto(
                "F9A5115D-44EB-4AD4-BC7E-E8E8A0BC425E",
                "fake_value",
                "fake_value",
                "fake_value",
                true,
                1);

            // Act
            await target.SendAsync(dataAvailable).ConfigureAwait(false);

            // Assert
            serviceBusSenderMock.Verify(x => x.SendMessageAsync(It.IsAny<ServiceBusMessage>(), default), Times.Once);
        }
    }
}
