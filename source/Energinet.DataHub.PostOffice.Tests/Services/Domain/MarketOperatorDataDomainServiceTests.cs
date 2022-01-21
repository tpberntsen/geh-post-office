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
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using Energinet.DataHub.PostOffice.Domain.Model;
using Energinet.DataHub.PostOffice.Domain.Repositories;
using Energinet.DataHub.PostOffice.Domain.Services;
using Moq;
using Xunit;
using Xunit.Categories;

namespace Energinet.DataHub.PostOffice.Tests.Services.Domain
{
    [UnitTest]
    public sealed class MarketOperatorDataDomainServiceTests
    {
        [Fact]
        public async Task GetNextUnacknowledgedAsync_NoNotificationsReady_ReturnsNull()
        {
            // Arrange
            var recipient = new MarketOperator(new GlobalLocationNumber("fake_value"));
            var bundleId = new Uuid("7dfb2080-fb56-4a37-a85d-1ac2f1559b45");

            var dataAvailableNotificationRepositoryMock = new Mock<IDataAvailableNotificationRepository>();
            dataAvailableNotificationRepositoryMock
                .Setup(x => x.GetNextUnacknowledgedAsync(recipient))
                .ReturnsAsync((DataAvailableNotification?)null);

            var bundleRepositoryMock = new Mock<IBundleRepository>();
            bundleRepositoryMock
                .Setup(x => x.GetNextUnacknowledgedAsync(recipient))
                .ReturnsAsync((Bundle?)null);

            var requestDomainServiceMock = new Mock<IRequestBundleDomainService>();
            var contentTypeWeightMap = new Mock<IWeightCalculatorDomainService>();
            var operationServiceMock = new Mock<IDequeueCleanUpSchedulingService>();

            var target = new MarketOperatorDataDomainService(
                bundleRepositoryMock.Object,
                dataAvailableNotificationRepositoryMock.Object,
                requestDomainServiceMock.Object,
                contentTypeWeightMap.Object,
                operationServiceMock.Object);

            // Act
            var bundle = await target.GetNextUnacknowledgedAsync(recipient, bundleId).ConfigureAwait(false);

            // Assert
            Assert.Null(bundle);
        }

        [Fact]
        public async Task GetNextUnacknowledgedAsync_HasNotificationsButCannotTryAdd_ReturnsNull()
        {
            // Arrange
            var recipient = new MarketOperator(new GlobalLocationNumber("fake_value"));
            var bundleId = new Uuid("7dfb2080-fb56-4a37-a85d-1ac2f1559b45");
            var contentType = new ContentType("timeseries");

            var dataAvailableNotificationFirst = CreateDataAvailableNotification(recipient, contentType);
            var allDataAvailableNotificationsForMessageType = new[]
            {
                dataAvailableNotificationFirst,
                CreateDataAvailableNotification(recipient, contentType),
                CreateDataAvailableNotification(recipient, contentType),
                CreateDataAvailableNotification(recipient, contentType)
            };

            var dataAvailableNotificationRepositoryMock = new Mock<IDataAvailableNotificationRepository>();
            dataAvailableNotificationRepositoryMock
                .Setup(x => x.GetNextUnacknowledgedAsync(recipient))
                .ReturnsAsync(dataAvailableNotificationFirst);

            var weight = new Weight(1);

            var contentTypeWeightMapMock = new Mock<IWeightCalculatorDomainService>();
            contentTypeWeightMapMock
                .Setup(x => x.CalculateMaxWeight(DomainOrigin.TimeSeries))
                .Returns(weight);

            dataAvailableNotificationRepositoryMock
                .Setup(x => x.GetNextUnacknowledgedAsync(recipient, DomainOrigin.TimeSeries, contentType, weight))
                .ReturnsAsync(allDataAvailableNotificationsForMessageType);

            var requestDomainServiceMock = new Mock<IRequestBundleDomainService>();
            var operationServiceMock = new Mock<IDequeueCleanUpSchedulingService>();

            var bundleRepositoryMock = new Mock<IBundleRepository>();
            bundleRepositoryMock
                .Setup(x => x.GetNextUnacknowledgedAsync(recipient))
                .ReturnsAsync((Bundle?)null);

            bundleRepositoryMock
                .Setup(x => x.TryAddNextUnacknowledgedAsync(It.IsAny<Bundle>()))
                .ReturnsAsync(BundleCreatedResponse.AnotherBundleExists);

            var target = new MarketOperatorDataDomainService(
                bundleRepositoryMock.Object,
                dataAvailableNotificationRepositoryMock.Object,
                requestDomainServiceMock.Object,
                contentTypeWeightMapMock.Object,
                operationServiceMock.Object);

            // Act
            var bundle = await target.GetNextUnacknowledgedAsync(recipient, bundleId).ConfigureAwait(false);

            // Assert
            Assert.Null(bundle);
        }

        [Fact]
        public async Task GetNextUnacknowledgedAsync_HasNotificationsReady_ReturnsBundle()
        {
            // Arrange
            var recipient = new MarketOperator(new GlobalLocationNumber("fake_value"));
            var bundleId = new Uuid("7dfb2080-fb56-4a37-a85d-1ac2f1559b45");
            var contentType = new ContentType("timeseries");

            var dataAvailableNotificationFirst = CreateDataAvailableNotification(recipient, contentType);
            var allDataAvailableNotificationsForMessageType = new[]
            {
                dataAvailableNotificationFirst,
                CreateDataAvailableNotification(recipient, contentType),
                CreateDataAvailableNotification(recipient, contentType),
                CreateDataAvailableNotification(recipient, contentType)
            };

            var dataAvailableNotificationRepositoryMock = new Mock<IDataAvailableNotificationRepository>();
            dataAvailableNotificationRepositoryMock
                .Setup(x => x.GetNextUnacknowledgedAsync(recipient))
                .ReturnsAsync(dataAvailableNotificationFirst);

            var weight = new Weight(1);

            var contentTypeWeightMapMock = new Mock<IWeightCalculatorDomainService>();
            contentTypeWeightMapMock
                .Setup(x => x.CalculateMaxWeight(DomainOrigin.TimeSeries))
                .Returns(weight);

            dataAvailableNotificationRepositoryMock
                .Setup(x => x.GetNextUnacknowledgedAsync(recipient, DomainOrigin.TimeSeries, contentType, weight))
                .ReturnsAsync(allDataAvailableNotificationsForMessageType);

            var requestDomainServiceMock = new Mock<IRequestBundleDomainService>();
            var bundleContentMock = new Mock<IBundleContent>();
            var operationServiceMock = new Mock<IDequeueCleanUpSchedulingService>();

            var bundleRepositoryMock = new Mock<IBundleRepository>();
            bundleRepositoryMock
                .Setup(x => x.GetNextUnacknowledgedAsync(recipient))
                .ReturnsAsync((Bundle?)null);

            bundleRepositoryMock
                .Setup(x => x.TryAddNextUnacknowledgedAsync(It.IsAny<Bundle>()))
                .ReturnsAsync(BundleCreatedResponse.Success);

            requestDomainServiceMock
                .Setup(x => x.WaitForBundleContentFromSubDomainAsync(It.IsAny<Bundle>()))
                .ReturnsAsync(bundleContentMock.Object);

            var target = new MarketOperatorDataDomainService(
                bundleRepositoryMock.Object,
                dataAvailableNotificationRepositoryMock.Object,
                requestDomainServiceMock.Object,
                contentTypeWeightMapMock.Object,
                operationServiceMock.Object);

            // Act
            var bundle = await target.GetNextUnacknowledgedAsync(recipient, bundleId).ConfigureAwait(false);

            // Assert
            Assert.NotNull(bundle);
            Assert.Equal(dataAvailableNotificationFirst.Recipient, bundle!.Recipient);
            Assert.Equal(dataAvailableNotificationFirst.Origin, bundle.Origin);
            Assert.True(bundle.TryGetContent(out var actualBundleContent));
            Assert.Equal(bundleContentMock.Object, actualBundleContent);
        }

        [Fact]
        public async Task GetNextUnacknowledgedAsync_BundleIdAlreadyInUse_ReturnsValidationException()
        {
            // Arrange
            var recipient = new MarketOperator(new GlobalLocationNumber("fake_value"));
            var bundleId = new Uuid("7dfb2080-fb56-4a37-a85d-1ac2f1559b45");
            var contentType = new ContentType("timeseries");

            var dataAvailableNotificationFirst = CreateDataAvailableNotification(recipient, contentType);
            var allDataAvailableNotificationsForMessageType = new[]
            {
                dataAvailableNotificationFirst,
                CreateDataAvailableNotification(recipient, contentType),
                CreateDataAvailableNotification(recipient, contentType),
                CreateDataAvailableNotification(recipient, contentType)
            };

            var dataAvailableNotificationRepositoryMock = new Mock<IDataAvailableNotificationRepository>();
            dataAvailableNotificationRepositoryMock
                .Setup(x => x.GetNextUnacknowledgedAsync(recipient))
                .ReturnsAsync(dataAvailableNotificationFirst);

            var weight = new Weight(1);

            var contentTypeWeightMapMock = new Mock<IWeightCalculatorDomainService>();
            contentTypeWeightMapMock
                .Setup(x => x.CalculateMaxWeight(DomainOrigin.TimeSeries))
                .Returns(weight);

            dataAvailableNotificationRepositoryMock
                .Setup(x => x.GetNextUnacknowledgedAsync(recipient, DomainOrigin.TimeSeries, contentType, weight))
                .ReturnsAsync(allDataAvailableNotificationsForMessageType);

            var requestDomainServiceMock = new Mock<IRequestBundleDomainService>();
            var bundleContentMock = new Mock<IBundleContent>();
            var operationServiceMock = new Mock<IDequeueCleanUpSchedulingService>();

            var bundleRepositoryMock = new Mock<IBundleRepository>();
            bundleRepositoryMock
                .Setup(x => x.GetNextUnacknowledgedAsync(recipient))
                .ReturnsAsync((Bundle?)null);

            bundleRepositoryMock
                .Setup(x => x.TryAddNextUnacknowledgedAsync(It.IsAny<Bundle>()))
                .ReturnsAsync(BundleCreatedResponse.BundleIdAlreadyInUse);

            requestDomainServiceMock
                .Setup(x => x.WaitForBundleContentFromSubDomainAsync(It.IsAny<Bundle>()))
                .ReturnsAsync(bundleContentMock.Object);

            var target = new MarketOperatorDataDomainService(
                bundleRepositoryMock.Object,
                dataAvailableNotificationRepositoryMock.Object,
                requestDomainServiceMock.Object,
                contentTypeWeightMapMock.Object,
                operationServiceMock.Object);

            // Act
            var nextUnacknowledged = target.GetNextUnacknowledgedAsync(recipient, bundleId).ConfigureAwait(true);

            // Assert
            await Assert.ThrowsAsync<ValidationException>(async () => await nextUnacknowledged).ConfigureAwait(true);
        }

