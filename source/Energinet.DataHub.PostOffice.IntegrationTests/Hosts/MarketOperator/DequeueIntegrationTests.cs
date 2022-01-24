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
using System.Linq;
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
    public sealed class DequeueIntegrationTests
    {
        [Fact]
        public async Task Dequeue_InvalidCommand_ThrowsException()
        {
            // Arrange
            await using var host = await MarketOperatorIntegrationTestHost
                .InitializeAsync()
                .ConfigureAwait(false);

            await using var scope = host.BeginScope();
            var mediator = scope.GetInstance<IMediator>();

            var dequeueCommand = new DequeueCommand("  ", "  ");

            // Act + Assert
            await Assert.ThrowsAsync<ValidationException>(() => mediator.Send(dequeueCommand)).ConfigureAwait(false);
        }

        [Fact]
        public async Task Dequeue_NoData_ReturnsNotDequeued()
        {
            // Arrange
            var recipientGln = new MockedGln();
            var bundleUuid = Guid.NewGuid().ToString();

            await using var host = await MarketOperatorIntegrationTestHost
                .InitializeAsync()
                .ConfigureAwait(false);

            await using var scope = host.BeginScope();
            var mediator = scope.GetInstance<IMediator>();

            var dequeueCommand = new DequeueCommand(recipientGln, bundleUuid);

            // Act
            var response = await mediator.Send(dequeueCommand).ConfigureAwait(false);

            // Assert
            Assert.NotNull(response);
            Assert.False(response.IsDequeued);
        }

        [Fact]
        public async Task Dequeue_HasData_ReturnsIsDequeued()
        {
            // Arrange
            var recipientGln = new MockedGln();
            var bundleId = Guid.NewGuid().ToString();
            await AddDataAvailableNotificationAsync(recipientGln).ConfigureAwait(false);

            await using var host = await MarketOperatorIntegrationTestHost
                .InitializeAsync()
                .ConfigureAwait(false);

            await using var scope = host.BeginScope();
            var mediator = scope.GetInstance<IMediator>();

            var peekResponse = await mediator.Send(new PeekCommand(recipientGln, bundleId)).ConfigureAwait(false);
            var bundleUuid = await ReadBundleIdAsync(peekResponse).ConfigureAwait(false);

            var dequeueCommand = new DequeueCommand(recipientGln, bundleUuid);

            // Act
            var response = await mediator.Send(dequeueCommand).ConfigureAwait(false);

            // Assert
            Assert.NotNull(response);
            Assert.True(response.IsDequeued);
        }

        [Fact]
        public async Task Dequeue_HasDataTwoDequeue_ReturnsNotDequeued()
        {
            // Arrange
            var recipientGln = new MockedGln();
            var bundleId = Guid.NewGuid().ToString();
            await AddDataAvailableNotificationAsync(recipientGln).ConfigureAwait(false);

            await using var host = await MarketOperatorIntegrationTestHost
                .InitializeAsync()
                .ConfigureAwait(false);

            await using var scope = host.BeginScope();
            var mediator = scope.GetInstance<IMediator>();

            var peekResponse = await mediator.Send(new PeekCommand(recipientGln, bundleId)).ConfigureAwait(false);
            var bundleUuid = await ReadBundleIdAsync(peekResponse).ConfigureAwait(false);

            var dequeueCommand = new DequeueCommand(recipientGln, bundleUuid);

            // Act
            var responseA = await mediator.Send(dequeueCommand).ConfigureAwait(false);
            var responseB = await mediator.Send(dequeueCommand).ConfigureAwait(false);

            // Assert
            Assert.NotNull(responseA);
            Assert.True(responseA.IsDequeued);
            Assert.NotNull(responseB);
            Assert.False(responseB.IsDequeued);
        }

        [Fact]
        public async Task Dequeue_DifferentRecipient_ReturnsNotDequeued()
        {
            // Arrange
            var recipientGln = new MockedGln();
            var unrelatedGln = new MockedGln();
            var bundleId = Guid.NewGuid().ToString();
            await AddDataAvailableNotificationAsync(recipientGln).ConfigureAwait(false);

            await using var host = await MarketOperatorIntegrationTestHost
                .InitializeAsync()
                .ConfigureAwait(false);

            await using var scope = host.BeginScope();
            var mediator = scope.GetInstance<IMediator>();

            var peekResponse = await mediator.Send(new PeekCommand(recipientGln, bundleId)).ConfigureAwait(false);
            var bundleUuid = await ReadBundleIdAsync(peekResponse).ConfigureAwait(false);

            var dequeueCommand = new DequeueCommand(unrelatedGln, bundleUuid);

            // Act
            var response = await mediator.Send(dequeueCommand).ConfigureAwait(false);

            // Assert
            Assert.NotNull(response);
            Assert.False(response.IsDequeued);
        }

        private static async Task AddDataAvailableNotificationAsync(string recipientGln)
        {
            var dataAvailableUuid = Guid.NewGuid().ToString();
            var dataAvailableNotification = new DataAvailableNotificationDto(
                dataAvailableUuid,
                recipientGln,
                "timeseries",
                "timeseries",
                false,
                1,
                1);

            await using var host = await SubDomainIntegrationTestHost
                .InitializeAsync()
                .ConfigureAwait(false);

            await using var scope = host.BeginScope();
            var mediator = scope.GetInstance<IMediator>();

            var command = new InsertDataAvailableNotificationsCommand(new[] { dataAvailableNotification });
            await mediator.Send(command).ConfigureAwait(false);
        }

        private static async Task<string> ReadBundleIdAsync(PeekResponse response)
        {
            Assert.True(response.HasContent);
            var bundleContents = await response.Data
                .ReadAsDataBundleRequestAsync()
                .ConfigureAwait(false);

            var bundleId = bundleContents.IdempotencyId.Split("_", StringSplitOptions.RemoveEmptyEntries).First();
            return bundleId;
        }
    }
}
