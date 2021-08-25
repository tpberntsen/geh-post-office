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
using System.Threading.Tasks;
using Energinet.DataHub.PostOffice.Domain.Model;
using Energinet.DataHub.PostOffice.Infrastructure.Repositories;
using Energinet.DataHub.PostOffice.Infrastructure.Repositories.Containers;
using FluentAssertions;
using Microsoft.Azure.Cosmos;
using Xunit;
using Xunit.Categories;

namespace Energinet.DataHub.PostOffice.IntegrationTests.Repositories
{
    [IntegrationTest]
    public sealed class BundleRepositoryIntegrationTests
    {
        [Fact]
        public async Task CreateBundle_Should_Return_Bundle()
        {
            // Arrange
            var recipient = new Recipient("fake_value");
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
            var container = client.GetContainer("post-office", "bundles");
            BundleRepository bundleRepository = new BundleRepository(new BundleRepositoryContainer(container));

            //Act
            var bundle = await bundleRepository.CreateBundleAsync(dataAvailableNotificationIds, recipient)
                .ConfigureAwait(false);
            //Assert
            Assert.NotNull(bundle);
        }

        private static DataAvailableNotification CreateDataAvailableNotifications(
            Recipient recipient,
            MessageType messageType)
        {
            return new DataAvailableNotification(
                new Uuid("fake_value"),
                recipient,
                messageType,
                Origin.TimeSeries,
                new Weight(1));
        }
    }
}
