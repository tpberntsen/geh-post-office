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
using Energinet.DataHub.PostOffice.Domain.Services.Model;
using Moq;
using Xunit;
using Xunit.Categories;

namespace Energinet.DataHub.PostOffice.Tests.Services.Domain
{
    [UnitTest]
    public sealed class WarehouseDomainServiceTests
    {
        [Fact]
        public async Task PeekAsync_NoMessagesReady_ReturnsNull()
        {
            // Arrange
            var recipient = new MarketOperator("fake_value");

            var dataAvailableNotificationRepositoryMock = new Mock<IDataAvailableNotificationRepository>();
            dataAvailableNotificationRepositoryMock
                .Setup(x => x.GetNextUnacknowledgedAsync(recipient))
                .ReturnsAsync((DataAvailableNotification?)null);

            var bundleRepositoryMock = new Mock<IBundleRepository>();
            bundleRepositoryMock
                .Setup(x => x.GetNextUnacknowledgedAsync(recipient))
                .ReturnsAsync((IBundle?)null);

            var bundleDomainServiceMock = new Mock<IRequestBundleDomainService>();

            var target = new MarketOperatorDataDomainService(
                bundleRepositoryMock.Object,
                dataAvailableNotificationRepositoryMock.Object,
                bundleDomainServiceMock.Object);

            // Act
            var bundle = await target.GetNextUnacknowledgedAsync(recipient).ConfigureAwait(false);

            // Assert
            Assert.Null(bundle);
        }

        [Fact]
        public async Task PeekAsync_MessagesReady_ReturnsBundle()
        {
            // Arrange
            var recipient = new MarketOperator("fake_value");
            var messageType = new ContentType(5, "fake_value");

            var dataAvailableNotificationFirst = CreateDataAvailableNotification(recipient, messageType);
            var allDataAvailableNotificationsForMessageType = new[]
            {
                dataAvailableNotificationFirst,
                CreateDataAvailableNotification(recipient, messageType),
                CreateDataAvailableNotification(recipient, messageType),
                CreateDataAvailableNotification(recipient, messageType)
            };
            var requestSession = new RequestDataSession() { Id = new Uuid(System.Guid.NewGuid().ToString()) };
            var replyData = new SubDomainReply() { Success = true, UriToContent = new Uri("https://test.test.dk") };

            var dataAvailableNotificationRepositoryMock = new Mock<IDataAvailableNotificationRepository>();
            dataAvailableNotificationRepositoryMock
                .Setup(x => x.GetNextUnacknowledgedAsync(recipient))
                .ReturnsAsync(dataAvailableNotificationFirst);

            dataAvailableNotificationRepositoryMock
                .Setup(x => x.GetNextUnacknowledgedAsync(recipient, messageType))
                .ReturnsAsync(allDataAvailableNotificationsForMessageType);

            var bundleMock = new Mock<IBundle>();

            var bundleRepositoryMock = new Mock<IBundleRepository>();
            bundleRepositoryMock
                .Setup(x => x.GetNextUnacknowledgedAsync(recipient))
                .ReturnsAsync((IBundle?)null);

            bundleRepositoryMock
                .Setup(x => x.CreateBundleAsync(allDataAvailableNotificationsForMessageType, replyData.UriToContent))
                .ReturnsAsync(bundleMock.Object);

            var bundleDomainServiceMock = new Mock<IRequestBundleDomainService>();
            bundleDomainServiceMock
                .Setup(x => x.RequestBundledDataFromSubDomainAsync(
                    allDataAvailableNotificationsForMessageType,
                    dataAvailableNotificationFirst.Origin))
                .ReturnsAsync(requestSession);

            bundleDomainServiceMock
                .Setup(x => x.WaitForReplyFromSubDomainAsync(
                    requestSession,
                    dataAvailableNotificationFirst.Origin))
                .ReturnsAsync(replyData);

            var target = new MarketOperatorDataDomainService(
                bundleRepositoryMock.Object,
                dataAvailableNotificationRepositoryMock.Object,
                bundleDomainServiceMock.Object);

            // Act
            var bundle = await target.GetNextUnacknowledgedAsync(recipient).ConfigureAwait(false);

            // Assert
            Assert.Equal(bundleMock.Object, bundle);
        }

        [Fact]
        public async Task PeekAsync_HasBundleNotYetDequeued_ReturnsThatPreviousBundle()
        {
            // Arrange
            var recipient = new MarketOperator("fake_value");
            var bundleMock = new Mock<IBundle>();
            var dataAvailableNotificationRepositoryMock = new Mock<IDataAvailableNotificationRepository>();

            var bundleRepositoryMock = new Mock<IBundleRepository>();
            bundleRepositoryMock
                .Setup(x => x.GetNextUnacknowledgedAsync(recipient))
                .ReturnsAsync(bundleMock.Object);
            var bundleDomainServiceMock = new Mock<IRequestBundleDomainService>();
            var target = new MarketOperatorDataDomainService(
                bundleRepositoryMock.Object,
                dataAvailableNotificationRepositoryMock.Object,
                bundleDomainServiceMock.Object);

            // Act
            var bundle = await target.GetNextUnacknowledgedAsync(recipient).ConfigureAwait(false);

            // Assert
            Assert.Equal(bundleMock.Object, bundle);
        }

