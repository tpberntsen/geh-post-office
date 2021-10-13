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
using Energinet.DataHub.PostOffice.IntegrationTests.Common;
using FluentValidation;
using MediatR;
using Xunit;
using Xunit.Categories;

namespace Energinet.DataHub.PostOffice.IntegrationTests.Hosts.MarketOperator
{
    [Collection("IntegrationTest")]
    [IntegrationTest]
    public sealed class PeekIntegrationTests
    {
        [Fact]
        public async Task PeekCommand_InvalidCommand_ThrowsException()
        {
            // Arrange
            await using var host = await MarketOperatorIntegrationTestHost
                .InitializeAsync()
                .ConfigureAwait(false);

            await using var scope = host.BeginScope();
            var mediator = scope.GetInstance<IMediator>();

            var peekCommand = new PeekCommand("   ", "   ");

            // Act + Assert
            await Assert.ThrowsAsync<ValidationException>(() => mediator.Send(peekCommand)).ConfigureAwait(false);
        }

        [Fact]
        public async Task PeekCommand_Empty_ReturnsNothing()
        {
            // Arrange
            var recipientGln = new MockedGln();
            var unrelatedGln = new MockedGln();
            var bundleId = Guid.NewGuid().ToString();

            await AddTimeSeriesNotificationAsync(unrelatedGln).ConfigureAwait(false);

            await using var host = await MarketOperatorIntegrationTestHost
                .InitializeAsync()
                .ConfigureAwait(false);

            await using var scope = host.BeginScope();
            var mediator = scope.GetInstance<IMediator>();

            var peekCommand = new PeekCommand(recipientGln, bundleId);

            // Act
            var response = await mediator.Send(peekCommand).ConfigureAwait(false);

            // Assert
            Assert.NotNull(response);
            Assert.False(response.HasContent);
        }

        [Fact]
        public async Task PeekCommand_SingleNotification_ReturnsData()
        {
            // Arrange
            var recipientGln = new MockedGln();
            var bundleId = Guid.NewGuid().ToString();
            await AddTimeSeriesNotificationAsync(recipientGln).ConfigureAwait(false);

            await using var host = await MarketOperatorIntegrationTestHost
                .InitializeAsync()
                .ConfigureAwait(false);

            await using var scope = host.BeginScope();
            var mediator = scope.GetInstance<IMediator>();

            var peekCommand = new PeekCommand(recipientGln, bundleId);

            // Act
            var response = await mediator.Send(peekCommand).ConfigureAwait(false);

            // Assert
            Assert.NotNull(response);
            Assert.True(response.HasContent);
        }

        [Fact]
        public async Task PeekCommand_SingleNotificationMultiplePeek_ReturnsData()
        {
            // Arrange
            var recipientGln = new MockedGln();
            var bundleId = Guid.NewGuid().ToString();
            await AddTimeSeriesNotificationAsync(recipientGln).ConfigureAwait(false);

            await using var host = await MarketOperatorIntegrationTestHost
                .InitializeAsync()
                .ConfigureAwait(false);

            await using var scope = host.BeginScope();
            var mediator = scope.GetInstance<IMediator>();

            var peekCommand = new PeekCommand(recipientGln, bundleId);

            // Act
            var responseA = await mediator.Send(peekCommand).ConfigureAwait(false);
            var responseB = await mediator.Send(peekCommand).ConfigureAwait(false);

            // Assert
            Assert.NotNull(responseA);
            Assert.True(responseA.HasContent);
            Assert.NotNull(responseB);
            Assert.True(responseB.HasContent);
        }

        [Fact]
        public async Task PeekTimeSeriesCommand_InvalidCommand_ThrowsException()
        {
            // Arrange
            await using var host = await MarketOperatorIntegrationTestHost
                .InitializeAsync()
                .ConfigureAwait(false);

            await using var scope = host.BeginScope();
            var mediator = scope.GetInstance<IMediator>();

            var peekCommand = new PeekTimeSeriesCommand("   ", "   ");

            // Act + Assert
            await Assert.ThrowsAsync<ValidationException>(() => mediator.Send(peekCommand)).ConfigureAwait(false);
        }

        [Fact]
        public async Task PeekTimeSeriesCommand_Empty_ReturnsNothing()
        {
            // Arrange
            var recipientGln = new MockedGln();
            var unrelatedGln = new MockedGln();
            var bundleId = Guid.NewGuid().ToString();

            await AddTimeSeriesNotificationAsync(unrelatedGln).ConfigureAwait(false);

            await using var host = await MarketOperatorIntegrationTestHost
                .InitializeAsync()
                .ConfigureAwait(false);

            await using var scope = host.BeginScope();
            var mediator = scope.GetInstance<IMediator>();

            var peekCommand = new PeekTimeSeriesCommand(recipientGln, bundleId);

            // Act
            var response = await mediator.Send(peekCommand).ConfigureAwait(false);

            // Assert
            Assert.NotNull(response);
            Assert.False(response.HasContent);
        }

