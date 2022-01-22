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
using System.Threading.Tasks;
using Energinet.DataHub.PostOffice.Application.Commands;
using Energinet.DataHub.PostOffice.Domain.Model;
using Energinet.DataHub.PostOffice.Domain.Repositories;
using Energinet.DataHub.PostOffice.IntegrationTests.Common;
using FluentValidation;
using MediatR;
using Xunit;
using Xunit.Categories;

namespace Energinet.DataHub.PostOffice.IntegrationTests.Hosts.SubDomain
{
    [Collection("IntegrationTest")]
    [IntegrationTest]
    public sealed class DataAvailableIntegrationTests
    {
        [Fact]
        public async Task InsertDataAvailableNotificationsCommand_InvalidCommand_ThrowsException()
        {
            // Arrange
            const string blankValue = "  ";

            await using var host = await MarketOperatorIntegrationTestHost
                .InitializeAsync()
                .ConfigureAwait(false);

            await using var scope = host.BeginScope();
            var mediator = scope.GetInstance<IMediator>();

            var dataAvailableNotification = new DataAvailableNotificationDto(
                blankValue,
                blankValue,
                blankValue,
                blankValue,
                false,
                1,
                1);

            var command = new InsertDataAvailableNotificationsCommand(new[] { dataAvailableNotification });

            // Act + Assert
            await Assert
                .ThrowsAsync<ValidationException>(() => mediator.Send(command))
                .ConfigureAwait(false);
        }

        [Fact]
        public async Task InsertDataAvailableNotificationsCommand_WithData_CanBePeekedBack()
        {
            // Arrange
            var recipientGln = new MockedGln();
            var bundleId = Guid.NewGuid().ToString();

            await using var host = await MarketOperatorIntegrationTestHost
                .InitializeAsync()
                .ConfigureAwait(false);

            await using var scope = host.BeginScope();
            var mediator = scope.GetInstance<IMediator>();

            var dataAvailableNotification = new DataAvailableNotificationDto(
                Guid.NewGuid().ToString(),
                recipientGln,
                "timeseries",
                "timeseries",
                false,
                1,
                1);

            var command = new InsertDataAvailableNotificationsCommand(new[] { dataAvailableNotification });

            // Act
            await mediator.Send(command).ConfigureAwait(false);

            // Assert
            var peekResponse = await mediator.Send(new PeekCommand(recipientGln, bundleId)).ConfigureAwait(false);
            Assert.NotNull(peekResponse);
            Assert.True(peekResponse.HasContent);
        }

        [Fact]
        public async Task UpdateMaximumSequenceNumberCommand_InvalidCommand_ThrowsException()
        {
            // Arrange
            await using var host = await MarketOperatorIntegrationTestHost
                .InitializeAsync()
                .ConfigureAwait(false);

            await using var scope = host.BeginScope();
            var mediator = scope.GetInstance<IMediator>();

            var command = new UpdateMaximumSequenceNumberCommand(null!);

            // Act + Assert
            await Assert
                .ThrowsAsync<ValidationException>(() => mediator.Send(command))
                .ConfigureAwait(false);
        }

        [Fact]
        public async Task UpdateMaximumSequenceNumberCommand_WithNewNumber_CanReadNumberBack()
        {
            // Arrange
            const long sequenceNumber = int.MaxValue + 1L;

            await using var host = await MarketOperatorIntegrationTestHost
                .InitializeAsync()
                .ConfigureAwait(false);

            await using var scope = host.BeginScope();
            var mediator = scope.GetInstance<IMediator>();

            var command = new UpdateMaximumSequenceNumberCommand(new SequenceNumber(sequenceNumber));

            // Act
            await mediator.Send(command).ConfigureAwait(false);

            // Assert
            var sequenceNumberRepository = scope.GetInstance<ISequenceNumberRepository>();
            var actual = await sequenceNumberRepository.GetMaximumSequenceNumberAsync().ConfigureAwait(false);
            Assert.Equal(sequenceNumber, actual.Value);
        }
    }
}