        [Fact]
        public async Task GetNextUnacknowledgedAsync_HasBundleNotYetDequeued_ReturnsThatPreviousBundle()
        {
            // Arrange
            var recipient = new MarketOperator(new GlobalLocationNumber("fake_value"));
            var bundleId = new Uuid("7dfb2080-fb56-4a37-a85d-1ac2f1559b45");
            var dataAvailableNotificationRepositoryMock = new Mock<IDataAvailableNotificationRepository>();

            var bundleContentMock = new Mock<IBundleContent>();
            var setupBundle = new Bundle(
                new Uuid("7dfb2080-fb56-4a37-a85d-1ac2f1559b45"),
                recipient,
                DomainOrigin.TimeSeries,
                new ContentType("fake_value"),
                Array.Empty<Uuid>(),
                bundleContentMock.Object);

            var bundleRepositoryMock = new Mock<IBundleRepository>();
            bundleRepositoryMock
                .Setup(x => x.GetNextUnacknowledgedAsync(recipient))
                .ReturnsAsync(setupBundle);

            var contentTypeWeightMapMock = new Mock<IWeightCalculatorDomainService>();
            var requestDomainServiceMock = new Mock<IRequestBundleDomainService>();
            var operationServiceMock = new Mock<IDequeueCleanUpSchedulingService>();
            var target = new MarketOperatorDataDomainService(
                bundleRepositoryMock.Object,
                dataAvailableNotificationRepositoryMock.Object,
                requestDomainServiceMock.Object,
                contentTypeWeightMapMock.Object,
                operationServiceMock.Object);

            // Act
            var bundle = await target.GetNextUnacknowledgedAsync(recipient, bundleId).ConfigureAwait(false);

            // Assert
            Assert.Equal(setupBundle, bundle);
        }

        [Fact]
        public async Task GetNextUnacknowledgedAsync_HasBundleNotYetDequeuedWithNoData_ReturnsBundle()
        {
            // Arrange
            var recipient = new MarketOperator(new GlobalLocationNumber("fake_value"));
            var bundleId = new Uuid("7dfb2080-fb56-4a37-a85d-1ac2f1559b45");

            var bundleContentMock = new Mock<IBundleContent>();
            var setupBundle = new Bundle(
                new Uuid("7dfb2080-fb56-4a37-a85d-1ac2f1559b45"),
                recipient,
                DomainOrigin.TimeSeries,
                new ContentType("fake_value"),
                Array.Empty<Uuid>());

            var bundleRepositoryMock = new Mock<IBundleRepository>();
            bundleRepositoryMock
                .Setup(x => x.GetNextUnacknowledgedAsync(recipient))
                .ReturnsAsync(setupBundle);

            var requestDomainServiceMock = new Mock<IRequestBundleDomainService>();
            requestDomainServiceMock
                .Setup(x => x.WaitForBundleContentFromSubDomainAsync(setupBundle))
                .ReturnsAsync(bundleContentMock.Object);

            var dataAvailableNotificationRepositoryMock = new Mock<IDataAvailableNotificationRepository>();
            var contentTypeWeightMapMock = new Mock<IWeightCalculatorDomainService>();
            var operationServiceMock = new Mock<IDequeueCleanUpSchedulingService>();
            var target = new MarketOperatorDataDomainService(
                bundleRepositoryMock.Object,
                dataAvailableNotificationRepositoryMock.Object,
                requestDomainServiceMock.Object,
                contentTypeWeightMapMock.Object,
                operationServiceMock.Object);

            // Act
            var bundle = await target.GetNextUnacknowledgedAsync(recipient, bundleId).ConfigureAwait(false);

            // Assert
            Assert.NotNull(bundle);
            Assert.Equal(setupBundle, bundle);
            Assert.True(bundle!.TryGetContent(out var actualBundleContent));
            Assert.Equal(bundleContentMock.Object, actualBundleContent);
        }

        [Fact]
        public async Task GetNextUnacknowledgedAsync_HasBundleNotYetDequeuedCannotGetData_ReturnsNull()
        {
            // Arrange
            var recipient = new MarketOperator(new GlobalLocationNumber("fake_value"));
            var bundleId = new Uuid("7dfb2080-fb56-4a37-a85d-1ac2f1559b45");
            var setupBundle = new Bundle(
                bundleId,
                recipient,
                DomainOrigin.TimeSeries,
                new ContentType("fake_value"),
                Array.Empty<Uuid>());

            var bundleRepositoryMock = new Mock<IBundleRepository>();
            bundleRepositoryMock
                .Setup(x => x.GetNextUnacknowledgedAsync(recipient))
                .ReturnsAsync(setupBundle);

            var requestDomainServiceMock = new Mock<IRequestBundleDomainService>();
            requestDomainServiceMock
                .Setup(x => x.WaitForBundleContentFromSubDomainAsync(setupBundle))
                .ReturnsAsync((IBundleContent?)null);

            var dataAvailableNotificationRepositoryMock = new Mock<IDataAvailableNotificationRepository>();
            var contentTypeWeightMapMock = new Mock<IWeightCalculatorDomainService>();
            var operationServiceMock = new Mock<IDequeueCleanUpSchedulingService>();
            var target = new MarketOperatorDataDomainService(
                bundleRepositoryMock.Object,
                dataAvailableNotificationRepositoryMock.Object,
                requestDomainServiceMock.Object,
                contentTypeWeightMapMock.Object,
                operationServiceMock.Object);

            // Act
            var bundle = await target.GetNextUnacknowledgedAsync(recipient, bundleId).ConfigureAwait(false);

            // Assert
            Assert.Null(bundle);
        }

        [Fact]
        public async Task GetNextUnacknowledgedAsync_BundlingNotSupported_ReturnsBundleWithSingleNotification()
        {
            // Arrange
            var recipient = new MarketOperator(new GlobalLocationNumber("fake_value"));
            var bundleId = new Uuid("7dfb2080-fb56-4a37-a85d-1ac2f1559b45");
            var contentType = new ContentType("timeseries");

            var dataAvailableNotification = new DataAvailableNotification(
                new Uuid(Guid.NewGuid()),
                recipient,
                contentType,
                DomainOrigin.TimeSeries,
                new SupportsBundling(false),
                new Weight(1),
                new SequenceNumber(1));

            var dataAvailableNotificationRepositoryMock = new Mock<IDataAvailableNotificationRepository>();
            dataAvailableNotificationRepositoryMock
                .Setup(x => x.GetNextUnacknowledgedAsync(recipient))
                .ReturnsAsync(dataAvailableNotification);

            var weight = new Weight(100);

            var contentTypeWeightMapMock = new Mock<IWeightCalculatorDomainService>();
            contentTypeWeightMapMock
                .Setup(x => x.CalculateMaxWeight(DomainOrigin.TimeSeries))
                .Returns(weight);

            var requestDomainServiceMock = new Mock<IRequestBundleDomainService>();
            var bundleContentMock = new Mock<IBundleContent>();
            var operationServiceMock = new Mock<IDequeueCleanUpSchedulingService>();

            var bundleRepositoryMock = new Mock<IBundleRepository>();
            bundleRepositoryMock
                .Setup(x => x.GetNextUnacknowledgedAsync(recipient))
                .ReturnsAsync((Bundle?)null);

            bundleRepositoryMock
                .Setup(x => x.TryAddNextUnacknowledgedAsync(It.IsAny<Bundle>()))
                .ReturnsAsync(BundleCreatedResponse.Success);

            requestDomainServiceMock
                .Setup(x => x.WaitForBundleContentFromSubDomainAsync(It.IsAny<Bundle>()))
                .ReturnsAsync(bundleContentMock.Object);

            var target = new MarketOperatorDataDomainService(
                bundleRepositoryMock.Object,
                dataAvailableNotificationRepositoryMock.Object,
                requestDomainServiceMock.Object,
                contentTypeWeightMapMock.Object,
                operationServiceMock.Object);

            // Act
            var bundle = await target.GetNextUnacknowledgedAsync(recipient, bundleId).ConfigureAwait(false);

            // Assert
            Assert.NotNull(bundle);
            Assert.Equal(dataAvailableNotification.Recipient, bundle!.Recipient);
            Assert.Equal(dataAvailableNotification.Origin, bundle.Origin);
            Assert.True(bundle.TryGetContent(out var actualBundleContent));
            Assert.Equal(bundleContentMock.Object, actualBundleContent);
        }

        [Fact]
        public async Task GetNextUnacknowledgedAsync_TooLargeToBundle_ReturnsBundleWithSingleNotification()
        {
            // Arrange
            var recipient = new MarketOperator(new GlobalLocationNumber("fake_value"));
            var bundleId = new Uuid("7dfb2080-fb56-4a37-a85d-1ac2f1559b45");
            var contentType = new ContentType("timeseries");

            var dataAvailableNotification = new DataAvailableNotification(
                new Uuid(Guid.NewGuid()),
                recipient,
                contentType,
                DomainOrigin.TimeSeries,
                new SupportsBundling(true),
                new Weight(int.MaxValue),
                new SequenceNumber(1));

            var dataAvailableNotificationRepositoryMock = new Mock<IDataAvailableNotificationRepository>();
            dataAvailableNotificationRepositoryMock
                .Setup(x => x.GetNextUnacknowledgedAsync(recipient))
                .ReturnsAsync(dataAvailableNotification);

            var weight = new Weight(100);

            var contentTypeWeightMapMock = new Mock<IWeightCalculatorDomainService>();
            contentTypeWeightMapMock
                .Setup(x => x.CalculateMaxWeight(DomainOrigin.TimeSeries))
                .Returns(weight);

            var requestDomainServiceMock = new Mock<IRequestBundleDomainService>();
            var bundleContentMock = new Mock<IBundleContent>();
            var operationServiceMock = new Mock<IDequeueCleanUpSchedulingService>();

            var bundleRepositoryMock = new Mock<IBundleRepository>();
            bundleRepositoryMock
                .Setup(x => x.GetNextUnacknowledgedAsync(recipient))
                .ReturnsAsync((Bundle?)null);

            bundleRepositoryMock
                .Setup(x => x.TryAddNextUnacknowledgedAsync(It.IsAny<Bundle>()))
                .ReturnsAsync(BundleCreatedResponse.Success);

            requestDomainServiceMock
                .Setup(x => x.WaitForBundleContentFromSubDomainAsync(It.IsAny<Bundle>()))
                .ReturnsAsync(bundleContentMock.Object);

            var target = new MarketOperatorDataDomainService(
                bundleRepositoryMock.Object,
                dataAvailableNotificationRepositoryMock.Object,
                requestDomainServiceMock.Object,
                contentTypeWeightMapMock.Object,
                operationServiceMock.Object);

            // Act
            var bundle = await target.GetNextUnacknowledgedAsync(recipient, bundleId).ConfigureAwait(false);

            // Assert
            Assert.NotNull(bundle);
            Assert.Equal(dataAvailableNotification.Recipient, bundle!.Recipient);
            Assert.Equal(dataAvailableNotification.Origin, bundle.Origin);
            Assert.True(bundle.TryGetContent(out var actualBundleContent));
            Assert.Equal(bundleContentMock.Object, actualBundleContent);
        }

        [Fact]
        public async Task GetNextUnacknowledgedTimeSeriesAsync_NoNotificationsReady_ReturnsNull()
        {
            // Arrange
            var recipient = new MarketOperator(new GlobalLocationNumber("fake_value"));
            var bundleId = new Uuid(Guid.NewGuid());

            var dataAvailableNotificationRepositoryMock = new Mock<IDataAvailableNotificationRepository>();
            dataAvailableNotificationRepositoryMock
                .Setup(x => x.GetNextUnacknowledgedAsync(recipient, DomainOrigin.TimeSeries))
                .ReturnsAsync((DataAvailableNotification?)null);

            var bundleRepositoryMock = new Mock<IBundleRepository>();
            bundleRepositoryMock
                .Setup(x => x.GetNextUnacknowledgedAsync(recipient, DomainOrigin.TimeSeries))
                .ReturnsAsync((Bundle?)null);

            var requestDomainServiceMock = new Mock<IRequestBundleDomainService>();
            var contentTypeWeightMap = new Mock<IWeightCalculatorDomainService>();
            var operationServiceMock = new Mock<IDequeueCleanUpSchedulingService>();

            var target = new MarketOperatorDataDomainService(
                bundleRepositoryMock.Object,
                dataAvailableNotificationRepositoryMock.Object,
                requestDomainServiceMock.Object,
                contentTypeWeightMap.Object,
                operationServiceMock.Object);

            // Act
            var bundle = await target.GetNextUnacknowledgedTimeSeriesAsync(recipient, bundleId).ConfigureAwait(false);

            // Assert
            Assert.Null(bundle);
        }

