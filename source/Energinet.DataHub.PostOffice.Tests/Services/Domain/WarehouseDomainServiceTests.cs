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
    public sealed class WarehouseDomainServiceTests
    {
        [Fact]
        public async Task PeekAsync_NoMessagesReady_ReturnsNull()
        {
            // Arrange
            var recipient = new Recipient("fake_value");

            var dataAvailableNotificationRepositoryMock = new Mock<IDataAvailableNotificationRepository>();
            dataAvailableNotificationRepositoryMock
                .Setup(x => x.PeekAsync(recipient))
                .ReturnsAsync((DataAvailableNotification?)null);

            var bundleRepositoryMock = new Mock<IBundleRepository>();
            bundleRepositoryMock
                .Setup(x => x.PeekAsync(recipient))
                .ReturnsAsync((IBundle?)null);

            var target = new WarehouseDomainService(
                bundleRepositoryMock.Object,
                dataAvailableNotificationRepositoryMock.Object);

            // Act
            var bundle = await target.PeekAsync(recipient).ConfigureAwait(false);

            // Assert
            Assert.Null(bundle);
        }

        [Fact]
        public async Task PeekAsync_MessagesReady_ReturnsBundle()
        {
            // Arrange
            var recipient = new Recipient("fake_value");
            var messageType = new MessageType(5, "fake_value");

            var dataAvailableNotificationFirst = CreateDataAvailableNotification(recipient, messageType);
            var allDataAvailableNotificationsForMessageType = new[]
            {
                dataAvailableNotificationFirst,
                CreateDataAvailableNotification(recipient, messageType),
                CreateDataAvailableNotification(recipient, messageType),
                CreateDataAvailableNotification(recipient, messageType)
            };

            var dataAvailableNotificationRepositoryMock = new Mock<IDataAvailableNotificationRepository>();
            dataAvailableNotificationRepositoryMock
                .Setup(x => x.PeekAsync(recipient))
                .ReturnsAsync(dataAvailableNotificationFirst);

            dataAvailableNotificationRepositoryMock
                .Setup(x => x.PeekAsync(recipient, messageType))
                .ReturnsAsync(allDataAvailableNotificationsForMessageType);

            var bundleMock = new Mock<IBundle>();

            var bundleRepositoryMock = new Mock<IBundleRepository>();
            bundleRepositoryMock
                .Setup(x => x.PeekAsync(recipient))
                .ReturnsAsync((IBundle?)null);

            bundleRepositoryMock
                .Setup(x => x.CreateBundleAsync(allDataAvailableNotificationsForMessageType))
                .ReturnsAsync(bundleMock.Object);

            var target = new WarehouseDomainService(
                bundleRepositoryMock.Object,
                dataAvailableNotificationRepositoryMock.Object);

            // Act
            var bundle = await target.PeekAsync(recipient).ConfigureAwait(false);

            // Assert
            Assert.Equal(bundleMock.Object, bundle);
        }

        [Fact]
        public async Task PeekAsync_HasBundleNotYetDequeued_ReturnsThatPreviousBundle()
        {
            // Arrange
            var recipient = new Recipient("fake_value");
            var bundleMock = new Mock<IBundle>();
            var dataAvailableNotificationRepositoryMock = new Mock<IDataAvailableNotificationRepository>();

            var bundleRepositoryMock = new Mock<IBundleRepository>();
            bundleRepositoryMock
                .Setup(x => x.PeekAsync(recipient))
                .ReturnsAsync(bundleMock.Object);

            var target = new WarehouseDomainService(
                bundleRepositoryMock.Object,
                dataAvailableNotificationRepositoryMock.Object);

            // Act
            var bundle = await target.PeekAsync(recipient).ConfigureAwait(false);

            // Assert
            Assert.Equal(bundleMock.Object, bundle);
        }

        [Fact]
        public async Task DequeueAsync_HasBundle_ReturnsTrue()
        {
            // Arrange
            var recipient = new Recipient("fake_value");
            var bundleUuid = new Uuid("1E0A906E-8895-4C86-B4FC-48E9BAF2A2B6");
            var idsInBundle = new[]
            {
                new Uuid("5AA0BDE7-EAB7-408D-B4A4-BBF1EEFF3F7E"),
                new Uuid("7E188D0E-A923-4AD5-A7CB-39889884241B"),
                new Uuid("9DEA909A-179B-413B-A669-E38D4C812009"),
                new Uuid("B0425457-8E0A-4E66-80EF-2717562EAEA7")
            };

            var bundleMock = new Mock<IBundle>();
            bundleMock.Setup(x => x.Id).Returns(bundleUuid);
            bundleMock.Setup(x => x.NotificationsIds).Returns(idsInBundle);

            var dataAvailableNotificationRepositoryMock = new Mock<IDataAvailableNotificationRepository>();

            var bundleRepositoryMock = new Mock<IBundleRepository>();
            bundleRepositoryMock
                .Setup(x => x.PeekAsync(recipient))
                .ReturnsAsync(bundleMock.Object);

            var target = new WarehouseDomainService(
                bundleRepositoryMock.Object,
                dataAvailableNotificationRepositoryMock.Object);

            // Act
            var result = await target.TryDequeueAsync(recipient, bundleUuid).ConfigureAwait(false);

            // Assert
            Assert.True(result);
            bundleRepositoryMock.Verify(x => x.DequeueAsync(bundleUuid), Times.Once);
            dataAvailableNotificationRepositoryMock.Verify(x => x.DequeueAsync(idsInBundle), Times.Once);
        }

        [Fact]
        public async Task DequeueAsync_HasNoBundle_ReturnsFalse()
        {
            // Arrange
            var recipient = new Recipient("fake_value");
            var bundleUuid = new Uuid("60D041F5-548B-49C0-8118-BB0F3DF1E692");
            var dataAvailableNotificationRepositoryMock = new Mock<IDataAvailableNotificationRepository>();

            var bundleRepositoryMock = new Mock<IBundleRepository>();
            bundleRepositoryMock
                .Setup(x => x.PeekAsync(recipient))
                .ReturnsAsync((IBundle?)null);

            var target = new WarehouseDomainService(
                bundleRepositoryMock.Object,
                dataAvailableNotificationRepositoryMock.Object);

            // Act
            var result = await target.TryDequeueAsync(recipient, bundleUuid).ConfigureAwait(false);

            // Assert
            Assert.False(result);
            bundleRepositoryMock.Verify(x => x.DequeueAsync(It.IsAny<Uuid>()), Times.Never);
            dataAvailableNotificationRepositoryMock.Verify(x => x.DequeueAsync(It.IsAny<IEnumerable<Uuid>>()), Times.Never);
        }

        [Fact]
        public async Task DequeueAsync_WrongId_ReturnsFalse()
        {
            // Arrange
            var recipient = new Recipient("fake_value");
            var bundleUuid = new Uuid("60D041F5-548B-49C0-8118-BB0F3DF1E692");
            var incorrectId = new Uuid("8BF7791E-A179-4B86-AE2F-69B5C276E99F");
            var dataAvailableNotificationRepositoryMock = new Mock<IDataAvailableNotificationRepository>();

            var bundleMock = new Mock<IBundle>();
            bundleMock.Setup(x => x.Id).Returns(bundleUuid);

            var bundleRepositoryMock = new Mock<IBundleRepository>();
            bundleRepositoryMock
                .Setup(x => x.PeekAsync(recipient))
                .ReturnsAsync(bundleMock.Object);

            var target = new WarehouseDomainService(
                bundleRepositoryMock.Object,
                dataAvailableNotificationRepositoryMock.Object);

            // Act
            var result = await target.TryDequeueAsync(recipient, incorrectId).ConfigureAwait(false);

            // Assert
            Assert.False(result);
            bundleRepositoryMock.Verify(x => x.DequeueAsync(It.IsAny<Uuid>()), Times.Never);
            dataAvailableNotificationRepositoryMock.Verify(x => x.DequeueAsync(It.IsAny<IEnumerable<Uuid>>()), Times.Never);
        }

        private static DataAvailableNotification CreateDataAvailableNotification(
            Recipient recipient,
            MessageType messageType)
        {
            return new DataAvailableNotification(
                new Uuid("fake_value"),
                recipient,
                messageType,
                Origin.TimeSeries,
                new Weight(1));
        }
    }
}