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
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Energinet.DataHub.PostOffice.Application.Commands;
using Energinet.DataHub.PostOffice.Application.Handlers;
using Energinet.DataHub.PostOffice.Domain.Model;
using Energinet.DataHub.PostOffice.Domain.Model.Logging;
using Energinet.DataHub.PostOffice.Domain.Repositories;
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
            var logRepositoryMock = new Mock<ILogRepository>();
            var target = new PeekHandler(warehouseDomainServiceMock.Object, logRepositoryMock.Object);

            // Act + Assert
            await Assert
                .ThrowsAsync<ArgumentNullException>(() => target.Handle((PeekCommand)null!, CancellationToken.None))
                .ConfigureAwait(false);
        }

        [Fact]
        public async Task PeekCommandHandle_WithData_ReturnsDataStream()
        {
            // Arrange
            var bundleId = Guid.NewGuid().ToString();
            var request = new PeekCommand("fake_value", bundleId, BundleReturnType.Xml);

            var logRepositoryMock = new Mock<ILogRepository>();

            var bundleContentMock = new Mock<IBundleContent>();
            bundleContentMock
                .Setup(x => x.OpenAsync())
                .ReturnsAsync(() => new MemoryStream(new byte[] { 1, 2, 3 }));

            var bundle = new Bundle(
                new Uuid(bundleId),
                new MarketOperator(new GlobalLocationNumber("fake_value")),
                DomainOrigin.TimeSeries,
                new ContentType("fake_value"),
                Array.Empty<Uuid>(),
                bundleContentMock.Object,
                Enumerable.Empty<string>(),
                BundleReturnType.Xml);

            var warehouseDomainServiceMock = new Mock<IMarketOperatorDataDomainService>();
            warehouseDomainServiceMock
                .Setup(x =>
                    x.GetNextUnacknowledgedAsync(
                        It.Is<MarketOperator>(r =>
                            string.Equals(r.Gln.Value, request.MarketOperator, StringComparison.OrdinalIgnoreCase)),
                        It.Is<Uuid>(r => BundleIdCheck(r, request)),
                        BundleReturnType.Xml))
                .ReturnsAsync(bundle);

            var target = new PeekHandler(warehouseDomainServiceMock.Object, logRepositoryMock.Object);

            // Act
            var (hasContent, bid, stream, _) = await target.Handle(request, CancellationToken.None).ConfigureAwait(false);

            // Assert
            Assert.True(hasContent);
            Assert.Equal(bundleId, bid);
            Assert.Equal(1, stream.ReadByte());
            Assert.Equal(2, stream.ReadByte());
            Assert.Equal(3, stream.ReadByte());
            await stream.DisposeAsync().ConfigureAwait(false);
        }

        [Fact]
        public async Task PeekCommandHandle_WithoutData_ReturnsNullStream()
        {
            // Arrange
            var request = new PeekCommand("fake_value", Guid.NewGuid().ToString(), BundleReturnType.Xml);

            var logRepositoryMock = new Mock<ILogRepository>();

            var warehouseDomainServiceMock = new Mock<IMarketOperatorDataDomainService>();
            var bundleId = It.Is<Uuid>(r => string.Equals(r.ToString(), request.BundleId, StringComparison.OrdinalIgnoreCase));

            warehouseDomainServiceMock
                .Setup(x =>
                    x.GetNextUnacknowledgedAsync(
                        It.Is<MarketOperator>(r =>
                            string.Equals(r.Gln.Value, request.MarketOperator, StringComparison.OrdinalIgnoreCase)),
                        bundleId,
                        BundleReturnType.Xml))
                .ReturnsAsync((Bundle?)null);

            var target = new PeekHandler(warehouseDomainServiceMock.Object, logRepositoryMock.Object);

            // Act
            var (hasContent, _, stream, _) = await target.Handle(request, CancellationToken.None).ConfigureAwait(false);

            // Assert
            Assert.False(hasContent);
            Assert.Equal(0, stream.Length);
            await stream.DisposeAsync().ConfigureAwait(false);
        }

        [Fact]
        public async Task PeekAggregationsCommandHandle_NullArgument_ThrowsException()
        {
            // Arrange
            var warehouseDomainServiceMock = new Mock<IMarketOperatorDataDomainService>();
            var logRepositoryMock = new Mock<ILogRepository>();
            var target = new PeekHandler(warehouseDomainServiceMock.Object, logRepositoryMock.Object);

            // Act + Assert
            await Assert
                .ThrowsAsync<ArgumentNullException>(() => target.Handle((PeekAggregationsCommand)null!, CancellationToken.None))
                .ConfigureAwait(false);
        }

        [Fact]
        public async Task PeekAggregationsCommandHandle_WithData_ReturnsDataStream()
        {
            // Arrange
            var bundleId = Guid.NewGuid().ToString();
            var request = new PeekAggregationsCommand("fake_value", bundleId, BundleReturnType.Xml);

            var logRepositoryMock = new Mock<ILogRepository>();

            var bundleContentMock = new Mock<IBundleContent>();
            bundleContentMock
                .Setup(x => x.OpenAsync())
                .ReturnsAsync(() => new MemoryStream(new byte[] { 1, 2, 3 }));

            var bundle = new Bundle(
                new Uuid(bundleId),
                new MarketOperator(new GlobalLocationNumber("fake_value")),
                DomainOrigin.TimeSeries,
                new ContentType("fake_value"),
                Array.Empty<Uuid>(),
                bundleContentMock.Object,
                Enumerable.Empty<string>(),
                BundleReturnType.Xml);

            var warehouseDomainServiceMock = new Mock<IMarketOperatorDataDomainService>();
            warehouseDomainServiceMock
                .Setup(x =>
                    x.GetNextUnacknowledgedAggregationsAsync(
                        It.Is<MarketOperator>(r =>
                            string.Equals(r.Gln.Value, request.MarketOperator, StringComparison.OrdinalIgnoreCase)),
                        It.Is<Uuid>(r => r.ToString().Equals(request.BundleId, StringComparison.OrdinalIgnoreCase)),
                        BundleReturnType.Xml))
                .ReturnsAsync(bundle);

            var target = new PeekHandler(warehouseDomainServiceMock.Object, logRepositoryMock.Object);

            // Act
            var (hasContent, bid, stream, _) = await target.Handle(request, CancellationToken.None).ConfigureAwait(false);

            // Assert
            Assert.True(hasContent);
            Assert.Equal(bundleId, bid);
            Assert.Equal(1, stream.ReadByte());
            Assert.Equal(2, stream.ReadByte());
            Assert.Equal(3, stream.ReadByte());
            await stream.DisposeAsync().ConfigureAwait(false);
        }

        [Fact]
        public async Task PeekAggregationsCommandHandle_WithoutData_ReturnsNullStream()
        {
            // Arrange
            var logRepositoryMock = new Mock<ILogRepository>();
            var request = new PeekAggregationsCommand("fake_value", Guid.NewGuid().ToString(), BundleReturnType.Xml);

            var warehouseDomainServiceMock = new Mock<IMarketOperatorDataDomainService>();
            warehouseDomainServiceMock
                .Setup(x =>
                    x.GetNextUnacknowledgedAggregationsAsync(
                        It.Is<MarketOperator>(r => string.Equals(r.Gln.Value, request.MarketOperator, StringComparison.OrdinalIgnoreCase)),
                        It.Is<Uuid>(r => string.Equals(r.ToString(), request.BundleId, StringComparison.OrdinalIgnoreCase)),
                        BundleReturnType.Xml))
                .ReturnsAsync((Bundle?)null);

            var target = new PeekHandler(warehouseDomainServiceMock.Object, logRepositoryMock.Object);

            // Act
            var (hasContent, _, stream, _) = await target.Handle(request, CancellationToken.None).ConfigureAwait(false);

            // Assert
            Assert.False(hasContent);
            Assert.Equal(0, stream.Length);
            await stream.DisposeAsync().ConfigureAwait(false);
        }

        [Fact]
        public async Task PeekTimeSeriesCommandHandle_NullArgument_ThrowsException()
        {
            // Arrange
            var warehouseDomainServiceMock = new Mock<IMarketOperatorDataDomainService>();
            var logMock = new Mock<ILogRepository>();
            var target = new PeekHandler(warehouseDomainServiceMock.Object, logMock.Object);

            // Act + Assert
            await Assert
                .ThrowsAsync<ArgumentNullException>(() => target.Handle((PeekTimeSeriesCommand)null!, CancellationToken.None))
                .ConfigureAwait(false);
        }

        [Fact]
        public async Task PeekTimeSeriesCommandHandle_WithData_ReturnsDataStream()
        {
            // Arrange
            var logMock = new Mock<ILogRepository>();
            var bundleId = Guid.NewGuid().ToString();
            var request = new PeekTimeSeriesCommand("fake_value", bundleId, BundleReturnType.Xml);

            var bundleContentMock = new Mock<IBundleContent>();
            bundleContentMock
                .Setup(x => x.OpenAsync())
                .ReturnsAsync(() => new MemoryStream(new byte[] { 1, 2, 3 }));

            var bundle = new Bundle(
                new Uuid(bundleId),
                new MarketOperator(new GlobalLocationNumber("fake_value")),
                DomainOrigin.Charges,
                new ContentType("fake_value"),
                Array.Empty<Uuid>(),
                bundleContentMock.Object,
                Enumerable.Empty<string>(),
                BundleReturnType.Xml);

            var warehouseDomainServiceMock = new Mock<IMarketOperatorDataDomainService>();
            warehouseDomainServiceMock
                .Setup(x =>
                    x.GetNextUnacknowledgedTimeSeriesAsync(
                        It.Is<MarketOperator>(r =>
                            string.Equals(r.Gln.Value, request.MarketOperator, StringComparison.OrdinalIgnoreCase)),
                        It.Is<Uuid>(r => BundleIdCheck(r, request)),
                        BundleReturnType.Xml))
                .ReturnsAsync(bundle);

            var target = new PeekHandler(warehouseDomainServiceMock.Object, logMock.Object);

            // Act
            var (hasContent, bid, stream, _) = await target.Handle(request, CancellationToken.None).ConfigureAwait(false);

            // Assert
            Assert.True(hasContent);
            Assert.Equal(bundleId, bid);
            Assert.Equal(1, stream.ReadByte());
            Assert.Equal(2, stream.ReadByte());
            Assert.Equal(3, stream.ReadByte());
            await stream.DisposeAsync().ConfigureAwait(false);
        }

        [Fact]
        public async Task PeekTimeSeriesCommandHandle_WithoutData_ReturnsNullStream()
        {
            // Arrange
            var logMock = new Mock<ILogRepository>();
            var request = new PeekTimeSeriesCommand("fake_value", Guid.NewGuid().ToString(), BundleReturnType.Xml);

            var warehouseDomainServiceMock = new Mock<IMarketOperatorDataDomainService>();
            warehouseDomainServiceMock
                .Setup(x =>
                    x.GetNextUnacknowledgedTimeSeriesAsync(
                        It.Is<MarketOperator>(r =>
                            string.Equals(r.Gln.Value, request.MarketOperator, StringComparison.OrdinalIgnoreCase)),
                        It.Is<Uuid>(r => BundleIdCheck(r, request)),
                        BundleReturnType.Xml))
                .ReturnsAsync((Bundle?)null);

            var target = new PeekHandler(warehouseDomainServiceMock.Object, logMock.Object);

            // Act
            var (hasContent, _, stream, _) = await target.Handle(request, CancellationToken.None).ConfigureAwait(false);

            // Assert
            Assert.False(hasContent);
            Assert.Equal(0, stream.Length);
            await stream.DisposeAsync().ConfigureAwait(false);
        }

        [Fact]
        public async Task PeekMasterDataCommandHandle_NullArgument_ThrowsException()
        {
            // Arrange
            var logMock = new Mock<ILogRepository>();
            var warehouseDomainServiceMock = new Mock<IMarketOperatorDataDomainService>();
            var target = new PeekHandler(warehouseDomainServiceMock.Object, logMock.Object);

            // Act + Assert
            await Assert
                .ThrowsAsync<ArgumentNullException>(() => target.Handle((PeekMasterDataCommand)null!, CancellationToken.None))
                .ConfigureAwait(false);
        }

        [Fact]
        public async Task PeekMasterDataCommandHandle_WithData_ReturnsDataStream()
        {
            // Arrange
            var logMock = new Mock<ILogRepository>();
            var bundleId = Guid.NewGuid().ToString();
            var request = new PeekMasterDataCommand("fake_value", bundleId, BundleReturnType.Xml);

            var bundleContentMock = new Mock<IBundleContent>();
            bundleContentMock
                .Setup(x => x.OpenAsync())
                .ReturnsAsync(() => new MemoryStream(new byte[] { 1, 2, 3 }));

            var bundle = new Bundle(
                new Uuid(bundleId),
                new MarketOperator(new GlobalLocationNumber("fake_value")),
                DomainOrigin.MarketRoles,
                new ContentType("fake_value"),
                Array.Empty<Uuid>(),
                bundleContentMock.Object,
                Enumerable.Empty<string>(),
                BundleReturnType.Xml);

            var warehouseDomainServiceMock = new Mock<IMarketOperatorDataDomainService>();
            warehouseDomainServiceMock
                .Setup(x =>
                    x.GetNextUnacknowledgedMasterDataAsync(
                        It.Is<MarketOperator>(r =>
                            string.Equals(r.Gln.Value, request.MarketOperator, StringComparison.OrdinalIgnoreCase)),
                        It.Is<Uuid>(r => BundleIdCheck(r, request)),
                        BundleReturnType.Xml))
                .ReturnsAsync(bundle);

            var target = new PeekHandler(warehouseDomainServiceMock.Object, logMock.Object);

            // Act
            var (hasContent, bid, stream, _) = await target.Handle(request, CancellationToken.None).ConfigureAwait(false);

            // Assert
            Assert.True(hasContent);
            Assert.Equal(bundleId, bid);
            Assert.Equal(1, stream.ReadByte());
            Assert.Equal(2, stream.ReadByte());
            Assert.Equal(3, stream.ReadByte());
            await stream.DisposeAsync().ConfigureAwait(false);
        }

        [Fact]
        public async Task PeekMasterDataCommandHandle_WithoutData_ReturnsNullStream()
        {
            // Arrange
            var logMock = new Mock<ILogRepository>();
            var request = new PeekMasterDataCommand("fake_value", Guid.NewGuid().ToString(), BundleReturnType.Xml);

            var warehouseDomainServiceMock = new Mock<IMarketOperatorDataDomainService>();
            warehouseDomainServiceMock
                .Setup(x =>
                    x.GetNextUnacknowledgedMasterDataAsync(
                        It.Is<MarketOperator>(r =>
                            string.Equals(r.Gln.Value, request.MarketOperator, StringComparison.OrdinalIgnoreCase)),
                        It.Is<Uuid>(r => BundleIdCheck(r, request)),
                        BundleReturnType.Xml))
                .ReturnsAsync((Bundle?)null);

            var target = new PeekHandler(warehouseDomainServiceMock.Object, logMock.Object);

            // Act
            var (hasContent, _, stream, _) = await target.Handle(request, CancellationToken.None).ConfigureAwait(false);

            // Assert
            Assert.False(hasContent);
            Assert.Equal(0, stream.Length);
            await stream.DisposeAsync().ConfigureAwait(false);
        }

        [Fact]
        public async Task SavePeekLogOccurrenceAsync_IsMethodCalled_IsCalled()
        {
            // Arrange
            var dataDomainServiceMock = new Mock<IMarketOperatorDataDomainService>();
            var target = new Mock<ILogRepository>();
            var peekCommand = new PeekCommand("fake_value", Guid.NewGuid().ToString(), BundleReturnType.Xml);

            var bundleContentMock = new Mock<IBundleContent>();
            bundleContentMock
                .Setup(x => x.OpenAsync())
                .ReturnsAsync(() => new MemoryStream(new byte[] { 1, 2, 3 }));

            var bundle = new Bundle(
                new Uuid(Guid.NewGuid()),
                new MarketOperator(new GlobalLocationNumber("fake_value")),
                DomainOrigin.MarketRoles,
                new ContentType("fake_value"),
                Array.Empty<Uuid>(),
                bundleContentMock.Object,
                Enumerable.Empty<string>(),
                BundleReturnType.Xml);

            dataDomainServiceMock.Setup(x => x.GetNextUnacknowledgedAsync(
                        It.IsAny<MarketOperator>(), It.IsAny<Uuid>(), BundleReturnType.Xml))
                .ReturnsAsync(bundle);

            var handler = new PeekHandler(dataDomainServiceMock.Object, target.Object);

            // Act
            await handler.Handle(peekCommand, CancellationToken.None).ConfigureAwait(false);

            // Assert
            target.Verify(m => m.SavePeekLogOccurrenceAsync(It.IsAny<PeekLog>()), Times.Once);
        }

        [Fact]
        public async Task SavePeekLogOccurrenceAsync_IsMethodCalled_NotCalled()
        {
            // Arrange
            var dataDomainServiceMock = new Mock<IMarketOperatorDataDomainService>();
            var target = new Mock<ILogRepository>();
            var peekCommand = new PeekCommand("fake_value", Guid.NewGuid().ToString(), BundleReturnType.Xml);

            Bundle bundle = null!;

            dataDomainServiceMock.Setup(m => m.GetNextUnacknowledgedAsync(
                    It.IsAny<MarketOperator>(), It.IsAny<Uuid>(), BundleReturnType.Xml))
                .ReturnsAsync(bundle);

            var handler = new PeekHandler(dataDomainServiceMock.Object, target.Object);

            // Act
            await handler.Handle(peekCommand, CancellationToken.None).ConfigureAwait(false);

            // Assert
            target.Verify(m => m.SavePeekLogOccurrenceAsync(It.IsAny<PeekLog>()), Times.Never);
        }

        private static bool BundleIdCheck(Uuid r, PeekCommandBase request)
        {
            return r.ToString().Equals(request.BundleId, StringComparison.OrdinalIgnoreCase);
        }
    }
}