        [Fact]
        public async Task GetNextUnacknowledgedTimeSeriesAsync_HasNotificationsButCannotTryAdd_ReturnsNull()
        {
            // Arrange
            var recipient = new MarketOperator(new GlobalLocationNumber("fake_value"));
            var contentType = new ContentType("rms-xyz");
            var bundleId = new Uuid(Guid.NewGuid());

            var dataAvailableNotificationFirst = CreateDataAvailableNotification(recipient, contentType);
            var allDataAvailableNotificationsForMessageType = new[]
            {
                dataAvailableNotificationFirst,
                CreateDataAvailableNotification(recipient, contentType),
                CreateDataAvailableNotification(recipient, contentType),
                CreateDataAvailableNotification(recipient, contentType)
            };

            var dataAvailableNotificationRepositoryMock = new Mock<IDataAvailableNotificationRepository>();
            dataAvailableNotificationRepositoryMock
                .Setup(x => x.GetNextUnacknowledgedAsync(recipient, DomainOrigin.TimeSeries))
                .ReturnsAsync(dataAvailableNotificationFirst);

            var weight = new Weight(1);

            var contentTypeWeightMapMock = new Mock<IWeightCalculatorDomainService>();
            contentTypeWeightMapMock
                .Setup(x => x.CalculateMaxWeight(DomainOrigin.TimeSeries))
                .Returns(weight);

            dataAvailableNotificationRepositoryMock
                .Setup(x => x.GetNextUnacknowledgedAsync(recipient, DomainOrigin.TimeSeries, contentType, weight))
                .ReturnsAsync(allDataAvailableNotificationsForMessageType);

            var requestDomainServiceMock = new Mock<IRequestBundleDomainService>();
            var operationServiceMock = new Mock<IDequeueCleanUpSchedulingService>();

            var bundleRepositoryMock = new Mock<IBundleRepository>();
            bundleRepositoryMock
                .Setup(x => x.GetNextUnacknowledgedAsync(recipient, DomainOrigin.TimeSeries))
                .ReturnsAsync((Bundle?)null);

            bundleRepositoryMock
                .Setup(x => x.TryAddNextUnacknowledgedAsync(It.IsAny<Bundle>()))
                .ReturnsAsync(BundleCreatedResponse.AnotherBundleExists);

            var target = new MarketOperatorDataDomainService(
                bundleRepositoryMock.Object,
                dataAvailableNotificationRepositoryMock.Object,
                requestDomainServiceMock.Object,
                contentTypeWeightMapMock.Object,
                operationServiceMock.Object);

            // Act
            var bundle = await target.GetNextUnacknowledgedTimeSeriesAsync(recipient, bundleId).ConfigureAwait(false);

            // Assert
            Assert.Null(bundle);
        }

        [Fact]
        public async Task GetNextUnacknowledgedTimeSeriesAsync_HasNotificationsReady_ReturnsBundle()
        {
            // Arrange
            var recipient = new MarketOperator(new GlobalLocationNumber("fake_value"));
            var contentType = new ContentType("rms-xyz");
            var bundleId = new Uuid(Guid.NewGuid());

            var dataAvailableNotificationFirst = CreateDataAvailableNotification(recipient, contentType);
            var allDataAvailableNotificationsForMessageType = new[]
            {
                dataAvailableNotificationFirst,
                CreateDataAvailableNotification(recipient, contentType),
                CreateDataAvailableNotification(recipient, contentType),
                CreateDataAvailableNotification(recipient, contentType)
            };

            var dataAvailableNotificationRepositoryMock = new Mock<IDataAvailableNotificationRepository>();
            dataAvailableNotificationRepositoryMock
                .Setup(x => x.GetNextUnacknowledgedAsync(recipient, DomainOrigin.TimeSeries))
                .ReturnsAsync(dataAvailableNotificationFirst);

            var weight = new Weight(1);

            var contentTypeWeightMapMock = new Mock<IWeightCalculatorDomainService>();
            contentTypeWeightMapMock
                .Setup(x => x.CalculateMaxWeight(DomainOrigin.TimeSeries))
                .Returns(weight);

            dataAvailableNotificationRepositoryMock
                .Setup(x => x.GetNextUnacknowledgedAsync(recipient, DomainOrigin.TimeSeries, contentType, weight))
                .ReturnsAsync(allDataAvailableNotificationsForMessageType);

            var requestDomainServiceMock = new Mock<IRequestBundleDomainService>();
            var bundleContentMock = new Mock<IBundleContent>();
            var operationServiceMock = new Mock<IDequeueCleanUpSchedulingService>();

            var bundleRepositoryMock = new Mock<IBundleRepository>();
            bundleRepositoryMock
                .Setup(x => x.GetNextUnacknowledgedAsync(recipient, DomainOrigin.TimeSeries))
                .ReturnsAsync((Bundle?)null);

            bundleRepositoryMock
                .Setup(x => x.TryAddNextUnacknowledgedAsync(It.IsAny<Bundle>()))
                .ReturnsAsync(BundleCreatedResponse.Success);

            requestDomainServiceMock
                .Setup(x => x.WaitForBundleContentFromSubDomainAsync(It.IsAny<Bundle>()))
                .ReturnsAsync(bundleContentMock.Object);

            var target = new MarketOperatorDataDomainService(
                bundleRepositoryMock.Object,
                dataAvailableNotificationRepositoryMock.Object,
                requestDomainServiceMock.Object,
                contentTypeWeightMapMock.Object,
                operationServiceMock.Object);

            // Act
            var bundle = await target.GetNextUnacknowledgedTimeSeriesAsync(recipient, bundleId).ConfigureAwait(false);

            // Assert
            Assert.NotNull(bundle);
            Assert.Equal(dataAvailableNotificationFirst.Recipient, bundle!.Recipient);
            Assert.Equal(dataAvailableNotificationFirst.Origin, bundle.Origin);
            Assert.True(bundle.TryGetContent(out var actualBundleContent));
            Assert.Equal(bundleContentMock.Object, actualBundleContent);
        }

        [Fact]
        public async Task GetNextUnacknowledgedTimeSeriesAsync_HasBundleNotYetDequeued_ReturnsThatPreviousBundle()
        {
            // Arrange
            var recipient = new MarketOperator(new GlobalLocationNumber("fake_value"));
            var dataAvailableNotificationRepositoryMock = new Mock<IDataAvailableNotificationRepository>();
            var bundleId = new Uuid(Guid.NewGuid());
            var bundleContentMock = new Mock<IBundleContent>();
            var setupBundle = new Bundle(
                bundleId,
                recipient,
                DomainOrigin.TimeSeries,
                new ContentType("fake_value"),
                Array.Empty<Uuid>(),
                bundleContentMock.Object);

            var bundleRepositoryMock = new Mock<IBundleRepository>();
            bundleRepositoryMock
                .Setup(x => x.GetNextUnacknowledgedAsync(recipient, DomainOrigin.TimeSeries))
                .ReturnsAsync(setupBundle);

            var contentTypeWeightMapMock = new Mock<IWeightCalculatorDomainService>();
            var requestDomainServiceMock = new Mock<IRequestBundleDomainService>();
            var operationServiceMock = new Mock<IDequeueCleanUpSchedulingService>();
            var target = new MarketOperatorDataDomainService(
                bundleRepositoryMock.Object,
                dataAvailableNotificationRepositoryMock.Object,
                requestDomainServiceMock.Object,
                contentTypeWeightMapMock.Object,
                operationServiceMock.Object);

            // Act
            var bundle = await target.GetNextUnacknowledgedTimeSeriesAsync(recipient, bundleId).ConfigureAwait(false);

            // Assert
            Assert.Equal(setupBundle, bundle);
        }

        [Fact]
        public async Task GetNextUnacknowledgedTimeSeriesAsync_HasBundleNotYetDequeuedWithNoData_ReturnsBundle()
        {
            // Arrange
            var recipient = new MarketOperator(new GlobalLocationNumber("fake_value"));
            var bundleId = new Uuid(Guid.NewGuid());
            var bundleContentMock = new Mock<IBundleContent>();
            var setupBundle = new Bundle(
                bundleId,
                recipient,
                DomainOrigin.TimeSeries,
                new ContentType("fake_value"),
                Array.Empty<Uuid>());

            var bundleRepositoryMock = new Mock<IBundleRepository>();
            bundleRepositoryMock
                .Setup(x => x.GetNextUnacknowledgedAsync(recipient, DomainOrigin.TimeSeries))
                .ReturnsAsync(setupBundle);

            var requestDomainServiceMock = new Mock<IRequestBundleDomainService>();
            requestDomainServiceMock
                .Setup(x => x.WaitForBundleContentFromSubDomainAsync(setupBundle))
                .ReturnsAsync(bundleContentMock.Object);

            var dataAvailableNotificationRepositoryMock = new Mock<IDataAvailableNotificationRepository>();
            var contentTypeWeightMapMock = new Mock<IWeightCalculatorDomainService>();
            var operationServiceMock = new Mock<IDequeueCleanUpSchedulingService>();
            var target = new MarketOperatorDataDomainService(
                bundleRepositoryMock.Object,
                dataAvailableNotificationRepositoryMock.Object,
                requestDomainServiceMock.Object,
                contentTypeWeightMapMock.Object,
                operationServiceMock.Object);

            // Act
            var bundle = await target.GetNextUnacknowledgedTimeSeriesAsync(recipient, bundleId).ConfigureAwait(false);

            // Assert
            Assert.NotNull(bundle);
            Assert.Equal(setupBundle, bundle);
            Assert.True(bundle!.TryGetContent(out var actualBundleContent));
            Assert.Equal(bundleContentMock.Object, actualBundleContent);
        }

        [Fact]
        public async Task GetNextUnacknowledgedTimeSeriesAsync_HasBundleNotYetDequeuedCannotGetData_ReturnsNull()
        {
            // Arrange
            var recipient = new MarketOperator(new GlobalLocationNumber("fake_value"));
            var bundleId = new Uuid(Guid.NewGuid());
            var setupBundle = new Bundle(
                bundleId,
                recipient,
                DomainOrigin.TimeSeries,
                new ContentType("fake_value"),
                Array.Empty<Uuid>());

            var bundleRepositoryMock = new Mock<IBundleRepository>();
            bundleRepositoryMock
                .Setup(x => x.GetNextUnacknowledgedAsync(recipient, DomainOrigin.TimeSeries))
                .ReturnsAsync(setupBundle);

            var requestDomainServiceMock = new Mock<IRequestBundleDomainService>();
            requestDomainServiceMock
                .Setup(x => x.WaitForBundleContentFromSubDomainAsync(setupBundle))
                .ReturnsAsync((IBundleContent?)null);

            var dataAvailableNotificationRepositoryMock = new Mock<IDataAvailableNotificationRepository>();
            var contentTypeWeightMapMock = new Mock<IWeightCalculatorDomainService>();
            var operationServiceMock = new Mock<IDequeueCleanUpSchedulingService>();
            var target = new MarketOperatorDataDomainService(
                bundleRepositoryMock.Object,
                dataAvailableNotificationRepositoryMock.Object,
                requestDomainServiceMock.Object,
                contentTypeWeightMapMock.Object,
                operationServiceMock.Object);

            // Act
            var bundle = await target.GetNextUnacknowledgedTimeSeriesAsync(recipient, bundleId).ConfigureAwait(false);

            // Assert
            Assert.Null(bundle);
        }

