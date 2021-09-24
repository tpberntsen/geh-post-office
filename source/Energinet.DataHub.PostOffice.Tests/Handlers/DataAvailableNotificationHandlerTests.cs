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
using Energinet.DataHub.PostOffice.Domain.Repositories;
using Moq;
using Xunit;
using Xunit.Categories;

namespace Energinet.DataHub.PostOffice.Tests.Handlers
{
    [UnitTest]
    public sealed class DataAvailableNotificationHandlerTests
    {
        [Fact]
        public async Task Handle_NullArgument_ThrowsException()
        {
            // Arrange
            var target = new DataAvailableNotificationHandler(new Mock<IDataAvailableNotificationRepository>().Object);

            // Act + Assert
            await Assert
                .ThrowsAsync<ArgumentNullException>(() => target.Handle(null!, CancellationToken.None))
                .ConfigureAwait(false);
        }

        [Fact]
        public async Task Handle_WithData_ReturnsTrue()
        {
            // Arrange
            var repository = new Mock<IDataAvailableNotificationRepository>();
            var target = new DataAvailableNotificationHandler(repository.Object);

            var request = new DataAvailableNotificationCommand(
                "F8FC4D49-5245-4924-80D3-F1FB81FA3903",
                "06E45497-B653-468E-99A7-911E3F3CD38A",
                "timeseries",
                "timeseries",
                false,
                1);

            // Act
            var response = await target.Handle(request, CancellationToken.None).ConfigureAwait(false);

            // Assert
            Assert.NotNull(response);
            repository.Verify(x => x.SaveAsync(It.Is<DataAvailableNotification>(x =>
                x.Recipient.Gln.Value == request.Recipient &&
                x.Origin == DomainOrigin.TimeSeries &&
                x.Weight.Value == request.Weight &&
                x.ContentType.Value == "timeseries" &&
                x.NotificationId == new Uuid(request.Uuid))));
        }
    }
}
