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
                1,
                "RSM??");

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
                1,
                "RSM??");

            var command = new InsertDataAvailableNotificationsCommand(new[] { dataAvailableNotification });

            // Act
            await mediator.Send(command).ConfigureAwait(false);

            // Assert
            var peekResponse = await mediator.Send(new PeekCommand(recipientGln, bundleId, BundleReturnType.Xml)).ConfigureAwait(false);
            Assert.NotNull(peekResponse);
            Assert.True(peekResponse.HasContent);
        }

        [Fact]
        public async Task InsertDataAvailableNotificationsCommand_PeekDequeuePeekSequence_CanBePeekedBack()
        {
            // Arrange
            var recipientGln = new MockedGln();
            var bundleIdA = Guid.NewGuid().ToString();
            var bundleIdB = Guid.NewGuid().ToString();

            await using var host = await MarketOperatorIntegrationTestHost
                .InitializeAsync()
                .ConfigureAwait(false);

            await using var scope = host.BeginScope();
            var mediator = scope.GetInstance<IMediator>();

            var dataAvailableNotificationA = new DataAvailableNotificationDto(
                Guid.NewGuid().ToString(),
                recipientGln,
                "MeteringPoints",
                "MeteringPoints",
                true,
                1,
                1,
                "RSM??");

            var dataAvailableNotificationB = new DataAvailableNotificationDto(
                Guid.NewGuid().ToString(),
                dataAvailableNotificationA.Recipient,
                dataAvailableNotificationA.ContentType,
                dataAvailableNotificationA.Origin,
                dataAvailableNotificationA.SupportsBundling,
                dataAvailableNotificationA.Weight,
                2,
                dataAvailableNotificationA.DocumentType);

            var dataAvailableNotificationC = new DataAvailableNotificationDto(
                Guid.NewGuid().ToString(),
                dataAvailableNotificationA.Recipient,
                dataAvailableNotificationA.ContentType,
                dataAvailableNotificationA.Origin,
                dataAvailableNotificationA.SupportsBundling,
                dataAvailableNotificationA.Weight,
                3,
                dataAvailableNotificationA.DocumentType);

            var insert3 = new InsertDataAvailableNotificationsCommand(new[]
            {
                dataAvailableNotificationA,
                dataAvailableNotificationB,
                dataAvailableNotificationC
            });

            await mediator.Send(insert3).ConfigureAwait(false);
            await mediator.Send(new UpdateMaximumSequenceNumberCommand(3)).ConfigureAwait(false);

            // Act
            await mediator.Send(new PeekCommand(recipientGln, bundleIdA, BundleReturnType.Xml)).ConfigureAwait(false);
            await mediator.Send(new DequeueCommand(recipientGln, bundleIdA)).ConfigureAwait(false);

            var dataAvailableNotificationD = new DataAvailableNotificationDto(
                Guid.NewGuid().ToString(),
                dataAvailableNotificationA.Recipient,
                dataAvailableNotificationA.ContentType,
                dataAvailableNotificationA.Origin,
                dataAvailableNotificationA.SupportsBundling,
                dataAvailableNotificationA.Weight,
                4,
                dataAvailableNotificationA.DocumentType);

            var insert1 = new InsertDataAvailableNotificationsCommand(new[]
            {
                dataAvailableNotificationD
            });

            await mediator.Send(insert1).ConfigureAwait(false);
            await mediator.Send(new UpdateMaximumSequenceNumberCommand(4)).ConfigureAwait(false);

            var peekResponse = await mediator
                .Send(new PeekCommand(recipientGln, bundleIdB, BundleReturnType.Xml))
                .ConfigureAwait(false);

            // Assert
            Assert.True(peekResponse.HasContent);
        }

        [Fact]
        public async Task InsertDataAvailableNotificationsCommand_PeekInsertDequeueSequence_CanBePeekedBack()
        {
            // Arrange
            var recipientGln = new MockedGln();
            var bundleIdA = Guid.NewGuid().ToString();
            var bundleIdB = Guid.NewGuid().ToString();

            await using var host = await MarketOperatorIntegrationTestHost
                .InitializeAsync()
                .ConfigureAwait(false);

            await using var scope = host.BeginScope();
            var mediator = scope.GetInstance<IMediator>();

            var dataAvailableNotificationA = new DataAvailableNotificationDto(
                Guid.NewGuid().ToString(),
                recipientGln,
                "MeteringPoints",
                "MeteringPoints",
                true,
                1,
                1,
                "RSM??");

            var dataAvailableNotificationB = new DataAvailableNotificationDto(
                Guid.NewGuid().ToString(),
                dataAvailableNotificationA.Recipient,
                dataAvailableNotificationA.ContentType,
                dataAvailableNotificationA.Origin,
                dataAvailableNotificationA.SupportsBundling,
                dataAvailableNotificationA.Weight,
                2,
                dataAvailableNotificationA.DocumentType);

            await mediator
                .Send(new InsertDataAvailableNotificationsCommand(new[] { dataAvailableNotificationA }))
                .ConfigureAwait(false);

            // Act
            await mediator.Send(new PeekCommand(recipientGln, bundleIdA, BundleReturnType.Xml)).ConfigureAwait(false);

            await mediator
                .Send(new InsertDataAvailableNotificationsCommand(new[] { dataAvailableNotificationB }))
                .ConfigureAwait(false);

            await mediator.Send(new DequeueCommand(recipientGln, bundleIdA)).ConfigureAwait(false);

            // Assert
            var peekResponse = await mediator
                .Send(new PeekCommand(recipientGln, bundleIdB, BundleReturnType.Xml))
                .ConfigureAwait(false);

            Assert.True(peekResponse.HasContent);
        }

        [Fact]
        public async Task GetMaximumSequenceNumberCommand_ValidCommand_ReturnsNumber()
        {
            // Arrange
            await using var host = await MarketOperatorIntegrationTestHost
                .InitializeAsync()
                .ConfigureAwait(false);

            await using var scope = host.BeginScope();
            var mediator = scope.GetInstance<IMediator>();

            var command = new GetMaximumSequenceNumberCommand();

            // Act
            var actual = await mediator.Send(command).ConfigureAwait(false);

            // Assert
            Assert.True(actual >= 0);
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

            var command = new UpdateMaximumSequenceNumberCommand(-10);

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

            var command = new UpdateMaximumSequenceNumberCommand(sequenceNumber);

            // Act
            await mediator.Send(command).ConfigureAwait(false);

            // Assert
            var sequenceNumberRepository = scope.GetInstance<ISequenceNumberRepository>();
            var actual = await sequenceNumberRepository.GetMaximumSequenceNumberAsync().ConfigureAwait(false);
            Assert.Equal(sequenceNumber, actual.Value);
        }
    }
}
