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

using System.Threading.Tasks;
using Energinet.DataHub.PostOffice.Domain.Model;
using Energinet.DataHub.PostOffice.Infrastructure.Repositories;
using Energinet.DataHub.PostOffice.Infrastructure.Repositories.Containers;
using Xunit;
using Xunit.Categories;

namespace Energinet.DataHub.PostOffice.IntegrationTests.Repositories
{
    [Collection("IntegrationTest")]
    [IntegrationTest]
    public class SequenceNumberRepositoryIntegrationTests
    {
        [Fact]
        public async Task AdvanceSequenceNumberAsync_WriteAndReadBack_ReturnsCorrectNumber()
        {
            // Arrange
            await using var host = await MarketOperatorIntegrationTestHost.InitializeAsync().ConfigureAwait(false);
            var scope = host.BeginScope();

            var container = scope.GetInstance<IDataAvailableNotificationRepositoryContainer>();
            var target = new SequenceNumberRepository(container);
            var expected = 100000;

            // Act
            await target.AdvanceSequenceNumberAsync(new SequenceNumber(expected)).ConfigureAwait(false);

            // Assert
            var actual = await target.GetMaximumSequenceNumberAsync().ConfigureAwait(false);
            Assert.Equal(expected, actual.Value);
        }
    }
}