        [Fact]
        public async Task GetNextUnacknowledgedTimeSeriesAsync_BundlingNotSupported_ReturnsBundleWithSingleNotification()
        {
            // Arrange
            var recipient = new MarketOperator(new GlobalLocationNumber("fake_value"));
            var bundleId = new Uuid("7dfb2080-fb56-4a37-a85d-1ac2f1559b45");
            var contentType = new ContentType("timeseries");

            var dataAvailableNotification = new DataAvailableNotification(
                new Uuid(Guid.NewGuid()),
                recipient,
                contentType,
                DomainOrigin.TimeSeries,
                new SupportsBundling(false),
                new Weight(1),
                new SequenceNumber(1));

            var dataAvailableNotificationRepositoryMock = new Mock<IDataAvailableNotificationRepository>();
            dataAvailableNotificationRepositoryMock
                .Setup(x => x.GetNextUnacknowledgedAsync(recipient, DomainOrigin.TimeSeries))
                .ReturnsAsync(dataAvailableNotification);

            var weight = new Weight(100);

            var contentTypeWeightMapMock = new Mock<IWeightCalculatorDomainService>();
            contentTypeWeightMapMock
                .Setup(x => x.CalculateMaxWeight(DomainOrigin.TimeSeries))
                .Returns(weight);

            var requestDomainServiceMock = new Mock<IRequestBundleDomainService>();
            var bundleContentMock = new Mock<IBundleContent>();
            var operationServiceMock = new Mock<IDequeueCleanUpSchedulingService>();

            var bundleRepositoryMock = new Mock<IBundleRepository>();
            bundleRepositoryMock
                .Setup(x => x.GetNextUnacknowledgedAsync(recipient, DomainOrigin.TimeSeries))
                .ReturnsAsync((Bundle?)null);

            bundleRepositoryMock
                .Setup(x => x.TryAddNextUnacknowledgedAsync(It.IsAny<Bundle>()))
                .ReturnsAsync(BundleCreatedResponse.Success);

            requestDomainServiceMock
                .Setup(x => x.WaitForBundleContentFromSubDomainAsync(It.IsAny<Bundle>()))
                .ReturnsAsync(bundleContentMock.Object);

            var target = new MarketOperatorDataDomainService(
                bundleRepositoryMock.Object,
                dataAvailableNotificationRepositoryMock.Object,
                requestDomainServiceMock.Object,
                contentTypeWeightMapMock.Object,
                operationServiceMock.Object);

            // Act
            var bundle = await target.GetNextUnacknowledgedTimeSeriesAsync(recipient, bundleId).ConfigureAwait(false);

            // Assert
            Assert.NotNull(bundle);
            Assert.Equal(dataAvailableNotification.Recipient, bundle!.Recipient);
            Assert.Equal(dataAvailableNotification.Origin, bundle.Origin);
            Assert.True(bundle.TryGetContent(out var actualBundleContent));
            Assert.Equal(bundleContentMock.Object, actualBundleContent);
        }

        [Fact]
        public async Task GetNextUnacknowledgedTimeSeriesAsync_TooLargeToBundle_ReturnsBundleWithSingleNotification()
        {
            // Arrange
            var recipient = new MarketOperator(new GlobalLocationNumber("fake_value"));
            var bundleId = new Uuid("7dfb2080-fb56-4a37-a85d-1ac2f1559b45");
            var contentType = new ContentType("timeseries");

            var dataAvailableNotification = new DataAvailableNotification(
                new Uuid(Guid.NewGuid()),
                recipient,
                contentType,
                DomainOrigin.TimeSeries,
                new SupportsBundling(true),
                new Weight(int.MaxValue),
                new SequenceNumber(1));

            var dataAvailableNotificationRepositoryMock = new Mock<IDataAvailableNotificationRepository>();
            dataAvailableNotificationRepositoryMock
                .Setup(x => x.GetNextUnacknowledgedAsync(recipient, DomainOrigin.TimeSeries))
                .ReturnsAsync(dataAvailableNotification);

            var weight = new Weight(100);

            var contentTypeWeightMapMock = new Mock<IWeightCalculatorDomainService>();
            contentTypeWeightMapMock
                .Setup(x => x.CalculateMaxWeight(DomainOrigin.TimeSeries))
                .Returns(weight);

            var requestDomainServiceMock = new Mock<IRequestBundleDomainService>();
            var bundleContentMock = new Mock<IBundleContent>();
            var operationServiceMock = new Mock<IDequeueCleanUpSchedulingService>();

            var bundleRepositoryMock = new Mock<IBundleRepository>();
            bundleRepositoryMock
                .Setup(x => x.GetNextUnacknowledgedAsync(recipient, DomainOrigin.TimeSeries))
                .ReturnsAsync((Bundle?)null);

            bundleRepositoryMock
                .Setup(x => x.TryAddNextUnacknowledgedAsync(It.IsAny<Bundle>()))
                .ReturnsAsync(BundleCreatedResponse.Success);

            requestDomainServiceMock
                .Setup(x => x.WaitForBundleContentFromSubDomainAsync(It.IsAny<Bundle>()))
                .ReturnsAsync(bundleContentMock.Object);

            var target = new MarketOperatorDataDomainService(
                bundleRepositoryMock.Object,
                dataAvailableNotificationRepositoryMock.Object,
                requestDomainServiceMock.Object,
                contentTypeWeightMapMock.Object,
                operationServiceMock.Object);

            // Act
            var bundle = await target.GetNextUnacknowledgedTimeSeriesAsync(recipient, bundleId).ConfigureAwait(false);

            // Assert
            Assert.NotNull(bundle);
            Assert.Equal(dataAvailableNotification.Recipient, bundle!.Recipient);
            Assert.Equal(dataAvailableNotification.Origin, bundle.Origin);
            Assert.True(bundle.TryGetContent(out var actualBundleContent));
            Assert.Equal(bundleContentMock.Object, actualBundleContent);
        }

        [Fact]
        public async Task GetNextUnacknowledgedAggregationsAsync_NoNotificationsReady_ReturnsNull()
        {
            // Arrange
            var recipient = new MarketOperator(new GlobalLocationNumber("fake_value"));
            var bundleId = new Uuid(Guid.NewGuid());

            var dataAvailableNotificationRepositoryMock = new Mock<IDataAvailableNotificationRepository>();
            dataAvailableNotificationRepositoryMock
                .Setup(x => x.GetNextUnacknowledgedAsync(recipient, DomainOrigin.Aggregations))
                .ReturnsAsync((DataAvailableNotification?)null);

            var bundleRepositoryMock = new Mock<IBundleRepository>();
            bundleRepositoryMock
                .Setup(x => x.GetNextUnacknowledgedAsync(recipient, DomainOrigin.Aggregations))
                .ReturnsAsync((Bundle?)null);

            var requestDomainServiceMock = new Mock<IRequestBundleDomainService>();
            var contentTypeWeightMap = new Mock<IWeightCalculatorDomainService>();
            var operationServiceMock = new Mock<IDequeueCleanUpSchedulingService>();

            var target = new MarketOperatorDataDomainService(
                bundleRepositoryMock.Object,
                dataAvailableNotificationRepositoryMock.Object,
                requestDomainServiceMock.Object,
                contentTypeWeightMap.Object,
                operationServiceMock.Object);

            // Act
            var bundle = await target.GetNextUnacknowledgedAggregationsAsync(recipient, bundleId).ConfigureAwait(false);

            // Assert
            Assert.Null(bundle);
        }

        [Fact]
        public async Task GetNextUnacknowledgedAggregationsAsync_AggregationsHasNotificationsButCannotTryAdd_ReturnsNull()
        {
            // Arrange
            var recipient = new MarketOperator(new GlobalLocationNumber("fake_value"));
            var bundleId = new Uuid(Guid.NewGuid());
            var contentType = new ContentType("aggregations");

            var dataAvailableNotificationFirst = CreateDataAvailableNotification(recipient, contentType, DomainOrigin.Aggregations);
            var allDataAvailableNotificationsForMessageType = new[]
            {
                dataAvailableNotificationFirst,
                CreateDataAvailableNotification(recipient, contentType),
                CreateDataAvailableNotification(recipient, contentType),
                CreateDataAvailableNotification(recipient, contentType)
            };

            var dataAvailableNotificationRepositoryMock = new Mock<IDataAvailableNotificationRepository>();
            dataAvailableNotificationRepositoryMock
                .Setup(x => x.GetNextUnacknowledgedAsync(recipient, DomainOrigin.Aggregations))
                .ReturnsAsync(dataAvailableNotificationFirst);

            var weight = new Weight(1);

            var contentTypeWeightMapMock = new Mock<IWeightCalculatorDomainService>();
            contentTypeWeightMapMock
                .Setup(x => x.CalculateMaxWeight(DomainOrigin.Aggregations))
                .Returns(weight);

            dataAvailableNotificationRepositoryMock
                .Setup(x => x.GetNextUnacknowledgedAsync(recipient, DomainOrigin.Aggregations, contentType, weight))
                .ReturnsAsync(allDataAvailableNotificationsForMessageType);

            var requestDomainServiceMock = new Mock<IRequestBundleDomainService>();
            var operationServiceMock = new Mock<IDequeueCleanUpSchedulingService>();

            var bundleRepositoryMock = new Mock<IBundleRepository>();
            bundleRepositoryMock
                .Setup(x => x.GetNextUnacknowledgedAsync(recipient, DomainOrigin.Aggregations))
                .ReturnsAsync((Bundle?)null);

            bundleRepositoryMock
                .Setup(x => x.TryAddNextUnacknowledgedAsync(It.IsAny<Bundle>()))
                .ReturnsAsync(BundleCreatedResponse.AnotherBundleExists);

            var target = new MarketOperatorDataDomainService(
                bundleRepositoryMock.Object,
                dataAvailableNotificationRepositoryMock.Object,
                requestDomainServiceMock.Object,
                contentTypeWeightMapMock.Object,
                operationServiceMock.Object);

            // Act
            var bundle = await target.GetNextUnacknowledgedAggregationsAsync(recipient, bundleId).ConfigureAwait(false);

            // Assert
            Assert.Null(bundle);
        }

