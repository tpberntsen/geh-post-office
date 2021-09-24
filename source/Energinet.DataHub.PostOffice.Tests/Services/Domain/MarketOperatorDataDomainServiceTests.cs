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

            var target = new MarketOperatorDataDomainService(
                bundleRepositoryMock.Object,
                dataAvailableNotificationRepositoryMock.Object,
                requestDomainServiceMock.Object,
                contentTypeWeightMap.Object);

            // Act
            var bundle = await target.GetNextUnacknowledgedAsync(recipient).ConfigureAwait(false);

            // Assert
            Assert.Null(bundle);
        }

        [Fact]
        public async Task GetNextUnacknowledgedAsync_HasNotificationsButCannotTryAdd_ReturnsNull()
        {
            // Arrange
            var recipient = new MarketOperator(new GlobalLocationNumber("fake_value"));
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
                .Setup(x => x.GetNextUnacknowledgedAsync(recipient, contentType, weight))
                .ReturnsAsync(allDataAvailableNotificationsForMessageType);

            var requestDomainServiceMock = new Mock<IRequestBundleDomainService>();

            var bundleRepositoryMock = new Mock<IBundleRepository>();
            bundleRepositoryMock
                .Setup(x => x.GetNextUnacknowledgedAsync(recipient))
                .ReturnsAsync((Bundle?)null);

            bundleRepositoryMock
                .Setup(x => x.TryAddNextUnacknowledgedAsync(It.IsAny<Bundle>()))
                .ReturnsAsync(false);

            var target = new MarketOperatorDataDomainService(
                bundleRepositoryMock.Object,
                dataAvailableNotificationRepositoryMock.Object,
                requestDomainServiceMock.Object,
                contentTypeWeightMapMock.Object);

            // Act
            var bundle = await target.GetNextUnacknowledgedAsync(recipient).ConfigureAwait(false);

            // Assert
            Assert.Null(bundle);
        }

        [Fact]
        public async Task GetNextUnacknowledgedAsync_HasNotificationsReady_ReturnsBundle()
        {
            // Arrange
            var recipient = new MarketOperator(new GlobalLocationNumber("fake_value"));
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
                .Setup(x => x.GetNextUnacknowledgedAsync(recipient, contentType, weight))
                .ReturnsAsync(allDataAvailableNotificationsForMessageType);

            var requestDomainServiceMock = new Mock<IRequestBundleDomainService>();
            var bundleContentMock = new Mock<IBundleContent>();

            var bundleRepositoryMock = new Mock<IBundleRepository>();
            bundleRepositoryMock
                .Setup(x => x.GetNextUnacknowledgedAsync(recipient))
                .ReturnsAsync((Bundle?)null);

            bundleRepositoryMock
                .Setup(x => x.TryAddNextUnacknowledgedAsync(It.IsAny<Bundle>()))
                .ReturnsAsync(true);

            requestDomainServiceMock
                .Setup(x => x.WaitForBundleContentFromSubDomainAsync(It.IsAny<Bundle>()))
                .ReturnsAsync(bundleContentMock.Object);

            var target = new MarketOperatorDataDomainService(
                bundleRepositoryMock.Object,
                dataAvailableNotificationRepositoryMock.Object,
                requestDomainServiceMock.Object,
                contentTypeWeightMapMock.Object);

            // Act
            var bundle = await target.GetNextUnacknowledgedAsync(recipient).ConfigureAwait(false);

            // Assert
            Assert.NotNull(bundle);
            Assert.Equal(dataAvailableNotificationFirst.Recipient, bundle!.Recipient);
            Assert.Equal(dataAvailableNotificationFirst.Origin, bundle.Origin);
            Assert.True(bundle.TryGetContent(out var actualBundleContent));
            Assert.Equal(bundleContentMock.Object, actualBundleContent);
        }

        [Fact]
        public async Task GetNextUnacknowledgedAsync_HasBundleNotYetDequeued_ReturnsThatPreviousBundle()
        {
            // Arrange
            var recipient = new MarketOperator(new GlobalLocationNumber("fake_value"));
            var dataAvailableNotificationRepositoryMock = new Mock<IDataAvailableNotificationRepository>();

            var bundleContentMock = new Mock<IBundleContent>();
            var setupBundle = new Bundle(
                new Uuid(Guid.NewGuid()),
                DomainOrigin.TimeSeries,
                recipient,
                Array.Empty<Uuid>(),
                bundleContentMock.Object);

            var bundleRepositoryMock = new Mock<IBundleRepository>();
            bundleRepositoryMock
                .Setup(x => x.GetNextUnacknowledgedAsync(recipient))
                .ReturnsAsync(setupBundle);

            var contentTypeWeightMapMock = new Mock<IWeightCalculatorDomainService>();
            var requestDomainServiceMock = new Mock<IRequestBundleDomainService>();
            var target = new MarketOperatorDataDomainService(
                bundleRepositoryMock.Object,
                dataAvailableNotificationRepositoryMock.Object,
                requestDomainServiceMock.Object,
                contentTypeWeightMapMock.Object);

            // Act
            var bundle = await target.GetNextUnacknowledgedAsync(recipient).ConfigureAwait(false);

            // Assert
            Assert.Equal(setupBundle, bundle);
        }

        [Fact]
        public async Task GetNextUnacknowledgedAsync_HasBundleNotYetDequeuedWithNoData_ReturnsBundle()
        {
            // Arrange
            var recipient = new MarketOperator(new GlobalLocationNumber("fake_value"));

            var bundleContentMock = new Mock<IBundleContent>();
            var setupBundle = new Bundle(
                new Uuid(Guid.NewGuid()),
                DomainOrigin.TimeSeries,
                recipient,
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
            var target = new MarketOperatorDataDomainService(
                bundleRepositoryMock.Object,
                dataAvailableNotificationRepositoryMock.Object,
                requestDomainServiceMock.Object,
                contentTypeWeightMapMock.Object);

            // Act
            var bundle = await target.GetNextUnacknowledgedAsync(recipient).ConfigureAwait(false);

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
            var setupBundle = new Bundle(
                new Uuid(Guid.NewGuid()),
                DomainOrigin.TimeSeries,
                recipient,
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
            var target = new MarketOperatorDataDomainService(
                bundleRepositoryMock.Object,
                dataAvailableNotificationRepositoryMock.Object,
                requestDomainServiceMock.Object,
                contentTypeWeightMapMock.Object);

            // Act
            var bundle = await target.GetNextUnacknowledgedAsync(recipient).ConfigureAwait(false);

            // Assert
            Assert.Null(bundle);
        }

        [Fact]
        public async Task TryAcknowledgeAsync_HasBundle_ReturnsTrue()
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
                DomainOrigin.TimeSeries,
                recipient,
                idsInBundle);

            var dataAvailableNotificationRepositoryMock = new Mock<IDataAvailableNotificationRepository>();

            var bundleRepositoryMock = new Mock<IBundleRepository>();
            bundleRepositoryMock
                .Setup(x => x.GetNextUnacknowledgedAsync(recipient))
                .ReturnsAsync(bundle);

            var contentTypeWeightMapMock = new Mock<IWeightCalculatorDomainService>();
            var requestDomainServiceMock = new Mock<IRequestBundleDomainService>();
            var target = new MarketOperatorDataDomainService(
                bundleRepositoryMock.Object,
                dataAvailableNotificationRepositoryMock.Object,
                requestDomainServiceMock.Object,
                contentTypeWeightMapMock.Object);

            // Act
            var result = await target.TryAcknowledgeAsync(recipient, bundleUuid).ConfigureAwait(false);

            // Assert
            Assert.True(result);
            bundleRepositoryMock.Verify(x => x.AcknowledgeAsync(bundleUuid), Times.Once);
            dataAvailableNotificationRepositoryMock.Verify(x => x.AcknowledgeAsync(idsInBundle), Times.Once);
        }

        [Fact]
        public async Task TryAcknowledgeAsync_HasNoBundle_ReturnsFalse()
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
            var target = new MarketOperatorDataDomainService(
                bundleRepositoryMock.Object,
                dataAvailableNotificationRepositoryMock.Object,
                requestDomainServiceMock.Object,
                contentTypeWeightMapMock.Object);

            // Act
            var result = await target.TryAcknowledgeAsync(recipient, bundleUuid).ConfigureAwait(false);

            // Assert
            Assert.False(result);
            bundleRepositoryMock.Verify(x => x.AcknowledgeAsync(It.IsAny<Uuid>()), Times.Never);
            dataAvailableNotificationRepositoryMock.Verify(x => x.AcknowledgeAsync(It.IsAny<IEnumerable<Uuid>>()), Times.Never);
        }

        [Fact]
        public async Task TryAcknowledgeAsync_WrongId_ReturnsFalse()
        {
            // Arrange
            var recipient = new MarketOperator(new GlobalLocationNumber("fake_value"));
            var bundleUuid = new Uuid("60D041F5-548B-49C0-8118-BB0F3DF1E692");
            var incorrectId = new Uuid("8BF7791E-A179-4B86-AE2F-69B5C276E99F");
            var dataAvailableNotificationRepositoryMock = new Mock<IDataAvailableNotificationRepository>();

            var bundle = new Bundle(
                bundleUuid,
                DomainOrigin.TimeSeries,
                recipient,
                Array.Empty<Uuid>());

            var bundleRepositoryMock = new Mock<IBundleRepository>();
            bundleRepositoryMock
                .Setup(x => x.GetNextUnacknowledgedAsync(recipient))
                .ReturnsAsync(bundle);

            var contentTypeWeightMapMock = new Mock<IWeightCalculatorDomainService>();
            var requestDomainServiceMock = new Mock<IRequestBundleDomainService>();
            var target = new MarketOperatorDataDomainService(
                bundleRepositoryMock.Object,
                dataAvailableNotificationRepositoryMock.Object,
                requestDomainServiceMock.Object,
                contentTypeWeightMapMock.Object);

            // Act
            var result = await target.TryAcknowledgeAsync(recipient, incorrectId).ConfigureAwait(false);

            // Assert
            Assert.False(result);
            bundleRepositoryMock.Verify(x => x.AcknowledgeAsync(It.IsAny<Uuid>()), Times.Never);
            dataAvailableNotificationRepositoryMock.Verify(x => x.AcknowledgeAsync(It.IsAny<IEnumerable<Uuid>>()), Times.Never);
        }

        private static DataAvailableNotification CreateDataAvailableNotification(
            MarketOperator recipient,
            ContentType contentType)
        {
            return new DataAvailableNotification(
                new Uuid(Guid.NewGuid()),
                recipient,
                contentType,
                DomainOrigin.TimeSeries,
                new SupportsBundling(false),
                new Weight(1));
        }
    }
}
