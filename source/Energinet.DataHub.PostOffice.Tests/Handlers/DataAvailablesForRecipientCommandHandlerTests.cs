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

using System.Collections.Generic;
using System.Linq;
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
    public sealed class DataAvailablesForRecipientCommandHandlerTests
    {
        [Fact]
        public async Task Handle_WithData_DataIsVerified()
        {
            // Arrange
            var repository = new Mock<IDataAvailableNotificationRepository>();
            var target = new DataAvailablesForRecipientCommandHandler(repository.Object);

            const string recipient = "7495563456235";
            const string origin = "TimeSeries";
            const string contentType = "TimeSeries";

            var expectedCabinetKey = new CabinetKey(
                new MarketOperator(new GlobalLocationNumber(recipient)),
                DomainOrigin.TimeSeries,
                new ContentType(contentType));

            var dataAvailableNotificationCommand = new DataAvailableNotificationCommand(
                "F8FC4D49-5245-4924-80D3-F1FB81FA3903",
                recipient,
                contentType,
                origin,
                true,
                1,
                1);

            var dataAvailableNotificationCommands = new List<DataAvailableNotificationCommand> { dataAvailableNotificationCommand };
            var request = new DataAvailableNotificationsForRecipientCommand(dataAvailableNotificationCommands);

            // Act
            var response = await target.Handle(request, CancellationToken.None).ConfigureAwait(false);

            // Assert
            repository.Verify(x => x.SaveAsync(
                It.Is<IEnumerable<DataAvailableNotification>>(z =>
                z.First().NotificationId == new Uuid(request.Notifications.First().Uuid) &&
                z.First().Recipient.Gln.Value == request.Notifications.First().Recipient &&
                z.First().Origin == DomainOrigin.TimeSeries &&
                z.First().Weight.Value == request.Notifications.First().Weight &&
                z.First().ContentType.Value == "TimeSeries"),
                It.Is<CabinetKey>(y => y == expectedCabinetKey)));
        }
    }
}
