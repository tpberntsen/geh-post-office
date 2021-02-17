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
using System.Net;
using System.Threading.Tasks;
using Energinet.DataHub.PostOffice.Application;
using Energinet.DataHub.PostOffice.Domain;
using Microsoft.Azure.Cosmos;

namespace Energinet.DataHub.PostOffice.Infrastructure
{
    public class CosmosDocumentStore : IDocumentStore
    {
        private const string QueryString = @"
            SELECT TOP @pageSize *
            FROM Documents d
            WHERE d.recipient = @recipient
            ORDER BY d.effectuationDate";

        private readonly CosmosClient _cosmosClient;
        private readonly CosmosConfig _cosmosConfig;

        public CosmosDocumentStore(
            CosmosClient cosmosClient,
            CosmosConfig cosmosConfig)
        {
            _cosmosClient = cosmosClient;
            _cosmosConfig = cosmosConfig;
        }

        public async Task<IList<Document>> GetDocumentsAsync(DocumentQuery documentQuery)
        {
            if (documentQuery == null) throw new ArgumentNullException(nameof(documentQuery));

            var container = GetContainer(documentQuery.Type);

            // Querying with an equality filter on the partition key will create a partitioned documentQuery, per:
            // https://docs.microsoft.com/en-us/azure/cosmos-db/how-to-documentQuery-container#in-partition-documentQuery
            // TODO: Change when actual names are available, ie. recipient_MriD?
            var queryDefinition = new QueryDefinition(QueryString)
                .WithParameter("@recipient", documentQuery.Recipient)
                .WithParameter("@pageSize", documentQuery.PageSize);

            var documents = new List<Document>();
            var query = container.GetItemQueryIterator<CosmosDocument>(queryDefinition);
            foreach (var document in await query.ReadNextAsync().ConfigureAwait(false))
            {
                documents.Add(CosmosDocumentMapper.Map(document));
            }

            return documents;
        }

        public async Task SaveDocumentAsync(Document document)
        {
            if (document == null) throw new ArgumentNullException(nameof(document));

            // TODO: add error handling and fix type nullability?
            var container = GetContainer(document.Type!);

            var cosmosDocument = CosmosDocumentMapper.Map(document);

            var response = await container.CreateItemAsync(cosmosDocument).ConfigureAwait(false);
            if (response.StatusCode != HttpStatusCode.Created)
            {
                throw new InvalidOperationException("Could not create document in cosmos");
            }
        }

        private Container GetContainer(string type)
        {
            var container = _cosmosClient.GetContainer(
                _cosmosConfig.DatabaseId,
                _cosmosConfig.TypeToContainerIdMap[type]);
            return container;
        }
    }
}
