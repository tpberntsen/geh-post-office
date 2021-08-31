// // Copyright 2020 Energinet DataHub A/S
// //
// // Licensed under the Apache License, Version 2.0 (the "License2");
// // you may not use this file except in compliance with the License.
// // You may obtain a copy of the License at
// //
// //     http://www.apache.org/licenses/LICENSE-2.0
// //
// // Unless required by applicable law or agreed to in writing, software
// // distributed under the License is distributed on an "AS IS" BASIS,
// // WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// // See the License for the specific language governing permissions and
// // limitations under the License.

using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Energinet.DataHub.PostOffice.Domain.Model;
using Energinet.DataHub.PostOffice.Infrastructure.Repositories;
using Energinet.DataHub.PostOffice.Infrastructure.Repositories.Containers;
using Microsoft.Azure.Cosmos;
using Xunit;
using Xunit.Categories;

namespace Energinet.DataHub.PostOffice.IntegrationTests.Repositories
{
    [Collection("IntegrationTest")]
    [IntegrationTest]
    public sealed class BundleRepositoryIntegrationTests
    {
        [Fact]
        public async Task CreateBundle_Should_Return_Bundle()
        {
            // Arrange
            var recipient = new MarketOperator(System.Guid.NewGuid().ToString());
            var messageType = new ContentType(1, "fake_value");
            await using var host = await InboundIntegrationTestHost.InitializeAsync().ConfigureAwait(false);
            var scope = host.BeginScope();

            var dataAvailableNotificationIds = new List<DataAvailableNotification>()
            {
                CreateDataAvailableNotifications(recipient, messageType),
                CreateDataAvailableNotifications(recipient, messageType),
                CreateDataAvailableNotifications(recipient, messageType)
            };
            var client = scope.GetInstance<CosmosClient>();
            var bundleRepository = new BundleRepository(new BundleRepositoryContainer(client));

            //Act
            var bundle = await bundleRepository.CreateBundleAsync(dataAvailableNotificationIds)
                .ConfigureAwait(false);

            //Assert
            Assert.NotNull(bundle);
            Assert.Equal(3, bundle.NotificationIds.Count());
        }

        [Fact]
        public async Task Peek_Should_Return_Bundle_Created_For_Same_Recipient()
        {
            // Arrange
            var recipient = new MarketOperator(System.Guid.NewGuid().ToString());
            var messageType = new ContentType(1, "fake_value");
            await using var host = await InboundIntegrationTestHost.InitializeAsync().ConfigureAwait(false);
            var scope = host.BeginScope();
            var dataAvailableNotifications = new List<DataAvailableNotification>()
            {
                CreateDataAvailableNotifications(recipient, messageType),
                CreateDataAvailableNotifications(recipient, messageType),
                CreateDataAvailableNotifications(recipient, messageType),
            };
            var client = scope.GetInstance<CosmosClient>();
            var bundleRepository = new BundleRepository(new BundleRepositoryContainer(client));

            //Act
            var createdBundle = await bundleRepository
                .CreateBundleAsync(dataAvailableNotifications)
                .ConfigureAwait(false);

            var peakBundle = await bundleRepository
                .GetNextUnacknowledgedAsync(recipient)
                .ConfigureAwait(false);

            //Assert
            Assert.NotNull(createdBundle);
            Assert.NotNull(peakBundle);
            Assert.Equal(createdBundle?.BundleId, peakBundle?.BundleId);
            Assert.Equal(createdBundle?.NotificationIds.Count(), peakBundle?.NotificationIds.Count());
            Assert.True(createdBundle!.NotificationIds.All(x => peakBundle!.NotificationIds.Contains(x)));
        }

        [Fact]
        public async Task Peek_Should_Not_Return_Bundle_Created_For_Another_Recipient()
        {
            var recipient = new MarketOperator(System.Guid.NewGuid().ToString());
            var peakRecipient = new MarketOperator(System.Guid.NewGuid().ToString());
            var messageType = new ContentType(1, "fake_value");
            await using var host = await InboundIntegrationTestHost
                .InitializeAsync()
                .ConfigureAwait(false);

            var scope = host.BeginScope();
            var dataAvailableNotifications = new List<DataAvailableNotification>()
            {
                CreateDataAvailableNotifications(recipient, messageType)
            };
            var client = scope.GetInstance<CosmosClient>();
            var bundleRepository = new BundleRepository(new BundleRepositoryContainer(client));

            //Act
            var createdBundle = await bundleRepository
                .CreateBundleAsync(dataAvailableNotifications)
                .ConfigureAwait(false);
            var peakBundle = await bundleRepository
                .GetNextUnacknowledgedAsync(peakRecipient)
                .ConfigureAwait(false);

            //Assert
            Assert.NotNull(createdBundle);
            Assert.Null(peakBundle);
        }

        [Fact]
        public async Task Dequeue_Should_Set_Bundle_Dequeued()
        {
            var recipient = new MarketOperator(System.Guid.NewGuid().ToString());
            await using var host = await InboundIntegrationTestHost.InitializeAsync().ConfigureAwait(false);
            var scope = host.BeginScope();
            var messageType = new ContentType(1, "fake_value");
            var dataAvailableNotifications = new List<DataAvailableNotification>()
            {
                CreateDataAvailableNotifications(recipient, messageType),
            };
            var client = scope.GetInstance<CosmosClient>();
            var bundleRepository = new BundleRepository(new BundleRepositoryContainer(client));

            //Act
            var createdBundle = await bundleRepository
                .CreateBundleAsync(dataAvailableNotifications)
                .ConfigureAwait(false);

            await bundleRepository
                .AcknowledgeAsync(createdBundle.BundleId)
                .ConfigureAwait(false);

            var peakResult = await bundleRepository
                .GetNextUnacknowledgedAsync(recipient)
                .ConfigureAwait(false);

            //Assert
            Assert.NotNull(createdBundle);
            Assert.Null(peakResult);
        }

        private static DataAvailableNotification CreateDataAvailableNotifications(
            MarketOperator recipient,
            ContentType contentType)
        {
            return new DataAvailableNotification(
                new Uuid(System.Guid.NewGuid().ToString()),
                recipient,
                contentType,
                SubDomain.TimeSeries,
                new Weight(1));
        }
    }
}
