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
using Energinet.DataHub.PostOffice.Domain.Repositories;
using Energinet.DataHub.PostOffice.Infrastructure.Documents;
using Microsoft.Azure.Cosmos;

namespace Energinet.DataHub.PostOffice.Infrastructure.Repositories
{
    public class MessageReplyRepository : IMessageReplyRepository
    {
        private const string ContainerName = "messagereplies";
        private readonly CosmosClient _cosmosClient;
        private readonly CosmosDatabaseConfig _cosmosConfig;

        public MessageReplyRepository(CosmosClient cosmosClient, CosmosDatabaseConfig cosmosConfig)
        {
            _cosmosClient = cosmosClient;
            _cosmosConfig = cosmosConfig;
        }

        public async Task<string?> GetMessageReplyAsync(string messageKey)
        {
            if (messageKey is null) throw new ArgumentNullException(nameof(messageKey));

            const string queryString =
                "SELECT * FROM c WHERE c.id = @recipient ORDER BY c._ts ASC OFFSET 0 LIMIT 1";
            var parameters = new List<KeyValuePair<string, string>> { new("recipient", messageKey) };

            var documents = await GetDocumentsAsync(queryString, parameters).ConfigureAwait(false);
            var document = documents.FirstOrDefault();

            return document?.ContentPath;
        }

        public async Task<bool> SaveMessageReplyAsync(string messageKey, Uri contentUri)
        {
            if (messageKey is null) throw new ArgumentNullException(nameof(messageKey));
            if (contentUri is null) throw new ArgumentNullException(nameof(contentUri));
            var container = GetContainer(ContainerName);

            var messageDocument = new MessageReplyDocument()
            {
               Id = messageKey,
               ContentPath = contentUri.AbsoluteUri
            };

            var response = await container.CreateItemAsync(messageDocument).ConfigureAwait(false);
            if (response.StatusCode != HttpStatusCode.Created)
                throw new InvalidOperationException("Could not create document in cosmos");

            return true;
        }

        private async Task<IEnumerable<MessageReplyDocument>> GetDocumentsAsync(string query, List<KeyValuePair<string, string>> parameters)
        {
            if (query is null)
                throw new ArgumentNullException(nameof(query));
            if (parameters is null)
                throw new ArgumentNullException(nameof(parameters));

            var documentQuery = new QueryDefinition(query);
            parameters.ForEach(item => documentQuery.WithParameter($"@{item.Key}", item.Value));

            var container = GetContainer(ContainerName);

            using (FeedIterator<MessageReplyDocument> feedIterator =
                container.GetItemQueryIterator<MessageReplyDocument>(documentQuery))
            {
                var documentsFromCosmos = await feedIterator.ReadNextAsync().ConfigureAwait(false);
                var documents = documentsFromCosmos
                    .Select(document => new MessageReplyDocument()
                    {
                        Id = document.Id,
                        ContentPath = document.ContentPath
                    });

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
