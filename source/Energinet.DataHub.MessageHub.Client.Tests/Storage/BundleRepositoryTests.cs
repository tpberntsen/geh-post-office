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
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Energinet.DataHub.MessageHub.Client.Storage;
using Energinet.DataHub.MessageHub.Client.Storage.Documents;
using Energinet.DataHub.MessageHub.Model.Model;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Cosmos.Fluent;
using Xunit;
using Xunit.Categories;

namespace Energinet.DataHub.MessageHub.Client.Tests.Storage
{
    [UnitTest]
    public class BundleRepositoryTests
    {
        //[Fact(Skip = "Needs adjustment to run in CI/CD")]
        [Fact]
        public async Task TestFetchIds()
        {
            // arrange
            var storageConfig = new StorageConfig(
                "blob",
                string.Empty,
                "AccountEndpoint=https://localhost:8081/;AccountKey=C2y6yDjf5/R+ob0N8A7Cgv30VRDJIWEHLM+4QDU5DE2nQ9nDuVTqobD4b8mGGyPMbIZnqyMsEcaGQy67XIw/Jw==",
                "post-office");
            var cosmosClient = new CosmosClientBuilder("AccountEndpoint=https://localhost:8081/;AccountKey=C2y6yDjf5/R+ob0N8A7Cgv30VRDJIWEHLM+4QDU5DE2nQ9nDuVTqobD4b8mGGyPMbIZnqyMsEcaGQy67XIw/Jw==")
                .WithBulkExecution(false)
                .WithSerializerOptions(new CosmosSerializationOptions { PropertyNamingPolicy = CosmosPropertyNamingPolicy.CamelCase })
                .Build();

            var bundleId = await AddTestBundle(cosmosClient, storageConfig);
            var bundleRepo = new BundleRepository(cosmosClient, storageConfig);
            var requestDto = new DataBundleRequestDto(bundleId, Enumerable.Empty<Guid>());
            var expected = new List<Guid>() { Guid.Parse("364710E4-051B-47DF-8020-7C1A589268BF"), Guid.Parse("3D373E58-2AFE-4793-9025-060FC433AC7A"), Guid.Parse("9F1C27BC-258E-458C-9001-24246566290F") };

            // act
            var actual = await bundleRepo.GetDataAvailableIdsForRequestAsync(requestDto);

            // assert
            Assert.Equal(expected, actual);
        }

        private static async Task<string> AddTestBundle(CosmosClient client, StorageConfig config)
        {
            var bundle = new CosmosBundleDocument()
            {
                Id = "35D0E289-4222-4900-AD4C-5918BAC80942",
                NotificationIds = new List<string>()
                {
                    "364710E4-051B-47DF-8020-7C1A589268BF",
                    "3D373E58-2AFE-4793-9025-060FC433AC7A",
                    "9F1C27BC-258E-458C-9001-24246566290F"
                }
            };

            var database = client.GetDatabase(config.MessageHubDatabaseId);
            var bundlesResponse = await database
                .CreateContainerIfNotExistsAsync("bundles", "/pk")
                .ConfigureAwait(true);

            Container container = bundlesResponse.Container;
            var result = await container.CreateItemAsync(bundle);
            return bundle.Id;
        }
    }
}
