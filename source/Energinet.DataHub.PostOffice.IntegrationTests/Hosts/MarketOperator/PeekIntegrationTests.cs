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
        public async Task PeekChargesCommand_InvalidCommand_ThrowsException()
        {
            // Arrange
            await using var host = await MarketOperatorIntegrationTestHost
                .InitializeAsync()
                .ConfigureAwait(false);

            await using var scope = host.BeginScope();
            var mediator = scope.GetInstance<IMediator>();

            var peekCommand = new PeekChargesCommand("   ", "   ");

            // Act + Assert
            await Assert.ThrowsAsync<ValidationException>(() => mediator.Send(peekCommand)).ConfigureAwait(false);
        }

        [Fact]
        public async Task PeekChargesCommand_Empty_ReturnsNothing()
        {
            // Arrange
            var recipientGln = new MockedGln();
            var unrelatedGln = new MockedGln();
            var bundleId = Guid.NewGuid().ToString();

            await AddChargesNotificationAsync(unrelatedGln).ConfigureAwait(false);

            await using var host = await MarketOperatorIntegrationTestHost
                .InitializeAsync()
                .ConfigureAwait(false);

            await using var scope = host.BeginScope();
            var mediator = scope.GetInstance<IMediator>();

            var peekCommand = new PeekChargesCommand(recipientGln, bundleId);

            // Act
            var response = await mediator.Send(peekCommand).ConfigureAwait(false);

            // Assert
            Assert.NotNull(response);
            Assert.False(response.HasContent);
        }

        [Fact]
        public async Task PeekChargesCommand_SingleNotification_ReturnsData()
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

            var peekCommand = new PeekChargesCommand(recipientGln, bundleId);

            // Act
            var response = await mediator.Send(peekCommand).ConfigureAwait(false);

            // Assert
            Assert.NotNull(response);
            Assert.True(response.HasContent);
        }

        [Fact]
        public async Task PeekChargesCommand_SingleNotificationMultiplePeek_ReturnsData()
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

            var peekCommand = new PeekChargesCommand(recipientGln, bundleId);

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
        public async Task PeekAggregationsOrTimeSeriesCommand_InvalidCommand_ThrowsException()
        {
            // Arrange
            await using var host = await MarketOperatorIntegrationTestHost
                .InitializeAsync()
                .ConfigureAwait(false);

            await using var scope = host.BeginScope();
            var mediator = scope.GetInstance<IMediator>();

            var peekCommand = new PeekAggregationsOrTimeSeriesCommand("   ", "    ");

            // Act + Assert
            await Assert.ThrowsAsync<ValidationException>(() => mediator.Send(peekCommand)).ConfigureAwait(false);
        }

        [Fact]
        public async Task PeekAggregationsOrTimeSeriesCommand_Empty_ReturnsNothing()
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

            var peekCommand = new PeekAggregationsOrTimeSeriesCommand(recipientGln, bundleId);

            // Act
            var response = await mediator.Send(peekCommand).ConfigureAwait(false);

            // Assert
            Assert.NotNull(response);
            Assert.False(response.HasContent);
        }

        [Fact]
        public async Task PeekAggregationsOrTimeSeriesCommand_SingleAggregationNotification_ReturnsData()
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

            var peekCommand = new PeekAggregationsOrTimeSeriesCommand(recipientGln, bundleId);

            // Act
            var response = await mediator.Send(peekCommand).ConfigureAwait(false);

            // Assert
            Assert.NotNull(response);
            Assert.True(response.HasContent);
        }

        [Fact]
        public async Task PeekAggregationsOrTimeSeriesCommand_SingleTimeSeriesNotification_ReturnsData()
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

            var peekCommand = new PeekAggregationsOrTimeSeriesCommand(recipientGln, bundleId);

            // Act
            var response = await mediator.Send(peekCommand).ConfigureAwait(false);

            // Assert
            Assert.NotNull(response);
            Assert.True(response.HasContent);
        }

        [Fact]
        public async Task PeekAggregationsOrTimeSeriesCommand_SingleNotificationMultiplePeek_ReturnsData()
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

            var peekCommand = new PeekAggregationsOrTimeSeriesCommand(recipientGln, bundleId);

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
        public async Task PeekAggregationsOrTimeSeriesCommand_BothNotifications_ReturnsAggregations()
        {
            // Arrange
            var recipientGln = new MockedGln();
            var unexpectedGuid = await AddTimeSeriesNotificationAsync(recipientGln).ConfigureAwait(false);
            var expectedGuid = await AddAggregationsNotificationAsync(recipientGln).ConfigureAwait(false);
            var bundleId = Guid.NewGuid().ToString();

            await using var host = await MarketOperatorIntegrationTestHost
                .InitializeAsync()
                .ConfigureAwait(false);

            await using var scope = host.BeginScope();
            var mediator = scope.GetInstance<IMediator>();

            var peekCommand = new PeekAggregationsOrTimeSeriesCommand(recipientGln, bundleId);

            // Act
            var response = await mediator.Send(peekCommand).ConfigureAwait(false);

            // Assert
            Assert.NotNull(response);
            Assert.True(response.HasContent);

            var bundleContents = await response.Data.ReadAsDataBundleRequestAsync().ConfigureAwait(false);
            Assert.Contains(expectedGuid, bundleContents.DataAvailableNotificationIds);
            Assert.DoesNotContain(unexpectedGuid, bundleContents.DataAvailableNotificationIds);
        }

        private static async Task AddChargesNotificationAsync(string recipientGln)
        {
            var dataAvailableUuid = Guid.NewGuid();
            var dataAvailableCommand = new DataAvailableNotificationCommand(
                dataAvailableUuid.ToString(),
                recipientGln,
                "charges",
                "Charges",
                false,
                1);

            await using var host = await SubDomainIntegrationTestHost
                .InitializeAsync()
                .ConfigureAwait(false);

            await using var scope = host.BeginScope();
            var mediator = scope.GetInstance<IMediator>();

            await mediator.Send(dataAvailableCommand).ConfigureAwait(false);
        }

        private static async Task<Guid> AddTimeSeriesNotificationAsync(string recipientGln)
        {
            var dataAvailableUuid = Guid.NewGuid();
            var dataAvailableCommand = new DataAvailableNotificationCommand(
                dataAvailableUuid.ToString(),
                recipientGln,
                "timeseries",
                "TimeSeries",
                false,
                1);

            await using var host = await SubDomainIntegrationTestHost
                .InitializeAsync()
                .ConfigureAwait(false);

            await using var scope = host.BeginScope();
            var mediator = scope.GetInstance<IMediator>();

            await mediator.Send(dataAvailableCommand).ConfigureAwait(false);
            return dataAvailableUuid;
        }

        private static async Task<Guid> AddAggregationsNotificationAsync(string recipientGln)
        {
            var dataAvailableUuid = Guid.NewGuid();
            var dataAvailableCommand = new DataAvailableNotificationCommand(
                dataAvailableUuid.ToString(),
                recipientGln,
                "aggregations",
                "Aggregations",
                false,
                1);

            await using var host = await SubDomainIntegrationTestHost
                .InitializeAsync()
                .ConfigureAwait(false);

            await using var scope = host.BeginScope();
            var mediator = scope.GetInstance<IMediator>();

            await mediator.Send(dataAvailableCommand).ConfigureAwait(false);
            return dataAvailableUuid;
        }
    }
}
