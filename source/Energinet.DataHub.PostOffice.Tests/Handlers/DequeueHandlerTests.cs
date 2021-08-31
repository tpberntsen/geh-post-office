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
using System.Threading;
using System.Threading.Tasks;
using Energinet.DataHub.PostOffice.Application.Commands;
using Energinet.DataHub.PostOffice.Application.Handlers;
using Energinet.DataHub.PostOffice.Domain.Model;
using Energinet.DataHub.PostOffice.Domain.Services;
using Moq;
using Xunit;
using Xunit.Categories;

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
            var target = new DequeueHandler(warehouseDomainServiceMock.Object);

            // Act + Assert
            await Assert
                .ThrowsAsync<ArgumentNullException>(() => target.Handle(null!, CancellationToken.None))
                .ConfigureAwait(false);
        }

        [Fact]
        public async Task Handle_WithData_ReturnsTrue()
        {
            // Arrange
            var request = new DequeueCommand("fake_value", "fake_value");

            var warehouseDomainServiceMock = new Mock<IMarketOperatorDataDomainService>();
            warehouseDomainServiceMock.Setup(x => x.TryAcknowledgeAsync(
                    It.Is<MarketOperator>(r => string.Equals(r.Value, request.Recipient, StringComparison.OrdinalIgnoreCase)),
                    It.Is<Uuid>(id => string.Equals(id.Value, request.BundleUuid, StringComparison.OrdinalIgnoreCase))))
                .ReturnsAsync(true);

            var target = new DequeueHandler(warehouseDomainServiceMock.Object);

            // Act
            var response = await target.Handle(request, CancellationToken.None).ConfigureAwait(false);

            // Assert
            Assert.NotNull(response);
            Assert.True(response.IsDequeued);
        }

        [Fact]
        public async Task Handle_WithoutData_ReturnsFalse()
        {
            // Arrange
            var request = new DequeueCommand("fake_value", "fake_value");

            var warehouseDomainServiceMock = new Mock<IMarketOperatorDataDomainService>();
            warehouseDomainServiceMock.Setup(x => x.TryAcknowledgeAsync(
                    It.Is<MarketOperator>(r => string.Equals(r.Value, request.Recipient, StringComparison.OrdinalIgnoreCase)),
                    It.Is<Uuid>(id => string.Equals(id.Value, request.BundleUuid, StringComparison.OrdinalIgnoreCase))))
                .ReturnsAsync(false);

            var target = new DequeueHandler(warehouseDomainServiceMock.Object);

            // Act
            var response = await target.Handle(request, CancellationToken.None).ConfigureAwait(false);

            // Assert
            Assert.NotNull(response);
            Assert.False(response.IsDequeued);
        }
    }
}
