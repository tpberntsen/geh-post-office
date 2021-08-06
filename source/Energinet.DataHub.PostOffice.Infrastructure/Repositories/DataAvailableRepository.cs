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
using System.Net;
using System.Threading.Tasks;
using Energinet.DataHub.PostOffice.Domain;
using Energinet.DataHub.PostOffice.Domain.Repositories;
using Microsoft.Azure.Cosmos;

namespace Energinet.DataHub.PostOffice.Infrastructure.Repositories
{
    public sealed class DataAvailableRepository : IDataAvailableRepository
    {
        private const string ContainerName = "dataavailable";

        private readonly CosmosClient _cosmosClient;
        private readonly CosmosDatabaseConfig _cosmosConfig;

        public DataAvailableRepository(
            CosmosClient cosmosClient,
            CosmosDatabaseConfig cosmosConfig)
        {
            _cosmosClient = cosmosClient;
            _cosmosConfig = cosmosConfig;
        }

        public async Task<RequestData> GetDataAvailableUuidsAsync(string recipient)
        {
            if (recipient is null)
                throw new ArgumentNullException(nameof(recipient));

            const string queryString =
                "SELECT * FROM c WHERE c.recipient = @recipient ORDER BY c._ts ASC OFFSET 0 LIMIT 1";
            var parameters = new List<KeyValuePair<string, string>> { new ("recipient", recipient) };

            var documents = await GetDocumentsAsync(queryString, parameters).ConfigureAwait(false);
            var document = documents.FirstOrDefault();

            return document is not null
                ? new RequestData { Origin = document.origin, Uuids = new List<string> { document.uuid! } }
                : new RequestData();
        }

        public async Task<bool> SaveDocumentAsync(DataAvailable document)
        {
            if (document is null)
                throw new ArgumentNullException(nameof(document));

            var container = GetContainer(ContainerName);

            var cosmosDocument = new CosmosDataAvailable
            {
                uuid = document.uuid,
                recipient = document.recipient,
                messageType = document.messageType,
                origin = document.origin,
                supportsBundling = document.supportsBundling,
                relativeWeight = document.relativeWeight,
                priority = document.priority,
            };

            var response = await container.CreateItemAsync(cosmosDocument).ConfigureAwait(false);
            if (response.StatusCode != HttpStatusCode.Created)
                throw new InvalidOperationException("Could not create document in cosmos");

            return true;
        }

        private async Task<IEnumerable<DataAvailable>> GetDocumentsAsync(string query, List<KeyValuePair<string, string>> parameters)
        {
            if (query is null)
                throw new ArgumentNullException(nameof(query));
            if (parameters is null)
                throw new ArgumentNullException(nameof(parameters));

            var documentQuery = new QueryDefinition(query);
            parameters.ForEach(item => documentQuery.WithParameter($"@{item.Key}", item.Value));

            var container = GetContainer(ContainerName);

            using (FeedIterator<CosmosDataAvailable> feedIterator =
                container.GetItemQueryIterator<CosmosDataAvailable>(documentQuery))
            {
                var documentsFromCosmos = await feedIterator.ReadNextAsync().ConfigureAwait(false);
                var documents = documentsFromCosmos
                    .Select(document => new DataAvailable(
                        document.uuid,
                        document.recipient,
                        document.messageType,
                        document.origin,
                        document.supportsBundling,
                        document.relativeWeight,
                        document.priority));

                return documents.ToList();
            }
        }

        private Container GetContainer(string containerName)
        {
            var container = _cosmosClient.GetContainer(
                _cosmosConfig.DatabaseId,
                containerName);
            return container;
        }
    }
}
