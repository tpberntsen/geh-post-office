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
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Energinet.DataHub.PostOffice.Application.Commands;
using Energinet.DataHub.PostOffice.Application.Handlers;
using Energinet.DataHub.PostOffice.Domain.Model;
using Energinet.DataHub.PostOffice.Domain.Repositories;
using Moq;
using Xunit;
using Xunit.Categories;

namespace Energinet.DataHub.PostOffice.Tests.Handlers
{
    [UnitTest]
    public sealed class InsertDataAvailableNotificationsCommandHandlerTests
    {
        [Fact]
        public async Task Handle_NullArgument_ThrowsException()
        {
            // Arrange
            var repository = new Mock<IDataAvailableNotificationRepository>();
            var target = new InsertDataAvailableNotificationsCommandHandler(repository.Object);

            // Act + Assert
            await Assert
                .ThrowsAsync<ArgumentNullException>(() => target.Handle(null!, CancellationToken.None))
                .ConfigureAwait(false);
        }

        [Fact]
        public async Task Handle_WithData_DataIsVerified()
        {
            // Arrangem
            var repository = new Mock<IDataAvailableNotificationRepository>();
            var target = new InsertDataAvailableNotificationsCommandHandler(repository.Object);

            const string recipient = "7495563456235";
            const string origin = "TimeSeries";

            var dataAvailableNotifications = new[]
            {
                new DataAvailableNotificationDto(
                    "E8E875C0-250D-4B82-9357-4CE26E0E7A1E",
                    recipient,
                    "fake_value_1",
                    origin,
                    true,
                    1,
                    1),
                new DataAvailableNotificationDto(
                    "70469BE2-EFA3-4CBA-ABAF-AAE573BF057E",
                    recipient,
                    "fake_value_2",
                    origin,
                    true,
                    2,
                    2),
                new DataAvailableNotificationDto(
                    "F8FC4D49-5245-4924-80D3-F1FB81FA3903",
                    recipient,
                    "fake_value_3",
                    origin,
                    false,
                    3,
                    3),
            };

            var request = new InsertDataAvailableNotificationsCommand(dataAvailableNotifications);

            // Act
            await target.Handle(request, CancellationToken.None).ConfigureAwait(false);

            // Assert
            repository.Verify(
                x => x.SaveAsync(
                    It.Is(ExpectedCabinetKey(dataAvailableNotifications[0])),
                    It.Is(ExpectedNotification(dataAvailableNotifications[0]))),
                Times.Once);

            repository.Verify(
                x => x.SaveAsync(
                    It.Is(ExpectedCabinetKey(dataAvailableNotifications[1])),
                    It.Is(ExpectedNotification(dataAvailableNotifications[1]))),
                Times.Once);

            repository.Verify(
                x => x.SaveAsync(
                    It.Is(ExpectedCabinetKey(dataAvailableNotifications[2])),
                    It.Is(ExpectedNotification(dataAvailableNotifications[2]))),
                Times.Once);
        }

        private static Expression<Func<IEnumerable<DataAvailableNotification>, bool>>
            ExpectedNotification(DataAvailableNotificationDto dataAvailableNotification)
        {
            return notifications => ExpectedNotification(notifications.Single(), dataAvailableNotification);
        }

        private static bool ExpectedNotification(DataAvailableNotification notification, DataAvailableNotificationDto dto)
        {
            return
                notification.NotificationId == new Uuid(dto.Uuid) &&
                notification.Recipient.Gln.Value == dto.Recipient &&
                notification.Origin.ToString() == dto.Origin &&
                notification.ContentType.Value == dto.ContentType &&
                notification.Weight.Value == dto.Weight &&
                notification.SupportsBundling.Value == dto.SupportsBundling &&
                notification.SequenceNumber.Value == dto.SequenceNumber;
        }

        private static Expression<Func<CabinetKey, bool>> ExpectedCabinetKey(DataAvailableNotificationDto dto)
        {
            return cabinetKey => cabinetKey == new CabinetKey(
                new MarketOperator(new GlobalLocationNumber(dto.Recipient)),
                Enum.Parse<DomainOrigin>(dto.Origin),
                new ContentType(dto.ContentType));
        }
    }
}
