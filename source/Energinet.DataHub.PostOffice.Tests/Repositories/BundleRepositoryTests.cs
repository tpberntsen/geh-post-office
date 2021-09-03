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

using System;
using System.Linq;
using System.Threading.Tasks;
using Energinet.DataHub.PostOffice.Domain.Model;
using Energinet.DataHub.PostOffice.Domain.Services;
using Energinet.DataHub.PostOffice.Infrastructure.Repositories;
using Energinet.DataHub.PostOffice.Infrastructure.Repositories.Containers;
using Energinet.DataHub.PostOffice.Infrastructure.Services;
using Moq;
using Xunit;
using Xunit.Categories;

namespace Energinet.DataHub.PostOffice.Tests.Repositories
{
    [UnitTest]
    public class BundleRepositoryTests
    {
        private IMarketOperatorDataStorageService _marketOperatorDataStorageService = new MarketOperatorDataStorageService();

        [Fact]
        public async Task Peek_With_Null_Recipient_ThrowsException()
        {
            // Arrange
            var bundleRepositoryContainer = new Mock<IBundleRepositoryContainer>();
            var target = new BundleRepository(bundleRepositoryContainer.Object, _marketOperatorDataStorageService);

            // Act + Assert
            await Assert
                .ThrowsAsync<ArgumentNullException>(() => target.GetNextUnacknowledgedAsync(null!))
                .ConfigureAwait(false);
        }

        [Fact]
        public async Task Dequeue_With_Null_Argument_ThrowsException()
        {
            // Arrange
            var bundleRepositoryContainer = new Mock<IBundleRepositoryContainer>();
            var target = new BundleRepository(bundleRepositoryContainer.Object, _marketOperatorDataStorageService);

            // Act + Assert
            await Assert
                .ThrowsAsync<ArgumentNullException>(() => target.AcknowledgeAsync(null!))
                .ConfigureAwait(false);
        }

        [Fact]
        public async Task CreateBundle_Empty_List_Argument_ThrowsException()
        {
            // Arrange
            var bundleRepositoryContainer = new Mock<IBundleRepositoryContainer>();
            var target = new BundleRepository(bundleRepositoryContainer.Object, _marketOperatorDataStorageService);

            // Act + Assert
            await Assert
                .ThrowsAsync<ArgumentOutOfRangeException>(() =>
                    target.CreateBundleAsync(
                        Enumerable.Empty<DataAvailableNotification>(),
                        new Uri("https://test.test.dk")))
                .ConfigureAwait(false);
        }
    }
}