        [Fact]
        public async Task GetNextUnacknowledgedAggregationsAsync_AggregationsHasNotificationsReady_ReturnsBundle()
        {
            // Arrange
            var recipient = new MarketOperator(new GlobalLocationNumber("fake_value"));
            var bundleId = new Uuid(Guid.NewGuid());
            var contentType = new ContentType("aggregations");

            var dataAvailableNotificationFirst = CreateDataAvailableNotification(recipient, contentType, DomainOrigin.Aggregations);
            var allDataAvailableNotificationsForMessageType = new[]
            {
                dataAvailableNotificationFirst,
                CreateDataAvailableNotification(recipient, contentType),
                CreateDataAvailableNotification(recipient, contentType),
                CreateDataAvailableNotification(recipient, contentType)
            };

            var dataAvailableNotificationRepositoryMock = new Mock<IDataAvailableNotificationRepository>();
            dataAvailableNotificationRepositoryMock
                .Setup(x => x.GetNextUnacknowledgedAsync(recipient, DomainOrigin.Aggregations))
                .ReturnsAsync(dataAvailableNotificationFirst);

            var weight = new Weight(1);

            var contentTypeWeightMapMock = new Mock<IWeightCalculatorDomainService>();
            contentTypeWeightMapMock
                .Setup(x => x.CalculateMaxWeight(DomainOrigin.Aggregations))
                .Returns(weight);

            dataAvailableNotificationRepositoryMock
                .Setup(x => x.GetNextUnacknowledgedAsync(recipient, DomainOrigin.Aggregations, contentType, weight))
                .ReturnsAsync(allDataAvailableNotificationsForMessageType);

            var requestDomainServiceMock = new Mock<IRequestBundleDomainService>();
            var bundleContentMock = new Mock<IBundleContent>();
            var operationServiceMock = new Mock<IDequeueCleanUpSchedulingService>();

            var bundleRepositoryMock = new Mock<IBundleRepository>();
            bundleRepositoryMock
                .Setup(x => x.GetNextUnacknowledgedAsync(recipient, DomainOrigin.Aggregations))
                .ReturnsAsync((Bundle?)null);

            bundleRepositoryMock
                .Setup(x => x.TryAddNextUnacknowledgedAsync(It.IsAny<Bundle>()))
                .ReturnsAsync(BundleCreatedResponse.Success);

            requestDomainServiceMock
                .Setup(x => x.WaitForBundleContentFromSubDomainAsync(It.IsAny<Bundle>()))
                .ReturnsAsync(bundleContentMock.Object);

            var target = new MarketOperatorDataDomainService(
                bundleRepositoryMock.Object,
                dataAvailableNotificationRepositoryMock.Object,
                requestDomainServiceMock.Object,
                contentTypeWeightMapMock.Object,
                operationServiceMock.Object);

            // Act
            var bundle = await target.GetNextUnacknowledgedAggregationsAsync(recipient, bundleId).ConfigureAwait(false);

            // Assert
            Assert.NotNull(bundle);
            Assert.Equal(dataAvailableNotificationFirst.Recipient, bundle!.Recipient);
            Assert.Equal(dataAvailableNotificationFirst.Origin, bundle.Origin);
            Assert.True(bundle.TryGetContent(out var actualBundleContent));
            Assert.Equal(bundleContentMock.Object, actualBundleContent);
        }

        [Fact]
        public async Task GetNextUnacknowledgedAggregationsAsync_AggregationsHasBundleNotYetDequeued_ReturnsThatPreviousBundle()
        {
            // Arrange
            var recipient = new MarketOperator(new GlobalLocationNumber("fake_value"));
            var bundleId = new Uuid(Guid.NewGuid());
            var dataAvailableNotificationRepositoryMock = new Mock<IDataAvailableNotificationRepository>();

            var bundleContentMock = new Mock<IBundleContent>();
            var setupBundle = new Bundle(
                bundleId,
                recipient,
                DomainOrigin.Aggregations,
                new ContentType("fake_value"),
                Array.Empty<Uuid>(),
                bundleContentMock.Object);

            var bundleRepositoryMock = new Mock<IBundleRepository>();
            bundleRepositoryMock
                .Setup(x => x.GetNextUnacknowledgedAsync(recipient, DomainOrigin.Aggregations))
                .ReturnsAsync(setupBundle);

            var contentTypeWeightMapMock = new Mock<IWeightCalculatorDomainService>();
            var requestDomainServiceMock = new Mock<IRequestBundleDomainService>();
            var operationServiceMock = new Mock<IDequeueCleanUpSchedulingService>();

            var target = new MarketOperatorDataDomainService(
                bundleRepositoryMock.Object,
                dataAvailableNotificationRepositoryMock.Object,
                requestDomainServiceMock.Object,
                contentTypeWeightMapMock.Object,
                operationServiceMock.Object);

            // Act
            var bundle = await target.GetNextUnacknowledgedAggregationsAsync(recipient, bundleId).ConfigureAwait(false);

            // Assert
            Assert.Equal(setupBundle, bundle);
        }

        [Fact]
        public async Task GetNextUnacknowledgedAggregationsAsync_AggreggationsHasBundleNotYetDequeuedWithNoData_ReturnsBundle()
        {
            // Arrange
            var recipient = new MarketOperator(new GlobalLocationNumber("fake_value"));
            var bundleId = new Uuid(Guid.NewGuid());

            var bundleContentMock = new Mock<IBundleContent>();
            var setupBundle = new Bundle(
                bundleId,
                recipient,
                DomainOrigin.Aggregations,
                new ContentType("fake_value"),
                Array.Empty<Uuid>());

            var bundleRepositoryMock = new Mock<IBundleRepository>();
            bundleRepositoryMock
                .Setup(x => x.GetNextUnacknowledgedAsync(recipient, DomainOrigin.Aggregations))
                .ReturnsAsync(setupBundle);

            var requestDomainServiceMock = new Mock<IRequestBundleDomainService>();
            requestDomainServiceMock
                .Setup(x => x.WaitForBundleContentFromSubDomainAsync(setupBundle))
                .ReturnsAsync(bundleContentMock.Object);

            var dataAvailableNotificationRepositoryMock = new Mock<IDataAvailableNotificationRepository>();
            var contentTypeWeightMapMock = new Mock<IWeightCalculatorDomainService>();
            var operationServiceMock = new Mock<IDequeueCleanUpSchedulingService>();
            var target = new MarketOperatorDataDomainService(
                bundleRepositoryMock.Object,
                dataAvailableNotificationRepositoryMock.Object,
                requestDomainServiceMock.Object,
                contentTypeWeightMapMock.Object,
                operationServiceMock.Object);

            // Act
            var bundle = await target.GetNextUnacknowledgedAggregationsAsync(recipient, bundleId).ConfigureAwait(false);

            // Assert
            Assert.NotNull(bundle);
            Assert.Equal(setupBundle, bundle);
            Assert.True(bundle!.TryGetContent(out var actualBundleContent));
            Assert.Equal(bundleContentMock.Object, actualBundleContent);
        }

        [Fact]
        public async Task GetNextUnacknowledgedAggregationsAsync_AggreggationsHasBundleNotYetDequeuedCannotGetData_ReturnsNull()
        {
            // Arrange
            var recipient = new MarketOperator(new GlobalLocationNumber("fake_value"));
            var bundleId = new Uuid(Guid.NewGuid());
            var setupBundle = new Bundle(
                bundleId,
                recipient,
                DomainOrigin.Aggregations,
                new ContentType("fake_value"),
                Array.Empty<Uuid>());

            var bundleRepositoryMock = new Mock<IBundleRepository>();
            bundleRepositoryMock
                .Setup(x => x.GetNextUnacknowledgedAsync(recipient, DomainOrigin.Aggregations))
                .ReturnsAsync(setupBundle);

            var requestDomainServiceMock = new Mock<IRequestBundleDomainService>();
            requestDomainServiceMock
                .Setup(x => x.WaitForBundleContentFromSubDomainAsync(setupBundle))
                .ReturnsAsync((IBundleContent?)null);

            var dataAvailableNotificationRepositoryMock = new Mock<IDataAvailableNotificationRepository>();
            var contentTypeWeightMapMock = new Mock<IWeightCalculatorDomainService>();
            var operationServiceMock = new Mock<IDequeueCleanUpSchedulingService>();
            var target = new MarketOperatorDataDomainService(
                bundleRepositoryMock.Object,
                dataAvailableNotificationRepositoryMock.Object,
                requestDomainServiceMock.Object,
                contentTypeWeightMapMock.Object,
                operationServiceMock.Object);

            // Act
            var bundle = await target.GetNextUnacknowledgedAggregationsAsync(recipient, bundleId).ConfigureAwait(false);

            // Assert
            Assert.Null(bundle);
        }

        [Fact]
        public async Task GetNextUnacknowledgedAggregationsAsync_BundlingNotSupported_ReturnsBundleWithSingleNotification()
        {
            // Arrange
            var recipient = new MarketOperator(new GlobalLocationNumber("fake_value"));
            var bundleId = new Uuid("7dfb2080-fb56-4a37-a85d-1ac2f1559b45");
            var contentType = new ContentType("aggregations");

            var dataAvailableNotification = new DataAvailableNotification(
                new Uuid(Guid.NewGuid()),
                recipient,
                contentType,
                DomainOrigin.Aggregations,
                new SupportsBundling(false),
                new Weight(1),
                new SequenceNumber(1));

            var dataAvailableNotificationRepositoryMock = new Mock<IDataAvailableNotificationRepository>();
            dataAvailableNotificationRepositoryMock
                .Setup(x => x.GetNextUnacknowledgedAsync(recipient, DomainOrigin.Aggregations))
                .ReturnsAsync(dataAvailableNotification);

            var weight = new Weight(100);

            var contentTypeWeightMapMock = new Mock<IWeightCalculatorDomainService>();
            contentTypeWeightMapMock
                .Setup(x => x.CalculateMaxWeight(DomainOrigin.Aggregations))
                .Returns(weight);

            var requestDomainServiceMock = new Mock<IRequestBundleDomainService>();
            var bundleContentMock = new Mock<IBundleContent>();
            var operationServiceMock = new Mock<IDequeueCleanUpSchedulingService>();

            var bundleRepositoryMock = new Mock<IBundleRepository>();
            bundleRepositoryMock
                .Setup(x => x.GetNextUnacknowledgedAsync(recipient, DomainOrigin.Aggregations))
                .ReturnsAsync((Bundle?)null);

            bundleRepositoryMock
                .Setup(x => x.TryAddNextUnacknowledgedAsync(It.IsAny<Bundle>()))
                .ReturnsAsync(BundleCreatedResponse.Success);

            requestDomainServiceMock
                .Setup(x => x.WaitForBundleContentFromSubDomainAsync(It.IsAny<Bundle>()))
                .ReturnsAsync(bundleContentMock.Object);

            var target = new MarketOperatorDataDomainService(
                bundleRepositoryMock.Object,
                dataAvailableNotificationRepositoryMock.Object,
                requestDomainServiceMock.Object,
                contentTypeWeightMapMock.Object,
                operationServiceMock.Object);

            // Act
            var bundle = await target.GetNextUnacknowledgedAggregationsAsync(recipient, bundleId).ConfigureAwait(false);

            // Assert
            Assert.NotNull(bundle);
            Assert.Equal(dataAvailableNotification.Recipient, bundle!.Recipient);
            Assert.Equal(dataAvailableNotification.Origin, bundle.Origin);
            Assert.True(bundle.TryGetContent(out var actualBundleContent));
            Assert.Equal(bundleContentMock.Object, actualBundleContent);
        }

