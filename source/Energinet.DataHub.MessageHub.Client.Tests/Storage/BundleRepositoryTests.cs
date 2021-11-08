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
using Energinet.DataHub.MessageHub.Client.Storage;
using Energinet.DataHub.MessageHub.Model.Model;
using Microsoft.Azure.Cosmos;
using Xunit;
using Xunit.Categories;

namespace Energinet.DataHub.MessageHub.Client.Tests.Storage
{
    [UnitTest]
    public class BundleRepositoryTests
    {
        [Fact]
        public async Task TestFetchIds()
        {
            var cosmosClient = new CosmosClient("AccountEndpoint=https://localhost:8081/;AccountKey=C2y6yDjf5/R+ob0N8A7Cgv30VRDJIWEHLM+4QDU5DE2nQ9nDuVTqobD4b8mGGyPMbIZnqyMsEcaGQy67XIw/Jw==");
            var storageConfig = new StorageConfig(
                "blob",
                string.Empty,
                "AccountEndpoint=https://localhost:8081/;AccountKey=C2y6yDjf5/R+ob0N8A7Cgv30VRDJIWEHLM+4QDU5DE2nQ9nDuVTqobD4b8mGGyPMbIZnqyMsEcaGQy67XIw/Jw==",
                "post-office");

            var bundleRepo = new BundleRepository(cosmosClient, storageConfig);
            var requestDto = new DataBundleRequestDto("f379ac91-1e95-40f3-8e9f-a299be59c25a", Enumerable.Empty<Guid>());
            var guids = await bundleRepo.GetDataAvailableIdsForRequestAsync(requestDto);
        }
    }
}
