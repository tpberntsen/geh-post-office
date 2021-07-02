﻿// Copyright 2020 Energinet DataHub A/S
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
using Energinet.DataHub.PostOffice.Contracts;
using Energinet.DataHub.PostOffice.Domain;
using Microsoft.Azure.Cosmos;

namespace Energinet.DataHub.PostOffice.Infrastructure
{
    public class CosmosDataAvailableStore : IDocumentStore<Contracts.DataAvailable>
    {
        private const string ContainerName = "dataavailable";

        private readonly CosmosClient _cosmosClient;
        private readonly CosmosDatabaseConfig _cosmosConfig;

        public CosmosDataAvailableStore(
            CosmosClient cosmosClient,
            CosmosDatabaseConfig cosmosConfig)
        {
            _cosmosClient = cosmosClient;
            _cosmosConfig = cosmosConfig;
        }

        public async Task<IList<Contracts.DataAvailable>> GetDocumentsAsync(DocumentQuery documentQuery)
        {
            if (documentQuery == null) throw new ArgumentNullException(nameof(documentQuery));

            const string QueryString = @"
                SELECT *
                FROM Documents d
                WHERE d.recipient = @recipient";

            var container = GetContainer(documentQuery.ContainerName);

            // Querying with an equality filter on the partition key will create a partitioned documentQuery, per:
            // https://docs.microsoft.com/en-us/azure/cosmos-db/how-to-documentQuery-container#in-partition-documentQuery
            // TODO: Change when actual names are available, ie. recipient_MriD?
            var queryDefinition = new QueryDefinition(QueryString)
                .WithParameter("@recipient", documentQuery.Recipient);

            var documents = new List<Contracts.DataAvailable>();
            var query = container.GetItemQueryIterator<CosmosDataAvailable>(queryDefinition);
            foreach (var document in await query.ReadNextAsync().ConfigureAwait(false))
            {
                documents.Add(new DataAvailable() { UUID = document.uuid });
            }

            return documents;
        }

        public Task<IList<Contracts.DataAvailable>> GetDocumentBundleAsync(DocumentQuery documentQuery)
        {
            throw new NotImplementedException();
        }

        public Task SaveDocumentAsync(Contracts.DataAvailable document, string containerName)
        {
            throw new NotImplementedException();
        }

        public Task<bool> DeleteDocumentsAsync(DequeueCommand dequeueCommand)
        {
            throw new NotImplementedException();
        }

        public async Task SaveDocumentAsync(Contracts.DataAvailable document)
        {
            if (document == null) throw new ArgumentNullException(nameof(document));

            var container = GetContainer(ContainerName);

            var cosmosDocument = new CosmosDataAvailable
            {
                uuid = document.UUID,
                recipient = document.Recipient,
                messageType = document.MessageType,
                origin = document.Origin,
                supportsBundling = document.SupportsBundling,
                relativeWeight = document.RelativeWeight,
            };

            var response = await container.CreateItemAsync(cosmosDocument).ConfigureAwait(false);
            if (response.StatusCode != HttpStatusCode.Created)
            {
                throw new InvalidOperationException("Could not create document in cosmos");
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