        [Fact]
        public async Task DequeueAsync_HasBundle_ReturnsTrue()
        {
            // Arrange
            var recipient = new MarketOperator("fake_value");
            var bundleUuid = new Uuid("1E0A906E-8895-4C86-B4FC-48E9BAF2A2B6");
            var idsInBundle = new[]
            {
                new Uuid("5AA0BDE7-EAB7-408D-B4A4-BBF1EEFF3F7E"),
                new Uuid("7E188D0E-A923-4AD5-A7CB-39889884241B"),
                new Uuid("9DEA909A-179B-413B-A669-E38D4C812009"),
                new Uuid("B0425457-8E0A-4E66-80EF-2717562EAEA7")
            };

            var bundleMock = new Mock<IBundle>();
            bundleMock.Setup(x => x.BundleId).Returns(bundleUuid);
            bundleMock.Setup(x => x.NotificationIds).Returns(idsInBundle);

            var dataAvailableNotificationRepositoryMock = new Mock<IDataAvailableNotificationRepository>();

            var bundleRepositoryMock = new Mock<IBundleRepository>();
            bundleRepositoryMock
                .Setup(x => x.GetNextUnacknowledgedAsync(recipient))
                .ReturnsAsync(bundleMock.Object);
            var bundleDomainServiceMock = new Mock<IRequestBundleDomainService>();
            var target = new MarketOperatorDataDomainService(
                bundleRepositoryMock.Object,
                dataAvailableNotificationRepositoryMock.Object,
                bundleDomainServiceMock.Object);

            // Act
            var result = await target.TryAcknowledgeAsync(recipient, bundleUuid).ConfigureAwait(false);

            // Assert
            Assert.True(result);
            bundleRepositoryMock.Verify(x => x.AcknowledgeAsync(bundleUuid), Times.Once);
            dataAvailableNotificationRepositoryMock.Verify(x => x.AcknowledgeAsync(idsInBundle), Times.Once);
        }

        [Fact]
        public async Task DequeueAsync_HasNoBundle_ReturnsFalse()
        {
            // Arrange
            var recipient = new MarketOperator("fake_value");
            var bundleUuid = new Uuid("60D041F5-548B-49C0-8118-BB0F3DF1E692");
            var dataAvailableNotificationRepositoryMock = new Mock<IDataAvailableNotificationRepository>();

            var bundleRepositoryMock = new Mock<IBundleRepository>();
            bundleRepositoryMock
                .Setup(x => x.GetNextUnacknowledgedAsync(recipient))
                .ReturnsAsync((IBundle?)null);

            var bundleDomainServiceMock = new Mock<IRequestBundleDomainService>();
            var target = new MarketOperatorDataDomainService(
                bundleRepositoryMock.Object,
                dataAvailableNotificationRepositoryMock.Object,
                bundleDomainServiceMock.Object);

            // Act
            var result = await target.TryAcknowledgeAsync(recipient, bundleUuid).ConfigureAwait(false);

            // Assert
            Assert.False(result);
            bundleRepositoryMock.Verify(x => x.AcknowledgeAsync(It.IsAny<Uuid>()), Times.Never);
            dataAvailableNotificationRepositoryMock.Verify(x => x.AcknowledgeAsync(It.IsAny<IEnumerable<Uuid>>()), Times.Never);
        }

        [Fact]
        public async Task DequeueAsync_WrongId_ReturnsFalse()
        {
            // Arrange
            var recipient = new MarketOperator("fake_value");
            var bundleUuid = new Uuid("60D041F5-548B-49C0-8118-BB0F3DF1E692");
            var incorrectId = new Uuid("8BF7791E-A179-4B86-AE2F-69B5C276E99F");
            var dataAvailableNotificationRepositoryMock = new Mock<IDataAvailableNotificationRepository>();

            var bundleMock = new Mock<IBundle>();
            bundleMock.Setup(x => x.BundleId).Returns(bundleUuid);

            var bundleRepositoryMock = new Mock<IBundleRepository>();
            bundleRepositoryMock
                .Setup(x => x.GetNextUnacknowledgedAsync(recipient))
                .ReturnsAsync(bundleMock.Object);

            var bundleDomainServiceMock = new Mock<IRequestBundleDomainService>();
            var target = new MarketOperatorDataDomainService(
                bundleRepositoryMock.Object,
                dataAvailableNotificationRepositoryMock.Object,
                bundleDomainServiceMock.Object);

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
                new Uuid("fake_value"),
                recipient,
                contentType,
                SubDomain.TimeSeries,
                new Weight(1));
        }
    }
}