        [Fact]
        public async Task GetNextUnacknowledgedAggregationsAsync_TooLargeToBundle_ReturnsBundleWithSingleNotification()
        {
            // Arrange
            var recipient = new MarketOperator(new GlobalLocationNumber("fake_value"));
            var bundleId = new Uuid("7dfb2080-fb56-4a37-a85d-1ac2f1559b45");
            var contentType = new ContentType("aggregations");

            var dataAvailableNotification = new DataAvailableNotification(
                new Uuid(Guid.NewGuid()),
                recipient,
                contentType,
                DomainOrigin.Aggregations,
                new SupportsBundling(true),
                new Weight(int.MaxValue),
                new SequenceNumber(1));

            var dataAvailableNotificationRepositoryMock = new Mock<IDataAvailableNotificationRepository>();
            dataAvailableNotificationRepositoryMock
                .Setup(x => x.GetNextUnacknowledgedAsync(recipient, DomainOrigin.Aggregations))
                .ReturnsAsync(dataAvailableNotification);

            var weight = new Weight(100);

            var contentTypeWeightMapMock = new Mock<IWeightCalculatorDomainService>();
            contentTypeWeightMapMock
                .Setup(x => x.CalculateMaxWeight(DomainOrigin.Aggregations))
                .Returns(weight);

            var requestDomainServiceMock = new Mock<IRequestBundleDomainService>();
            var bundleContentMock = new Mock<IBundleContent>();
            var operationServiceMock = new Mock<IDequeueCleanUpSchedulingService>();

            var bundleRepositoryMock = new Mock<IBundleRepository>();
            bundleRepositoryMock
                .Setup(x => x.GetNextUnacknowledgedAsync(recipient, DomainOrigin.Aggregations))
                .ReturnsAsync((Bundle?)null);

            bundleRepositoryMock
                .Setup(x => x.TryAddNextUnacknowledgedAsync(It.IsAny<Bundle>()))
                .ReturnsAsync(BundleCreatedResponse.Success);

            requestDomainServiceMock
                .Setup(x => x.WaitForBundleContentFromSubDomainAsync(It.IsAny<Bundle>()))
                .ReturnsAsync(bundleContentMock.Object);

            var target = new MarketOperatorDataDomainService(
                bundleRepositoryMock.Object,
                dataAvailableNotificationRepositoryMock.Object,
                requestDomainServiceMock.Object,
                contentTypeWeightMapMock.Object,
                operationServiceMock.Object);

            // Act
            var bundle = await target.GetNextUnacknowledgedAggregationsAsync(recipient, bundleId).ConfigureAwait(false);

            // Assert
            Assert.NotNull(bundle);
            Assert.Equal(dataAvailableNotification.Recipient, bundle!.Recipient);
            Assert.Equal(dataAvailableNotification.Origin, bundle.Origin);
            Assert.True(bundle.TryGetContent(out var actualBundleContent));
            Assert.Equal(bundleContentMock.Object, actualBundleContent);
        }

        [Fact]
        public async Task GetNextUnacknowledgedMasterDataAsync_NoNotificationsReady_ReturnsNull()
        {
            // Arrange
            var recipient = new MarketOperator(new GlobalLocationNumber("fake_value"));
            var bundleId = new Uuid(Guid.NewGuid());

            var dataAvailableNotificationRepositoryMock = new Mock<IDataAvailableNotificationRepository>();
            dataAvailableNotificationRepositoryMock
                .Setup(x =>
                    x.GetNextUnacknowledgedAsync(
                        recipient,
                        DomainOrigin.MarketRoles,
                        DomainOrigin.MeteringPoints,
                        DomainOrigin.Charges))
                .ReturnsAsync((DataAvailableNotification?)null);

            var bundleRepositoryMock = new Mock<IBundleRepository>();
            bundleRepositoryMock
                .Setup(x =>
                    x.GetNextUnacknowledgedAsync(
                        recipient,
                        DomainOrigin.MarketRoles,
                        DomainOrigin.MeteringPoints,
                        DomainOrigin.Charges))
                .ReturnsAsync((Bundle?)null);

            var requestDomainServiceMock = new Mock<IRequestBundleDomainService>();
            var contentTypeWeightMap = new Mock<IWeightCalculatorDomainService>();
            var operationServiceMock = new Mock<IDequeueCleanUpSchedulingService>();

            var target = new MarketOperatorDataDomainService(
                bundleRepositoryMock.Object,
                dataAvailableNotificationRepositoryMock.Object,
                requestDomainServiceMock.Object,
                contentTypeWeightMap.Object,
                operationServiceMock.Object);

            // Act
            var bundle = await target.GetNextUnacknowledgedMasterDataAsync(recipient, bundleId).ConfigureAwait(false);

            // Assert
            Assert.Null(bundle);
        }

        [Fact]
        public async Task GetNextUnacknowledgedMasterDataAsync_MarketRolesHasNotificationsButCannotTryAdd_ReturnsNull()
        {
            // Arrange
            var recipient = new MarketOperator(new GlobalLocationNumber("fake_value"));
            var bundleId = new Uuid(Guid.NewGuid());
            var contentType = new ContentType("marketroles");

            var dataAvailableNotificationFirst = CreateDataAvailableNotification(recipient, contentType, DomainOrigin.MarketRoles);
            var allDataAvailableNotificationsForMessageType = new[]
            {
                dataAvailableNotificationFirst,
                CreateDataAvailableNotification(recipient, contentType),
                CreateDataAvailableNotification(recipient, contentType),
                CreateDataAvailableNotification(recipient, contentType)
            };

            var dataAvailableNotificationRepositoryMock = new Mock<IDataAvailableNotificationRepository>();
            dataAvailableNotificationRepositoryMock
                .Setup(x =>
                    x.GetNextUnacknowledgedAsync(
                        recipient,
                        DomainOrigin.MarketRoles,
                        DomainOrigin.MeteringPoints,
                        DomainOrigin.Charges))
                .ReturnsAsync(dataAvailableNotificationFirst);

            var weight = new Weight(1);

            var contentTypeWeightMapMock = new Mock<IWeightCalculatorDomainService>();
            contentTypeWeightMapMock
                .Setup(x => x.CalculateMaxWeight(DomainOrigin.MarketRoles))
                .Returns(weight);

            dataAvailableNotificationRepositoryMock
                .Setup(x => x.GetNextUnacknowledgedAsync(recipient, DomainOrigin.Aggregations, contentType, weight))
                .ReturnsAsync(allDataAvailableNotificationsForMessageType);

            var requestDomainServiceMock = new Mock<IRequestBundleDomainService>();
            var operationServiceMock = new Mock<IDequeueCleanUpSchedulingService>();

            var bundleRepositoryMock = new Mock<IBundleRepository>();
            bundleRepositoryMock
                .Setup(x =>
                    x.GetNextUnacknowledgedAsync(
                        recipient,
                        DomainOrigin.MarketRoles,
                        DomainOrigin.MeteringPoints,
                        DomainOrigin.Charges))
                .ReturnsAsync((Bundle?)null);

            bundleRepositoryMock
                .Setup(x => x.TryAddNextUnacknowledgedAsync(It.IsAny<Bundle>()))
                .ReturnsAsync(BundleCreatedResponse.AnotherBundleExists);

            var target = new MarketOperatorDataDomainService(
                bundleRepositoryMock.Object,
                dataAvailableNotificationRepositoryMock.Object,
                requestDomainServiceMock.Object,
                contentTypeWeightMapMock.Object,
                operationServiceMock.Object);

            // Act
            var bundle = await target.GetNextUnacknowledgedMasterDataAsync(recipient, bundleId).ConfigureAwait(false);

            // Assert
            Assert.Null(bundle);
        }

        [Fact]
        public async Task GetNextUnacknowledgedMasterDataAsync_MeteringPointsHasNotificationsButCannotTryAdd_ReturnsNull()
        {
            // Arrange
            var recipient = new MarketOperator(new GlobalLocationNumber("fake_value"));
            var bundleId = new Uuid(Guid.NewGuid());
            var contentType = new ContentType("meteringpoints");

            var dataAvailableNotificationFirst = CreateDataAvailableNotification(recipient, contentType, DomainOrigin.MeteringPoints);
            var allDataAvailableNotificationsForMessageType = new[]
            {
                dataAvailableNotificationFirst,
                CreateDataAvailableNotification(recipient, contentType),
                CreateDataAvailableNotification(recipient, contentType),
                CreateDataAvailableNotification(recipient, contentType)
            };

            var dataAvailableNotificationRepositoryMock = new Mock<IDataAvailableNotificationRepository>();
            dataAvailableNotificationRepositoryMock
                .Setup(x =>
                    x.GetNextUnacknowledgedAsync(
                        recipient,
                        DomainOrigin.MarketRoles,
                        DomainOrigin.MeteringPoints,
                        DomainOrigin.Charges))
                .ReturnsAsync(dataAvailableNotificationFirst);

            var weight = new Weight(1);

            var contentTypeWeightMapMock = new Mock<IWeightCalculatorDomainService>();
            contentTypeWeightMapMock
                .Setup(x => x.CalculateMaxWeight(DomainOrigin.MeteringPoints))
                .Returns(weight);

            dataAvailableNotificationRepositoryMock
                .Setup(x => x.GetNextUnacknowledgedAsync(recipient, DomainOrigin.Aggregations, contentType, weight))
                .ReturnsAsync(allDataAvailableNotificationsForMessageType);

            var requestDomainServiceMock = new Mock<IRequestBundleDomainService>();
            var operationServiceMock = new Mock<IDequeueCleanUpSchedulingService>();

            var bundleRepositoryMock = new Mock<IBundleRepository>();
            bundleRepositoryMock
                .Setup(x =>
                    x.GetNextUnacknowledgedAsync(
                        recipient,
                        DomainOrigin.MarketRoles,
                        DomainOrigin.MeteringPoints,
                        DomainOrigin.Charges))
                .ReturnsAsync((Bundle?)null);

            bundleRepositoryMock
                .Setup(x => x.TryAddNextUnacknowledgedAsync(It.IsAny<Bundle>()))
                .ReturnsAsync(BundleCreatedResponse.AnotherBundleExists);

            var target = new MarketOperatorDataDomainService(
                bundleRepositoryMock.Object,
                dataAvailableNotificationRepositoryMock.Object,
                requestDomainServiceMock.Object,
                contentTypeWeightMapMock.Object,
                operationServiceMock.Object);

            // Act
            var bundle = await target.GetNextUnacknowledgedMasterDataAsync(recipient, bundleId).ConfigureAwait(false);

            // Assert
            Assert.Null(bundle);
        }

