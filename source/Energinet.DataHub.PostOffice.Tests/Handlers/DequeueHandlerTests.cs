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
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Energinet.DataHub.PostOffice.Application.Commands;
using Energinet.DataHub.PostOffice.Application.Handlers;
using Energinet.DataHub.PostOffice.Domain.Model;
using Energinet.DataHub.PostOffice.Domain.Services;
using GreenEnergyHub.PostOffice.Communicator.Dequeue;
using GreenEnergyHub.PostOffice.Communicator.Model;
using Moq;
using Xunit;
using Xunit.Categories;
using DomainOrigin = Energinet.DataHub.PostOffice.Domain.Model.DomainOrigin;

namespace Energinet.DataHub.PostOffice.Tests.Handlers
{
    [UnitTest]
    public sealed class DequeueHandlerTests
    {
        [Fact]
        public async Task Handle_NullArgument_ThrowsException()
        {
            // Arrange
            var warehouseDomainServiceMock = new Mock<IMarketOperatorDataDomainService>();
            var dequeueNotificationSenderMock = new Mock<IDequeueNotificationSender>();
            var target = new DequeueHandler(warehouseDomainServiceMock.Object, dequeueNotificationSenderMock.Object);

            // Act + Assert
            await Assert
                .ThrowsAsync<ArgumentNullException>(() => target.Handle(null!, CancellationToken.None))
                .ConfigureAwait(false);
        }

        [Fact]
        public async Task Handle_WithData_ReturnsTrue()
        {
            // Arrange
            var request = new DequeueCommand("fake_value", "9FB4753A-0E2C-4F42-BA10-D38128DDA877");
            var bundleContentMock = new Mock<IBundleContent>();
            bundleContentMock
                .Setup(x => x.OpenAsync())
                .ReturnsAsync(() => new MemoryStream(new byte[] { 1, 2, 3 }));

            var bundle = new Bundle(
                new Uuid(Guid.NewGuid()),
                DomainOrigin.TimeSeries,
                new MarketOperator(new GlobalLocationNumber("fake_value")),
                Array.Empty<Uuid>(),
                bundleContentMock.Object);

            var warehouseDomainServiceMock = new Mock<IMarketOperatorDataDomainService>();
            warehouseDomainServiceMock.Setup(x => x.TryAcknowledgeAsync(
                    It.Is<MarketOperator>(r => string.Equals(r.Gln.Value, request.Recipient, StringComparison.OrdinalIgnoreCase)),
                    It.Is<Uuid>(id => string.Equals(id.ToString(), request.BundleUuid, StringComparison.OrdinalIgnoreCase))))
                .ReturnsAsync((true, bundle));
            var dequeueNotificationSenderMock = new Mock<IDequeueNotificationSender>();
            dequeueNotificationSenderMock.Setup(x => x.SendAsync(
                It.IsAny<DequeueNotificationDto>(),
                It.IsAny<GreenEnergyHub.PostOffice.Communicator.Model.DomainOrigin>())).Returns(Task.CompletedTask);

            var target = new DequeueHandler(warehouseDomainServiceMock.Object, dequeueNotificationSenderMock.Object);

            // Act
            var response = await target.Handle(request, CancellationToken.None).ConfigureAwait(false);

            // Assert
            Assert.NotNull(response);
            Assert.True(response.IsDequeued);
            dequeueNotificationSenderMock.Verify(
                x => x.SendAsync(
                    It.IsAny<DequeueNotificationDto>(),
                    It.IsAny<GreenEnergyHub.PostOffice.Communicator.Model.DomainOrigin>()),
                Times.Once);
        }

        [Fact]
        public async Task Handle_WithoutData_ReturnsFalse()
        {
            // Arrange
            var request = new DequeueCommand("fake_value", "E3A22C4F-BA71-4BC0-9571-85F7F906D20D");

            var warehouseDomainServiceMock = new Mock<IMarketOperatorDataDomainService>();
            warehouseDomainServiceMock.Setup(x => x.TryAcknowledgeAsync(
                    It.Is<MarketOperator>(r => string.Equals(r.Gln.Value, request.Recipient, StringComparison.OrdinalIgnoreCase)),
                    It.Is<Uuid>(id => string.Equals(id.ToString(), request.BundleUuid, StringComparison.OrdinalIgnoreCase))))
                .ReturnsAsync((false, null));
            var dequeueNotificationSenderMock = new Mock<IDequeueNotificationSender>();
            var target = new DequeueHandler(warehouseDomainServiceMock.Object, dequeueNotificationSenderMock.Object);

            // Act
            var response = await target.Handle(request, CancellationToken.None).ConfigureAwait(false);

            // Assert
            Assert.NotNull(response);
            Assert.False(response.IsDequeued);
        }
    }
}
