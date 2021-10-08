// Copyright 2020 Energinet DataHub A/S
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
using Moq;
using Xunit;
using Xunit.Categories;

namespace Energinet.DataHub.PostOffice.Tests.Handlers
{
    [UnitTest]
    public sealed class PeekHandlerTests
    {
        [Fact]
        public async Task PeekCommandHandle_NullArgument_ThrowsException()
        {
            // Arrange
            var warehouseDomainServiceMock = new Mock<IMarketOperatorDataDomainService>();
            var target = new PeekHandler(warehouseDomainServiceMock.Object);

            // Act + Assert
            await Assert
                .ThrowsAsync<ArgumentNullException>(() => target.Handle((PeekCommand)null!, CancellationToken.None))
                .ConfigureAwait(false);
        }

        [Fact]
        public async Task PeekCommandHandle_WithData_ReturnsDataStream()
        {
            // Arrange
            var request = new PeekCommand("fake_value", Guid.NewGuid().ToString());

            var bundleContentMock = new Mock<IBundleContent>();
            bundleContentMock
                .Setup(x => x.OpenAsync())
                .ReturnsAsync(() => new MemoryStream(new byte[] { 1, 2, 3 }));

            var bundle = new Bundle(
                new Uuid(request.BundleId),
                DomainOrigin.TimeSeries,
                new MarketOperator(new GlobalLocationNumber("fake_value")),
                Array.Empty<Uuid>(),
                bundleContentMock.Object);

            var warehouseDomainServiceMock = new Mock<IMarketOperatorDataDomainService>();
            warehouseDomainServiceMock
                .Setup(x =>
                    x.GetNextUnacknowledgedAsync(
                        It.Is<MarketOperator>(r =>
                            string.Equals(r.Gln.Value, request.Recipient, StringComparison.OrdinalIgnoreCase)),
                        It.Is<Uuid>(r => BundleIdCheck(r, request))))
                .ReturnsAsync(bundle);

            var target = new PeekHandler(warehouseDomainServiceMock.Object);

            // Act
            var (hasContent, stream) = await target.Handle(request, CancellationToken.None).ConfigureAwait(false);

            // Assert
            Assert.True(hasContent);
            Assert.Equal(1, stream.ReadByte());
            Assert.Equal(2, stream.ReadByte());
            Assert.Equal(3, stream.ReadByte());
            await stream.DisposeAsync().ConfigureAwait(false);
        }

        [Fact]
        public async Task PeekCommandHandle_WithoutData_ReturnsNullStream()
        {
            // Arrange
            var request = new PeekCommand("fake_value", Guid.NewGuid().ToString());

            var warehouseDomainServiceMock = new Mock<IMarketOperatorDataDomainService>();
            var marketOperator = It.Is<MarketOperator>(r => string.Equals(r.Gln.Value, request.Recipient, StringComparison.OrdinalIgnoreCase));
            var bundleId = It.Is<Uuid>(r => string.Equals(r.ToString(), request.BundleId, StringComparison.OrdinalIgnoreCase));

            warehouseDomainServiceMock
                .Setup(x =>
                    x.GetNextUnacknowledgedAsync(
                        It.Is<MarketOperator>(r =>
                            string.Equals(r.Gln.Value, request.Recipient, StringComparison.OrdinalIgnoreCase)),
                        bundleId))
                .ReturnsAsync((Bundle?)null);

            var target = new PeekHandler(warehouseDomainServiceMock.Object);

            // Act
            var (hasContent, stream) = await target.Handle(request, CancellationToken.None).ConfigureAwait(false);

            // Assert
            Assert.False(hasContent);
            Assert.Equal(0, stream.Length);
            await stream.DisposeAsync().ConfigureAwait(false);
        }

        [Fact]
        public async Task PeekAggregationsOrTimeSeriesCommandHandle_NullArgument_ThrowsException()
        {
            // Arrange
            var warehouseDomainServiceMock = new Mock<IMarketOperatorDataDomainService>();
            var target = new PeekHandler(warehouseDomainServiceMock.Object);

            // Act + Assert
            await Assert
                .ThrowsAsync<ArgumentNullException>(() => target.Handle((PeekAggregationsOrTimeSeriesCommand)null!, CancellationToken.None))
                .ConfigureAwait(false);
        }

        [Fact]
        public async Task PeekAggregationsOrTimeSeriesCommandHandle_WithData_ReturnsDataStream()
        {
            // Arrange
            var request = new PeekAggregationsOrTimeSeriesCommand("fake_value", Guid.NewGuid().ToString());

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
            warehouseDomainServiceMock
                .Setup(x =>
                    x.GetNextUnacknowledgedAggregationsOrTimeSeriesAsync(
                        It.Is<MarketOperator>(r =>
                            string.Equals(r.Gln.Value, request.Recipient, StringComparison.OrdinalIgnoreCase)),
                        It.Is<Uuid>(r => r.ToString().Equals(request.BundleId, StringComparison.OrdinalIgnoreCase))))
                .ReturnsAsync(bundle);

            var target = new PeekHandler(warehouseDomainServiceMock.Object);

            // Act
            var (hasContent, stream) = await target.Handle(request, CancellationToken.None).ConfigureAwait(false);

            // Assert
            Assert.True(hasContent);
            Assert.Equal(1, stream.ReadByte());
            Assert.Equal(2, stream.ReadByte());
            Assert.Equal(3, stream.ReadByte());
            await stream.DisposeAsync().ConfigureAwait(false);
        }

        [Fact]
        public async Task PeekAggregationsOrTimeSeriesCommandHandle_WithoutData_ReturnsNullStream()
        {
            // Arrange
            var request = new PeekAggregationsOrTimeSeriesCommand("fake_value", Guid.NewGuid().ToString());

            var warehouseDomainServiceMock = new Mock<IMarketOperatorDataDomainService>();
            warehouseDomainServiceMock
                .Setup(x =>
                    x.GetNextUnacknowledgedAggregationsOrTimeSeriesAsync(
                        It.Is<MarketOperator>(r => string.Equals(r.Gln.Value, request.Recipient, StringComparison.OrdinalIgnoreCase)),
                        It.Is<Uuid>(r => string.Equals(r.ToString(), request.BundleId, StringComparison.OrdinalIgnoreCase))))
                .ReturnsAsync((Bundle?)null);

            var target = new PeekHandler(warehouseDomainServiceMock.Object);

            // Act
            var (hasContent, stream) = await target.Handle(request, CancellationToken.None).ConfigureAwait(false);

            // Assert
            Assert.False(hasContent);
            Assert.Equal(0, stream.Length);
            await stream.DisposeAsync().ConfigureAwait(false);
        }

        private static bool BundleIdCheck(Uuid r, PeekCommand request)
        {
            return r.ToString().Equals(request.BundleId, StringComparison.OrdinalIgnoreCase);
        }
    }
}