        [Fact]
        public async Task GetNextUnacknowledgedMasterDataAsync_MarketRolesHasNotificationsReady_ReturnsBundle()
        {
            // Arrange
            var recipient = new MarketOperator(new GlobalLocationNumber("fake_value"));
            var bundleId = new Uuid(Guid.NewGuid());
            var contentType = new ContentType("marketroles");

            var dataAvailableNotificationFirst = CreateDataAvailableNotification(recipient, contentType, DomainOrigin.MarketRoles);
            var allDataAvailableNotificationsForMessageType = new[]
            {
                dataAvailableNotificationFirst,
                CreateDataAvailableNotification(recipient, contentType),
                CreateDataAvailableNotification(recipient, contentType),
                CreateDataAvailableNotification(recipient, contentType)
            };

            var dataAvailableNotificationRepositoryMock = new Mock<IDataAvailableNotificationRepository>();
            dataAvailableNotificationRepositoryMock
                .Setup(x =>
                    x.GetNextUnacknowledgedAsync(
                        recipient,
                        DomainOrigin.MarketRoles,
                        DomainOrigin.MeteringPoints,
                        DomainOrigin.Charges))
                .ReturnsAsync(dataAvailableNotificationFirst);

            var weight = new Weight(1);

            var contentTypeWeightMapMock = new Mock<IWeightCalculatorDomainService>();
            contentTypeWeightMapMock
                .Setup(x => x.CalculateMaxWeight(DomainOrigin.MarketRoles))
                .Returns(weight);

            dataAvailableNotificationRepositoryMock
                .Setup(x => x.GetNextUnacknowledgedAsync(recipient, DomainOrigin.Aggregations, contentType, weight))
                .ReturnsAsync(allDataAvailableNotificationsForMessageType);

            var requestDomainServiceMock = new Mock<IRequestBundleDomainService>();
            var bundleContentMock = new Mock<IBundleContent>();
            var operationServiceMock = new Mock<IDequeueCleanUpSchedulingService>();

            var bundleRepositoryMock = new Mock<IBundleRepository>();
            bundleRepositoryMock
                .Setup(x =>
                    x.GetNextUnacknowledgedAsync(
                        recipient,
                        DomainOrigin.MarketRoles,
                        DomainOrigin.MeteringPoints,
                        DomainOrigin.Charges))
                .ReturnsAsync((Bundle?)null);

            bundleRepositoryMock
                .Setup(x => x.TryAddNextUnacknowledgedAsync(It.IsAny<Bundle>()))
                .ReturnsAsync(BundleCreatedResponse.Success);

            requestDomainServiceMock
                .Setup(x => x.WaitForBundleContentFromSubDomainAsync(It.IsAny<Bundle>()))
                .ReturnsAsync(bundleContentMock.Object);

            var target = new MarketOperatorDataDomainService(
                bundleRepositoryMock.Object,
                dataAvailableNotificationRepositoryMock.Object,
                requestDomainServiceMock.Object,
                contentTypeWeightMapMock.Object,
                operationServiceMock.Object);

            // Act
            var bundle = await target.GetNextUnacknowledgedMasterDataAsync(recipient, bundleId).ConfigureAwait(false);

            // Assert
            Assert.NotNull(bundle);
            Assert.Equal(dataAvailableNotificationFirst.Recipient, bundle!.Recipient);
            Assert.Equal(dataAvailableNotificationFirst.Origin, bundle.Origin);
            Assert.True(bundle.TryGetContent(out var actualBundleContent));
            Assert.Equal(bundleContentMock.Object, actualBundleContent);
        }

        [Fact]
        public async Task GetNextUnacknowledgedMasterDataAsync_MarketRolesHasBundleNotYetDequeued_ReturnsThatPreviousBundle()
        {
            // Arrange
            var recipient = new MarketOperator(new GlobalLocationNumber("fake_value"));
            var bundleId = new Uuid(Guid.NewGuid());
            var dataAvailableNotificationRepositoryMock = new Mock<IDataAvailableNotificationRepository>();

            var bundleContentMock = new Mock<IBundleContent>();
            var setupBundle = new Bundle(
                bundleId,
                recipient,
                DomainOrigin.MarketRoles,
                new ContentType("fake_value"),
                Array.Empty<Uuid>(),
                bundleContentMock.Object);

            var bundleRepositoryMock = new Mock<IBundleRepository>();
            bundleRepositoryMock
                .Setup(x =>
                    x.GetNextUnacknowledgedAsync(
                        recipient,
                        DomainOrigin.MarketRoles,
                        DomainOrigin.MeteringPoints,
                        DomainOrigin.Charges))
                .ReturnsAsync(setupBundle);

            var contentTypeWeightMapMock = new Mock<IWeightCalculatorDomainService>();
            var requestDomainServiceMock = new Mock<IRequestBundleDomainService>();
            var operationServiceMock = new Mock<IDequeueCleanUpSchedulingService>();
            var target = new MarketOperatorDataDomainService(
                bundleRepositoryMock.Object,
                dataAvailableNotificationRepositoryMock.Object,
                requestDomainServiceMock.Object,
                contentTypeWeightMapMock.Object,
                operationServiceMock.Object);

            // Act
            var bundle = await target.GetNextUnacknowledgedMasterDataAsync(recipient, bundleId).ConfigureAwait(false);

            // Assert
            Assert.Equal(setupBundle, bundle);
        }

        [Fact]
        public async Task GetNextUnacknowledgedMasterDataAsync_MarketRolesHasBundleNotYetDequeuedWithNoData_ReturnsBundle()
        {
            // Arrange
            var recipient = new MarketOperator(new GlobalLocationNumber("fake_value"));
            var bundleId = new Uuid(Guid.NewGuid());

            var bundleContentMock = new Mock<IBundleContent>();
            var setupBundle = new Bundle(
                bundleId,
                recipient,
                DomainOrigin.MarketRoles,
                new ContentType("fake_value"),
                Array.Empty<Uuid>());

            var bundleRepositoryMock = new Mock<IBundleRepository>();
            bundleRepositoryMock
                .Setup(x =>
                    x.GetNextUnacknowledgedAsync(
                        recipient,
                        DomainOrigin.MarketRoles,
                        DomainOrigin.MeteringPoints,
                        DomainOrigin.Charges))
                .ReturnsAsync(setupBundle);

            var requestDomainServiceMock = new Mock<IRequestBundleDomainService>();
            requestDomainServiceMock
                .Setup(x => x.WaitForBundleContentFromSubDomainAsync(setupBundle))
                .ReturnsAsync(bundleContentMock.Object);

            var dataAvailableNotificationRepositoryMock = new Mock<IDataAvailableNotificationRepository>();
            var contentTypeWeightMapMock = new Mock<IWeightCalculatorDomainService>();
            var operationServiceMock = new Mock<IDequeueCleanUpSchedulingService>();
            var target = new MarketOperatorDataDomainService(
                bundleRepositoryMock.Object,
                dataAvailableNotificationRepositoryMock.Object,
                requestDomainServiceMock.Object,
                contentTypeWeightMapMock.Object,
                operationServiceMock.Object);

            // Act
            var bundle = await target.GetNextUnacknowledgedMasterDataAsync(recipient, bundleId).ConfigureAwait(false);

            // Assert
            Assert.NotNull(bundle);
            Assert.Equal(setupBundle, bundle);
            Assert.True(bundle!.TryGetContent(out var actualBundleContent));
            Assert.Equal(bundleContentMock.Object, actualBundleContent);
        }

        [Fact]
        public async Task GetNextUnacknowledgedMasterDataAsync_MarketRolesHasBundleNotYetDequeuedCannotGetData_ReturnsNull()
        {
            // Arrange
            var recipient = new MarketOperator(new GlobalLocationNumber("fake_value"));
            var bundleId = new Uuid(Guid.NewGuid());
            var setupBundle = new Bundle(
                bundleId,
                recipient,
                DomainOrigin.MarketRoles,
                new ContentType("fake_value"),
                Array.Empty<Uuid>());

            var bundleRepositoryMock = new Mock<IBundleRepository>();
            bundleRepositoryMock
                .Setup(x =>
                    x.GetNextUnacknowledgedAsync(
                        recipient,
                        DomainOrigin.MarketRoles,
                        DomainOrigin.MeteringPoints,
                        DomainOrigin.Charges))
                .ReturnsAsync(setupBundle);

            var requestDomainServiceMock = new Mock<IRequestBundleDomainService>();
            requestDomainServiceMock
                .Setup(x => x.WaitForBundleContentFromSubDomainAsync(setupBundle))
                .ReturnsAsync((IBundleContent?)null);

            var dataAvailableNotificationRepositoryMock = new Mock<IDataAvailableNotificationRepository>();
            var contentTypeWeightMapMock = new Mock<IWeightCalculatorDomainService>();
            var operationServiceMock = new Mock<IDequeueCleanUpSchedulingService>();
            var target = new MarketOperatorDataDomainService(
                bundleRepositoryMock.Object,
                dataAvailableNotificationRepositoryMock.Object,
                requestDomainServiceMock.Object,
                contentTypeWeightMapMock.Object,
                operationServiceMock.Object);

            // Act
            var bundle = await target.GetNextUnacknowledgedMasterDataAsync(recipient, bundleId).ConfigureAwait(false);

            // Assert
            Assert.Null(bundle);
        }

        [Fact]
        public async Task GetNextUnacknowledgedMasterDataAsync_BundlingNotSupported_ReturnsBundleWithSingleNotification()
        {
            // Arrange
            var recipient = new MarketOperator(new GlobalLocationNumber("fake_value"));
            var bundleId = new Uuid("7dfb2080-fb56-4a37-a85d-1ac2f1559b45");
            var contentType = new ContentType("charges");

            var dataAvailableNotification = new DataAvailableNotification(
                new Uuid(Guid.NewGuid()),
                recipient,
                contentType,
                DomainOrigin.Charges,
                new SupportsBundling(false),
                new Weight(1),
                new SequenceNumber(1));

            var dataAvailableNotificationRepositoryMock = new Mock<IDataAvailableNotificationRepository>();
            dataAvailableNotificationRepositoryMock
                .Setup(x => x.GetNextUnacknowledgedAsync(
                    recipient,
                    DomainOrigin.MarketRoles,
                    DomainOrigin.MeteringPoints,
                    DomainOrigin.Charges))
                .ReturnsAsync(dataAvailableNotification);

            var weight = new Weight(100);

            var contentTypeWeightMapMock = new Mock<IWeightCalculatorDomainService>();
            contentTypeWeightMapMock
                .Setup(x => x.CalculateMaxWeight(DomainOrigin.Charges))
                .Returns(weight);

            var requestDomainServiceMock = new Mock<IRequestBundleDomainService>();
            var bundleContentMock = new Mock<IBundleContent>();
            var operationServiceMock = new Mock<IDequeueCleanUpSchedulingService>();

            var bundleRepositoryMock = new Mock<IBundleRepository>();
            bundleRepositoryMock
                .Setup(x => x.GetNextUnacknowledgedAsync(
                    recipient,
                    DomainOrigin.MarketRoles,
                    DomainOrigin.MeteringPoints,
                    DomainOrigin.Charges))
                .ReturnsAsync((Bundle?)null);

            bundleRepositoryMock
                .Setup(x => x.TryAddNextUnacknowledgedAsync(It.IsAny<Bundle>()))
                .ReturnsAsync(BundleCreatedResponse.Success);

            requestDomainServiceMock
                .Setup(x => x.WaitForBundleContentFromSubDomainAsync(It.IsAny<Bundle>()))
                .ReturnsAsync(bundleContentMock.Object);

            var target = new MarketOperatorDataDomainService(
                bundleRepositoryMock.Object,
                dataAvailableNotificationRepositoryMock.Object,
                requestDomainServiceMock.Object,
                contentTypeWeightMapMock.Object,
                operationServiceMock.Object);

            // Act
            var bundle = await target.GetNextUnacknowledgedMasterDataAsync(recipient, bundleId).ConfigureAwait(false);

            // Assert
            Assert.NotNull(bundle);
            Assert.Equal(dataAvailableNotification.Recipient, bundle!.Recipient);
            Assert.Equal(dataAvailableNotification.Origin, bundle.Origin);
            Assert.True(bundle.TryGetContent(out var actualBundleContent));
            Assert.Equal(bundleContentMock.Object, actualBundleContent);
        }

