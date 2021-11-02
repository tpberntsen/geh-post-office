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
using Energinet.DataHub.MessageHub.Model.Model;
using Microsoft.Azure.Cosmos;

namespace Energinet.DataHub.MessageHub.Client.Storage
{
    public class BundleRepository : IBundleRepository
    {
        private readonly CosmosClient _cosmosClient;
        private readonly StorageConfig _storageConfig;

        public BundleRepository(CosmosClient cosmosClient, StorageConfig storageConfig)
        {
            _cosmosClient = cosmosClient;
            _storageConfig = storageConfig;
        }

        public async Task<IReadOnlyList<Guid>> GetDataAvailableIdsForRequestAsync(DataBundleRequestDto requestDto)
        {
            if (requestDto is null)
                throw new ArgumentNullException(nameof(requestDto));

            var container = _cosmosClient.GetContainer(_storageConfig.MessageHubDatabaseId, "bundles");
            var query = new QueryDefinition("SELECT c.NotificationIds FROM c WHERE c.id = @id")
                .WithParameter("@id", requestDto.IdempotencyId);
            var documentQuery = new QueryDefinition("SELECT * FROM c in c.NotificationIds WHERE c.id = @id")
                .WithParameter($"@id", requestDto.IdempotencyId);
            using FeedIterator<Guid> feedIterator =
                container.GetItemQueryIterator<Guid>(query);

            var resultList = new List<Guid>();
            while (feedIterator.HasMoreResults)
            {
                var guid = await feedIterator
                    .ReadNextAsync()
                    .ConfigureAwait(false);
                resultList.Add(guid.SingleOrDefault());
            }

            return resultList;
        }
    }
}
