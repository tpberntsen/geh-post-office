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
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Energinet.DataHub.PostOffice.Domain.Model;
using Energinet.DataHub.PostOffice.Infrastructure.Entities;
using Energinet.DataHub.PostOffice.Infrastructure.Repositories;
using Energinet.DataHub.PostOffice.Infrastructure.Repositories.Containers;
using FluentAssertions;
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
            var recipient = new Recipient(System.Guid.NewGuid().ToString());
            var messageType = new MessageType(1, "fake_value");
            await using var host = await InboundIntegrationTestHost.InitializeAsync().ConfigureAwait(false);
            var scope = host.BeginScope();

            var dataAvailableNotificationIds = new List<DataAvailableNotification>()
            {
                CreateDataAvailableNotifications(recipient, messageType),
                CreateDataAvailableNotifications(recipient, messageType),
                CreateDataAvailableNotifications(recipient, messageType)
            };

            var client = scope.GetInstance<CosmosClient>();
            var container = client.GetContainer("post-office", "bundles"); // TODO: Add config variables once config is in place.
            BundleRepository bundleRepository = new BundleRepository(new BundleRepositoryContainer(client));

            //Act
            var bundle = await bundleRepository.CreateBundleAsync(dataAvailableNotificationIds, recipient)
                .ConfigureAwait(false);
            //Assert
            Assert.NotNull(bundle);
        }

        [Fact]
        public async Task Peek_Should_Return_Bundle()
        {
            // Arrange
            var recipient = new Recipient(System.Guid.NewGuid().ToString());
            var messageType = new MessageType(1, "fake_value");
            await using var host = await InboundIntegrationTestHost.InitializeAsync().ConfigureAwait(false);
            var scope = host.BeginScope();

            var dataAvailableNotifications = new List<DataAvailableNotification>()
            {
                CreateDataAvailableNotifications(recipient, messageType),
            }.Select(x => new Uuid(x.Id.Value));

            var client = scope.GetInstance<CosmosClient>();
            var container = client.GetContainer("post-office", "bundles"); // TODO: Add config variables once config is in place.
            BundleRepository bundleRepository = new BundleRepository(new BundleRepositoryContainer(client));
            var testBundle = new BundleDocument(recipient, new Uuid("39272D67-6B63-4BE3-83CC-4AC0D2619F8A"), dataAvailableNotifications, false);

            //Act
            var insertResult = await container
                .UpsertItemAsync(testBundle)
                .ConfigureAwait(false);

            var peakBundle = await bundleRepository
                .PeekAsync(recipient)
                .ConfigureAwait(false);

            //Assert
            Assert.NotNull(peakBundle);
            Assert.Equal(testBundle.Id, peakBundle?.Id.Value);
            Assert.Equal(testBundle.NotificationsIds.Count(), peakBundle?.NotificationsIds.Count());
        }

        [Fact]
        public async Task Peek_Should_Not_Return_Bundle()
        {
            var recipient = new Recipient(System.Guid.NewGuid().ToString());
            var peakRecipient = new Recipient(System.Guid.NewGuid().ToString());
            var messageType = new MessageType(1, "fake_value");
            await using var host = await InboundIntegrationTestHost.InitializeAsync().ConfigureAwait(false);
            var scope = host.BeginScope();

            var dataAvailableNotifications = new List<DataAvailableNotification>()
            {
                CreateDataAvailableNotifications(recipient, messageType),
            }.Select(x => new Uuid(x.Id.Value));

            var client = scope.GetInstance<CosmosClient>();
            var container = client.GetContainer("post-office", "bundles"); // TODO: Add config variables once config is in place.
            BundleRepository bundleRepository = new BundleRepository(new BundleRepositoryContainer(client));

            var testBundle = new BundleDocument(recipient, new Uuid("9F34C9BB-C236-42DC-837F-0E04A898E1CB"), dataAvailableNotifications, false);

            //Act
            var insertResult = await container
                .UpsertItemAsync(testBundle)
                .ConfigureAwait(false);

            var peakBundle = await bundleRepository
                .PeekAsync(peakRecipient)
                .ConfigureAwait(false);

            //Assert
            Assert.Null(peakBundle);
        }

        [Fact]
        public async Task Dequeue_Should_Set_Bundle_Dequeued()
        {
            var recipient = new Recipient(System.Guid.NewGuid().ToString());
            await using var host = await InboundIntegrationTestHost.InitializeAsync().ConfigureAwait(false);
            var scope = host.BeginScope();
            var messageType = new MessageType(1, "fake_value");
            var dataAvailableNotifications =
                new List<DataAvailableNotification>() { CreateDataAvailableNotifications(recipient, messageType), };

            var dataAvailableUuids = dataAvailableNotifications.Select(x => new Uuid(x.Id.Value));
            var bundleUuid = new Uuid(System.Guid.NewGuid().ToString());
            var client = scope.GetInstance<CosmosClient>();
            var container = client.GetContainer("post-office", "bundles"); // TODO: Add config variables once config is in place.
            BundleRepository bundleRepository = new BundleRepository(new BundleRepositoryContainer(client));
            var testBundle = new BundleDocument(recipient, bundleUuid, dataAvailableUuids, false);

            //Act
            var insertResult = await container
                .UpsertItemAsync(testBundle)
                .ConfigureAwait(false);

            await bundleRepository.DequeueAsync(bundleUuid).ConfigureAwait(false);

            var peakResult = await bundleRepository.PeekAsync(recipient).ConfigureAwait(false);

            Assert.Null(peakResult);
        }

        private static DataAvailableNotification CreateDataAvailableNotifications(
            Recipient recipient,
            MessageType messageType)
        {
            return new DataAvailableNotification(
                new Uuid(System.Guid.NewGuid().ToString()),
                recipient,
                messageType,
                Origin.TimeSeries,
                new Weight(1));
        }
    }
}
