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
using System.Threading.Tasks;
using Energinet.DataHub.PostOffice.Application.Commands;
using Energinet.DataHub.PostOffice.Domain.Model;
using Energinet.DataHub.PostOffice.Domain.Services;
using Energinet.DataHub.PostOffice.Infrastructure.Repositories;
using Energinet.DataHub.PostOffice.Infrastructure.Repositories.Containers;
using Energinet.DataHub.PostOffice.IntegrationTests.Common;
using MediatR;
using Xunit;
using Xunit.Categories;

namespace Energinet.DataHub.PostOffice.IntegrationTests.Hosts.Operations
{
    [Collection("IntegrationTest")]
    [IntegrationTest]
    public class DequeueCleanUpIntegrationTests
    {
        [Fact]
        public async Task DequeueCleanUp_WithData_RunsWithoutException()
        {
            // Arrange
            var marketOperator = new Domain.Model.MarketOperator(new MockedGln());
            var notificationIds = new[] { new Uuid(Guid.NewGuid()) };

            await using var host = await OperationsIntegrationHost
                .InitializeAsync()
                .ConfigureAwait(false);

            await using var scope = host.BeginScope();
            var mediator = scope.GetInstance<IMediator>();

            var container = scope.GetInstance<IBundleRepositoryContainer>();
            var storageService = scope.GetInstance<IMarketOperatorDataStorageService>();
            var bundleRepository = new BundleRepository(container, storageService);

            var dataAvailableContainer = scope.GetInstance<IDataAvailableNotificationRepositoryContainer>();
            var dataAvailableRepository = new DataAvailableNotificationRepository(dataAvailableContainer);

            var dataAvailableToDequeueAndArchive = CrateDataAvailableNotification(notificationIds.First(), marketOperator);
            await dataAvailableRepository.SaveAsync(dataAvailableToDequeueAndArchive).ConfigureAwait(false);

            var bundle = CreateBundle(marketOperator, notificationIds);
            await bundleRepository.TryAddNextUnacknowledgedAsync(bundle).ConfigureAwait(false);

            var dequeueCleanUpCommand = new DequeueCleanUpCommand(bundle.BundleId);

            // Act
            var response = await mediator.Send(dequeueCleanUpCommand).ConfigureAwait(false);

            // Assert
            Assert.True(response.Completed);
            var bundleDequeued = await bundleRepository.GetBundleAsync(bundle.BundleId).ConfigureAwait(false);
            Assert.True(bundleDequeued is { NotificationsArchived: true });
        }

        private static Bundle CreateBundle(
            Domain.Model.MarketOperator recipient,
            IReadOnlyList<Uuid> notificationIds,
            IBundleContent? bundleContent = null,
            DomainOrigin domainOrigin = DomainOrigin.TimeSeries)
        {
            return new Bundle(
                new Uuid(Guid.NewGuid()),
                recipient,
                domainOrigin,
                new ContentType("fake_value"),
                notificationIds,
                bundleContent);
        }

        private static DataAvailableNotification CrateDataAvailableNotification(
            Uuid id,
            Domain.Model.MarketOperator recipient,
            DomainOrigin domain = DomainOrigin.TimeSeries)
        {
            return new DataAvailableNotification(
                id,
                recipient,
                new ContentType("fake_value"),
                domain,
                new SupportsBundling(false),
                new Weight(1));
        }
    }
}