        [Fact]
        public async Task GetNextUnacknowledgedMasterDataAsync_TooLargeToBundle_ReturnsBundleWithSingleNotification()
        {
            // Arrange
            var recipient = new MarketOperator(new GlobalLocationNumber("fake_value"));
            var bundleId = new Uuid("7dfb2080-fb56-4a37-a85d-1ac2f1559b45");
            var contentType = new ContentType("charges");

            var dataAvailableNotification = new DataAvailableNotification(
                new Uuid(Guid.NewGuid()),
                recipient,
                contentType,
                DomainOrigin.Charges,
                new SupportsBundling(true),
                new Weight(int.MaxValue),
                new SequenceNumber(1));

            var dataAvailableNotificationRepositoryMock = new Mock<IDataAvailableNotificationRepository>();
            dataAvailableNotificationRepositoryMock
                .Setup(x => x.GetNextUnacknowledgedAsync(
                    recipient,
                    DomainOrigin.MarketRoles,
                    DomainOrigin.MeteringPoints,
                    DomainOrigin.Charges))
                .ReturnsAsync(dataAvailableNotification);

            var weight = new Weight(100);

            var contentTypeWeightMapMock = new Mock<IWeightCalculatorDomainService>();
            contentTypeWeightMapMock
                .Setup(x => x.CalculateMaxWeight(DomainOrigin.Charges))
                .Returns(weight);

            var requestDomainServiceMock = new Mock<IRequestBundleDomainService>();
            var bundleContentMock = new Mock<IBundleContent>();
            var operationServiceMock = new Mock<IDequeueCleanUpSchedulingService>();

            var bundleRepositoryMock = new Mock<IBundleRepository>();
            bundleRepositoryMock
                .Setup(x => x.GetNextUnacknowledgedAsync(
                    recipient,
                    DomainOrigin.MarketRoles,
                    DomainOrigin.MeteringPoints,
                    DomainOrigin.Charges))
                .ReturnsAsync((Bundle?)null);

            bundleRepositoryMock
                .Setup(x => x.TryAddNextUnacknowledgedAsync(It.IsAny<Bundle>()))
                .ReturnsAsync(BundleCreatedResponse.Success);

            requestDomainServiceMock
                .Setup(x => x.WaitForBundleContentFromSubDomainAsync(It.IsAny<Bundle>()))
                .ReturnsAsync(bundleContentMock.Object);

            var target = new MarketOperatorDataDomainService(
                bundleRepositoryMock.Object,
                dataAvailableNotificationRepositoryMock.Object,
                requestDomainServiceMock.Object,
                contentTypeWeightMapMock.Object,
                operationServiceMock.Object);

            // Act
            var bundle = await target.GetNextUnacknowledgedMasterDataAsync(recipient, bundleId).ConfigureAwait(false);

            // Assert
            Assert.NotNull(bundle);
            Assert.Equal(dataAvailableNotification.Recipient, bundle!.Recipient);
            Assert.Equal(dataAvailableNotification.Origin, bundle.Origin);
            Assert.True(bundle.TryGetContent(out var actualBundleContent));
            Assert.Equal(bundleContentMock.Object, actualBundleContent);
        }

        [Fact]
        public async Task CanAcknowledgeAsync_HasBundle_ReturnsTrue()
        {
            // Arrange
            var recipient = new MarketOperator(new GlobalLocationNumber("fake_value"));
            var bundleUuid = new Uuid("1E0A906E-8895-4C86-B4FC-48E9BAF2A2B6");
            var idsInBundle = new[]
            {
                new Uuid("5AA0BDE7-EAB7-408D-B4A4-BBF1EEFF3F7E"),
                new Uuid("7E188D0E-A923-4AD5-A7CB-39889884241B"),
                new Uuid("9DEA909A-179B-413B-A669-E38D4C812009"),
                new Uuid("B0425457-8E0A-4E66-80EF-2717562EAEA7")
            };

            var bundle = new Bundle(
                bundleUuid,
                recipient,
                DomainOrigin.TimeSeries,
                new ContentType("fake_value"),
                idsInBundle);

            var dataAvailableNotificationRepositoryMock = new Mock<IDataAvailableNotificationRepository>();

            var bundleRepositoryMock = new Mock<IBundleRepository>();
            bundleRepositoryMock
                .Setup(x => x.GetNextUnacknowledgedAsync(recipient))
                .ReturnsAsync(bundle);

            var contentTypeWeightMapMock = new Mock<IWeightCalculatorDomainService>();
            var requestDomainServiceMock = new Mock<IRequestBundleDomainService>();
            var operationServiceMock = new Mock<IDequeueCleanUpSchedulingService>();
            var target = new MarketOperatorDataDomainService(
                bundleRepositoryMock.Object,
                dataAvailableNotificationRepositoryMock.Object,
                requestDomainServiceMock.Object,
                contentTypeWeightMapMock.Object,
                operationServiceMock.Object);

            // Act
            var result = await target.CanAcknowledgeAsync(recipient, bundleUuid).ConfigureAwait(false);

            // Assert
            Assert.True(result.CanAcknowledge);
        }

        [Fact]
        public async Task CanAcknowledgeAsync_HasNoBundle_ReturnsFalse()
        {
            // Arrange
            var recipient = new MarketOperator(new GlobalLocationNumber("fake_value"));
            var bundleUuid = new Uuid("60D041F5-548B-49C0-8118-BB0F3DF1E692");
            var dataAvailableNotificationRepositoryMock = new Mock<IDataAvailableNotificationRepository>();

            var bundleRepositoryMock = new Mock<IBundleRepository>();
            bundleRepositoryMock
                .Setup(x => x.GetNextUnacknowledgedAsync(recipient))
                .ReturnsAsync((Bundle?)null);

            var contentTypeWeightMapMock = new Mock<IWeightCalculatorDomainService>();
            var requestDomainServiceMock = new Mock<IRequestBundleDomainService>();
            var operationServiceMock = new Mock<IDequeueCleanUpSchedulingService>();
            var target = new MarketOperatorDataDomainService(
                bundleRepositoryMock.Object,
                dataAvailableNotificationRepositoryMock.Object,
                requestDomainServiceMock.Object,
                contentTypeWeightMapMock.Object,
                operationServiceMock.Object);

            // Act
            var result = await target.CanAcknowledgeAsync(recipient, bundleUuid).ConfigureAwait(false);

            // Assert
            Assert.False(result.CanAcknowledge);
        }

        [Fact]
        public async Task CanAcknowledgeAsync_WrongId_ReturnsFalse()
        {
            // Arrange
            var recipient = new MarketOperator(new GlobalLocationNumber("fake_value"));
            var bundleUuid = new Uuid("60D041F5-548B-49C0-8118-BB0F3DF1E692");
            var incorrectId = new Uuid("8BF7791E-A179-4B86-AE2F-69B5C276E99F");
            var dataAvailableNotificationRepositoryMock = new Mock<IDataAvailableNotificationRepository>();

            var bundle = new Bundle(
                bundleUuid,
                recipient,
                DomainOrigin.TimeSeries,
                new ContentType("fake_value"),
                Array.Empty<Uuid>());

            var bundleRepositoryMock = new Mock<IBundleRepository>();
            bundleRepositoryMock
                .Setup(x => x.GetNextUnacknowledgedAsync(recipient))
                .ReturnsAsync(bundle);

            var contentTypeWeightMapMock = new Mock<IWeightCalculatorDomainService>();
            var requestDomainServiceMock = new Mock<IRequestBundleDomainService>();
            var operationServiceMock = new Mock<IDequeueCleanUpSchedulingService>();
            var target = new MarketOperatorDataDomainService(
                bundleRepositoryMock.Object,
                dataAvailableNotificationRepositoryMock.Object,
                requestDomainServiceMock.Object,
                contentTypeWeightMapMock.Object,
                operationServiceMock.Object);

            // Act
            var result = await target.CanAcknowledgeAsync(recipient, incorrectId).ConfigureAwait(false);

            // Assert
            Assert.False(result.CanAcknowledge);
        }

        [Fact]
        public async Task AcknowledgeAsync_HasBundle_Succeeds()
        {
            // Arrange
            var recipient = new MarketOperator(new GlobalLocationNumber("fake_value"));
            var bundleUuid = new Uuid("1E0A906E-8895-4C86-B4FC-48E9BAF2A2B6");
            var idsInBundle = new[]
            {
                new Uuid("5AA0BDE7-EAB7-408D-B4A4-BBF1EEFF3F7E"),
                new Uuid("7E188D0E-A923-4AD5-A7CB-39889884241B"),
                new Uuid("9DEA909A-179B-413B-A669-E38D4C812009"),
                new Uuid("B0425457-8E0A-4E66-80EF-2717562EAEA7")
            };

            var bundle = new Bundle(
                bundleUuid,
                recipient,
                DomainOrigin.TimeSeries,
                new ContentType("fake_value"),
                idsInBundle);

            var dataAvailableNotificationRepositoryMock = new Mock<IDataAvailableNotificationRepository>();
            var bundleRepositoryMock = new Mock<IBundleRepository>();

            var contentTypeWeightMapMock = new Mock<IWeightCalculatorDomainService>();
            var requestDomainServiceMock = new Mock<IRequestBundleDomainService>();
            var operationServiceMock = new Mock<IDequeueCleanUpSchedulingService>();
            var target = new MarketOperatorDataDomainService(
                bundleRepositoryMock.Object,
                dataAvailableNotificationRepositoryMock.Object,
                requestDomainServiceMock.Object,
                contentTypeWeightMapMock.Object,
                operationServiceMock.Object);

            // Act
            await target.AcknowledgeAsync(bundle).ConfigureAwait(false);

            // Assert
            bundleRepositoryMock.Verify(x => x.AcknowledgeAsync(recipient, bundleUuid), Times.Once);
            dataAvailableNotificationRepositoryMock.Verify(x => x.AcknowledgeAsync(recipient, idsInBundle), Times.Once);
        }

        [Fact]
        public async Task Acknowledge2Async_HasBundle_Succeeds()
        {
            // Arrange
            var recipient = new MarketOperator(new GlobalLocationNumber("fake_value"));
            var bundleUuid = new Uuid("1E0A906E-8895-4C86-B4FC-48E9BAF2A2B6");
            var idsInBundle = new[]
            {
                new Uuid("5AA0BDE7-EAB7-408D-B4A4-BBF1EEFF3F7E"),
                new Uuid("7E188D0E-A923-4AD5-A7CB-39889884241B"),
                new Uuid("9DEA909A-179B-413B-A669-E38D4C812009"),
                new Uuid("B0425457-8E0A-4E66-80EF-2717562EAEA7")
            };

            var bundle = new Bundle(
                bundleUuid,
                recipient,
                DomainOrigin.TimeSeries,
                new ContentType("fake_value"),
                idsInBundle);

            var dataAvailableNotificationRepositoryMock = new Mock<IDataAvailableNotificationRepository>();
            var bundleRepositoryMock = new Mock<IBundleRepository>();

            var contentTypeWeightMapMock = new Mock<IWeightCalculatorDomainService>();
            var requestDomainServiceMock = new Mock<IRequestBundleDomainService>();
            var operationServiceMock = new Mock<IDequeueCleanUpSchedulingService>();
            var target = new MarketOperatorDataDomainService(
                bundleRepositoryMock.Object,
                dataAvailableNotificationRepositoryMock.Object,
                requestDomainServiceMock.Object,
                contentTypeWeightMapMock.Object,
                operationServiceMock.Object);

            // Act
            await target.Acknowledge2Async(bundle).ConfigureAwait(false);

            // Assert
            bundleRepositoryMock.Verify(x => x.AcknowledgeAsync(recipient, bundleUuid), Times.Once);
            dataAvailableNotificationRepositoryMock.Verify(x => x.AcknowledgeAsync(bundle), Times.Once);
        }

        private static DataAvailableNotification CreateDataAvailableNotification(
            MarketOperator recipient,
            ContentType contentType,
            DomainOrigin domainOrigin = DomainOrigin.TimeSeries)
        {
            return new DataAvailableNotification(
                new Uuid(Guid.NewGuid()),
                recipient,
                contentType,
                domainOrigin,
                new SupportsBundling(true),
                new Weight(1),
                new SequenceNumber(1));
        }
    }
}