        [Fact]
        public async Task PeekTimeSeriesCommand_SingleNotification_ReturnsData()
        {
            // Arrange
            var recipientGln = new MockedGln();
            var bundleId = Guid.NewGuid().ToString();

            await AddTimeSeriesNotificationAsync(recipientGln).ConfigureAwait(false);

            await using var host = await MarketOperatorIntegrationTestHost
                .InitializeAsync()
                .ConfigureAwait(false);

            await using var scope = host.BeginScope();
            var mediator = scope.GetInstance<IMediator>();

            var peekCommand = new PeekTimeSeriesCommand(recipientGln, bundleId);

            // Act
            var response = await mediator.Send(peekCommand).ConfigureAwait(false);

            // Assert
            Assert.NotNull(response);
            Assert.True(response.HasContent);
        }

        [Fact]
        public async Task PeekTimeSeriesCommand_SingleNotificationMultiplePeek_ReturnsData()
        {
            // Arrange
            var recipientGln = new MockedGln();
            var bundleId = Guid.NewGuid().ToString();

            await AddTimeSeriesNotificationAsync(recipientGln).ConfigureAwait(false);

            await using var host = await MarketOperatorIntegrationTestHost
                .InitializeAsync()
                .ConfigureAwait(false);

            await using var scope = host.BeginScope();
            var mediator = scope.GetInstance<IMediator>();

            var peekCommand = new PeekTimeSeriesCommand(recipientGln, bundleId);

            // Act
            var responseA = await mediator.Send(peekCommand).ConfigureAwait(false);
            var responseB = await mediator.Send(peekCommand).ConfigureAwait(false);

            // Assert
            Assert.NotNull(responseA);
            Assert.True(responseA.HasContent);
            Assert.NotNull(responseB);
            Assert.True(responseB.HasContent);
        }

        [Fact]
        public async Task PeekAggregationsCommand_InvalidCommand_ThrowsException()
        {
            // Arrange
            await using var host = await MarketOperatorIntegrationTestHost
                .InitializeAsync()
                .ConfigureAwait(false);

            await using var scope = host.BeginScope();
            var mediator = scope.GetInstance<IMediator>();

            var peekCommand = new PeekAggregationsCommand("   ", "    ");

            // Act + Assert
            await Assert.ThrowsAsync<ValidationException>(() => mediator.Send(peekCommand)).ConfigureAwait(false);
        }

        [Fact]
        public async Task PeekAggregationsCommand_Empty_ReturnsNothing()
        {
            // Arrange
            var recipientGln = new MockedGln();
            var unrelatedGln = new MockedGln();
            var bundleId = Guid.NewGuid().ToString();

            await AddAggregationsNotificationAsync(unrelatedGln).ConfigureAwait(false);

            await using var host = await MarketOperatorIntegrationTestHost
                .InitializeAsync()
                .ConfigureAwait(false);

            await using var scope = host.BeginScope();
            var mediator = scope.GetInstance<IMediator>();

            var peekCommand = new PeekAggregationsCommand(recipientGln, bundleId);

            // Act
            var response = await mediator.Send(peekCommand).ConfigureAwait(false);

            // Assert
            Assert.NotNull(response);
            Assert.False(response.HasContent);
        }

        [Fact]
        public async Task PeekAggregationsCommand_SingleAggregationNotification_ReturnsData()
        {
            // Arrange
            var recipientGln = new MockedGln();
            var bundleId = Guid.NewGuid().ToString();
            await AddAggregationsNotificationAsync(recipientGln).ConfigureAwait(false);

            await using var host = await MarketOperatorIntegrationTestHost
                .InitializeAsync()
                .ConfigureAwait(false);

            await using var scope = host.BeginScope();
            var mediator = scope.GetInstance<IMediator>();

            var peekCommand = new PeekAggregationsCommand(recipientGln, bundleId);

            // Act
            var response = await mediator.Send(peekCommand).ConfigureAwait(false);

            // Assert
            Assert.NotNull(response);
            Assert.True(response.HasContent);
        }

