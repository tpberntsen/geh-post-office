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
using Energinet.DataHub.PostOffice.Application;
using Energinet.DataHub.PostOffice.Domain;
using Microsoft.Azure.Cosmos;

namespace Energinet.DataHub.PostOffice.Infrastructure
{
    public class CosmosDocumentStore : IDocumentStore
    {
        private readonly CosmosClient _cosmosClient;
        private readonly CosmosConfig _cosmosConfig;

        public CosmosDocumentStore(
            CosmosClient cosmosClient,
            CosmosConfig cosmosConfig)
        {
            _cosmosClient = cosmosClient;
            _cosmosConfig = cosmosConfig;
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

        public async Task<bool> DeleteDocumentsAsync(string bundleIdentifier, string recipient)
        {
            foreach (var containerTypeIdentifier in _cosmosConfig.TypeToContainerIdMap.Keys)
            {
                var container = GetContainer(containerTypeIdentifier);
                var bundle = await GetBundleAsync(container, recipient).ConfigureAwait(false);
                if (bundle.FirstOrDefault()?.Bundle == bundleIdentifier)
                {
                    var itemRequestOptions = new ItemRequestOptions { EnableContentResponseOnWrite = false, };

                    var concurrentDeleteTasks = new List<Task>();
                    foreach (var document in bundle)
                    {
                        concurrentDeleteTasks.Add(container.DeleteItemAsync<CosmosDocument>(document.Id, new PartitionKey(recipient), itemRequestOptions));
                    }

                    await Task.WhenAll(concurrentDeleteTasks).ConfigureAwait(false);

                    return true; // We deleted the bundled documents
                }
            }

            return false; // We didn't find anything to delete
        }

        public async Task<IList<Document>> GetDocumentsAsync(DocumentQuery documentQuery)
        {
            if (documentQuery == null) throw new ArgumentNullException(nameof(documentQuery));

            const string QueryString = @"
                SELECT TOP @pageSize *
                FROM Documents d
                WHERE d.recipient = @recipient
                ORDER BY d.effectuationDate";

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

        public async Task<IList<Document>> GetDocumentBundleAsync(DocumentQuery documentQuery)
        {
            if (documentQuery == null) throw new ArgumentNullException(nameof(documentQuery));

            var container = GetContainer(documentQuery.Type);

            var existingBundle = await GetBundleAsync(container, documentQuery.Recipient).ConfigureAwait(false);
            if (existingBundle.Any())
            {
                return existingBundle
                    .Select(CosmosDocumentMapper.Map)
                    .ToList();
            }

            var bundle = await CreateBundleAsync(container, documentQuery).ConfigureAwait(false);
            return bundle
                .Select(CosmosDocumentMapper.Map)
                .ToList();
        }

        private static async Task<IList<CosmosDocument>> GetBundleAsync(Container container, string recipient)
        {
            var queryDefinition = new QueryDefinition(@"SELECT * FROM Documents d WHERE d.recipient = @recipient AND d.bundle != null")
                .WithParameter("@recipient", recipient);

            var documents = new List<CosmosDocument>();
            var query = container.GetItemQueryIterator<CosmosDocument>(queryDefinition);
            foreach (var document in await query.ReadNextAsync().ConfigureAwait(false))
            {
                documents.Add(document);
            }

            return documents;
        }

        private static async Task<IList<CosmosDocument>> CreateBundleAsync(Container container, DocumentQuery documentQuery)
        {
            var typeQueryDefinition = new QueryDefinition(@"SELECT top 1 * FROM Documents d WHERE d.recipient = @recipient ORDER BY d.effectuationDate")
                .WithParameter("@recipient", documentQuery.Recipient);
            var typeQuery = container.GetItemQueryIterator<CosmosDocument>(typeQueryDefinition);

            // No items to bundle
            if (!typeQuery.HasMoreResults)
            {
                return new List<CosmosDocument>();
            }

            var typeDocument = await typeQuery.ReadNextAsync().ConfigureAwait(false);
            var type = typeDocument.Resource?.SingleOrDefault()?.Type;
            if (type == null)
            {
                // TODO: Log error?
                return new List<CosmosDocument>();
            }

            var queryDefinition = new QueryDefinition(@"SELECT top @pageSize * FROM Documents d WHERE d.recipient = @recipient AND d.type = @type ORDER BY d.effectuationDate")
                .WithParameter("@recipient", documentQuery.Recipient)
                .WithParameter("@pageSize", documentQuery.PageSize)
                .WithParameter("@type", type);
            var documents = new List<CosmosDocument>();
            var query = container.GetItemQueryIterator<CosmosDocument>(queryDefinition);
            foreach (var document in await query.ReadNextAsync().ConfigureAwait(false))
            {
                documents.Add(document);
            }

            var bundle = Guid.NewGuid().ToString();
            var itemRequestOptions = new ItemRequestOptions { EnableContentResponseOnWrite = false, };

            var concurrentUpdateTasks = new List<Task>();
            foreach (var document in documents)
            {
                document.Bundle = bundle;
                concurrentUpdateTasks.Add(container.UpsertItemAsync(document, new PartitionKey(documentQuery.Recipient), itemRequestOptions));
            }

            await Task.WhenAll(concurrentUpdateTasks).ConfigureAwait(false);

            return documents;
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