        [Fact]
        public async Task PeekAggregationsCommand_SingleNotificationMultiplePeek_ReturnsData()
        {
            // Arrange
            var recipientGln = new MockedGln();
            var bundleId = Guid.NewGuid().ToString();
            await AddAggregationsNotificationAsync(recipientGln).ConfigureAwait(false);

            await using var host = await MarketOperatorIntegrationTestHost
                .InitializeAsync()
                .ConfigureAwait(false);

            await using var scope = host.BeginScope();
            var mediator = scope.GetInstance<IMediator>();

            var peekCommand = new PeekAggregationsCommand(recipientGln, bundleId);

            // Act
            var responseA = await mediator.Send(peekCommand).ConfigureAwait(false);
            var responseB = await mediator.Send(peekCommand).ConfigureAwait(false);

            // Assert
            Assert.NotNull(responseA);
            Assert.True(responseA.HasContent);
            Assert.NotNull(responseB);
            Assert.True(responseB.HasContent);
        }

        [Fact]
        public async Task PeekMasterDataCommand_InvalidCommand_ThrowsException()
        {
            // Arrange
            await using var host = await MarketOperatorIntegrationTestHost
                .InitializeAsync()
                .ConfigureAwait(false);

            await using var scope = host.BeginScope();
            var mediator = scope.GetInstance<IMediator>();

            var peekCommand = new PeekMasterDataCommand("   ", "    ");

            // Act + Assert
            await Assert.ThrowsAsync<ValidationException>(() => mediator.Send(peekCommand)).ConfigureAwait(false);
        }

        [Fact]
        public async Task PeekMasterDataCommand_Empty_ReturnsNothing()
        {
            // Arrange
            var recipientGln = new MockedGln();
            var unrelatedGln = new MockedGln();
            var bundleId = Guid.NewGuid().ToString();

            await AddMeteringPointsNotificationAsync(unrelatedGln).ConfigureAwait(false);

            await using var host = await MarketOperatorIntegrationTestHost
                .InitializeAsync()
                .ConfigureAwait(false);

            await using var scope = host.BeginScope();
            var mediator = scope.GetInstance<IMediator>();

            var peekCommand = new PeekMasterDataCommand(recipientGln, bundleId);

            // Act
            var response = await mediator.Send(peekCommand).ConfigureAwait(false);

            // Assert
            Assert.NotNull(response);
            Assert.False(response.HasContent);
        }

        [Fact]
        public async Task PeekMasterDataCommand_SingleMarketRolesNotification_ReturnsData()
        {
            // Arrange
            var recipientGln = new MockedGln();
            var bundleId = Guid.NewGuid().ToString();
            await AddMarketRolesNotificationAsync(recipientGln).ConfigureAwait(false);

            await using var host = await MarketOperatorIntegrationTestHost
                .InitializeAsync()
                .ConfigureAwait(false);

            await using var scope = host.BeginScope();
            var mediator = scope.GetInstance<IMediator>();

            var peekCommand = new PeekMasterDataCommand(recipientGln, bundleId);

            // Act
            var response = await mediator.Send(peekCommand).ConfigureAwait(false);

            // Assert
            Assert.NotNull(response);
            Assert.True(response.HasContent);
        }

        [Fact]
        public async Task PeekMasterDataCommand_SingleMeteringPointsNotification_ReturnsData()
        {
            // Arrange
            var recipientGln = new MockedGln();
            var bundleId = Guid.NewGuid().ToString();
            await AddMeteringPointsNotificationAsync(recipientGln).ConfigureAwait(false);

            await using var host = await MarketOperatorIntegrationTestHost
                .InitializeAsync()
                .ConfigureAwait(false);

            await using var scope = host.BeginScope();
            var mediator = scope.GetInstance<IMediator>();

            var peekCommand = new PeekMasterDataCommand(recipientGln, bundleId);

            // Act
            var response = await mediator.Send(peekCommand).ConfigureAwait(false);

            // Assert
            Assert.NotNull(response);
            Assert.True(response.HasContent);
        }

        [Fact]
        public async Task PeekMasterDataCommand_SingleChargesNotification_ReturnsData()
        {
            // Arrange
            var recipientGln = new MockedGln();
            var bundleId = Guid.NewGuid().ToString();
            await AddChargesNotificationAsync(recipientGln).ConfigureAwait(false);

            await using var host = await MarketOperatorIntegrationTestHost
                .InitializeAsync()
                .ConfigureAwait(false);

            await using var scope = host.BeginScope();
            var mediator = scope.GetInstance<IMediator>();

            var peekCommand = new PeekMasterDataCommand(recipientGln, bundleId);

            // Act
            var response = await mediator.Send(peekCommand).ConfigureAwait(false);

            // Assert
            Assert.NotNull(response);
            Assert.True(response.HasContent);
        }

        [Fact]
        public async Task PeekMasterDataCommand_SingleNotificationMultiplePeek_ReturnsData()
        {
            // Arrange
            var recipientGln = new MockedGln();
            var bundleId = Guid.NewGuid().ToString();
            await AddMeteringPointsNotificationAsync(recipientGln).ConfigureAwait(false);

            await using var host = await MarketOperatorIntegrationTestHost
                .InitializeAsync()
                .ConfigureAwait(false);

            await using var scope = host.BeginScope();
            var mediator = scope.GetInstance<IMediator>();

            var peekCommand = new PeekMasterDataCommand(recipientGln, bundleId);

            // Act
            var responseA = await mediator.Send(peekCommand).ConfigureAwait(false);
            var responseB = await mediator.Send(peekCommand).ConfigureAwait(false);

            // Assert
            Assert.NotNull(responseA);
            Assert.True(responseA.HasContent);
            Assert.NotNull(responseB);
            Assert.True(responseB.HasContent);
        }

        [Fact]
        public async Task PeekMasterDataCommand_AllThreeNotifications_ReturnsOldest()
        {
            // Arrange
            var recipientGln = new MockedGln();
            var expectedGuid = await AddMarketRolesNotificationAsync(recipientGln).ConfigureAwait(false);
            var unexpectedGuidA = await AddMeteringPointsNotificationAsync(recipientGln).ConfigureAwait(false);
            var unexpectedGuidB = await AddChargesNotificationAsync(recipientGln).ConfigureAwait(false);
            var bundleId = Guid.NewGuid().ToString();

            await using var host = await MarketOperatorIntegrationTestHost
                .InitializeAsync()
                .ConfigureAwait(false);

            await using var scope = host.BeginScope();
            var mediator = scope.GetInstance<IMediator>();

            var peekCommand = new PeekMasterDataCommand(recipientGln, bundleId);

            // Act
            var response = await mediator.Send(peekCommand).ConfigureAwait(false);

            // Assert
            Assert.NotNull(response);
            Assert.True(response.HasContent);

            var bundleContents = await response.Data.ReadAsDataBundleRequestAsync().ConfigureAwait(false);
            Assert.Contains(expectedGuid, bundleContents.DataAvailableNotificationIds);
            Assert.DoesNotContain(unexpectedGuidA, bundleContents.DataAvailableNotificationIds);
            Assert.DoesNotContain(unexpectedGuidB, bundleContents.DataAvailableNotificationIds);
        }

        private static async Task AddTimeSeriesNotificationAsync(string recipientGln)
        {
            var dataAvailableUuid = Guid.NewGuid();
            var dataAvailableCommand = new DataAvailableNotificationCommand(
                dataAvailableUuid.ToString(),
                recipientGln,
                "timeseries",
                "TimeSeries",
                false,
                1);

            await AddNotificationAsync(dataAvailableCommand).ConfigureAwait(false);
        }

        private static async Task AddAggregationsNotificationAsync(string recipientGln)
        {
            var dataAvailableUuid = Guid.NewGuid();
            var dataAvailableCommand = new DataAvailableNotificationCommand(
                dataAvailableUuid.ToString(),
                recipientGln,
                "aggregations",
                "Aggregations",
                false,
                1);

            await AddNotificationAsync(dataAvailableCommand).ConfigureAwait(false);
        }

        private static async Task<Guid> AddMarketRolesNotificationAsync(string recipientGln)
        {
            var dataAvailableUuid = Guid.NewGuid();
            var dataAvailableCommand = new DataAvailableNotificationCommand(
                dataAvailableUuid.ToString(),
                recipientGln,
                "marketroles",
                "MarketRoles",
                false,
                1);

            await AddNotificationAsync(dataAvailableCommand).ConfigureAwait(false);
            return dataAvailableUuid;
        }

        private static async Task<Guid> AddMeteringPointsNotificationAsync(string recipientGln)
        {
            var dataAvailableUuid = Guid.NewGuid();
            var dataAvailableCommand = new DataAvailableNotificationCommand(
                dataAvailableUuid.ToString(),
                recipientGln,
                "meteringpoints",
                "MeteringPoints",
                false,
                1);

            await AddNotificationAsync(dataAvailableCommand).ConfigureAwait(false);
            return dataAvailableUuid;
        }

        private static async Task<Guid> AddChargesNotificationAsync(string recipientGln)
        {
            var dataAvailableUuid = Guid.NewGuid();
            var dataAvailableCommand = new DataAvailableNotificationCommand(
                dataAvailableUuid.ToString(),
                recipientGln,
                "charges",
                "Charges",
                false,
                1);

            await AddNotificationAsync(dataAvailableCommand).ConfigureAwait(false);
            return dataAvailableUuid;
        }

        private static async Task AddNotificationAsync(DataAvailableNotificationCommand dataAvailableCommand)
        {
            await using var host = await SubDomainIntegrationTestHost
                .InitializeAsync()
                .ConfigureAwait(false);

            await using var scope = host.BeginScope();
            var mediator = scope.GetInstance<IMediator>();

            await mediator.Send(dataAvailableCommand).ConfigureAwait(false);
        }
    }
}
