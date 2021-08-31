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
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Energinet.DataHub.PostOffice.Domain.Model;
using Energinet.DataHub.PostOffice.Domain.Repositories;
using Microsoft.Azure.Cosmos;

namespace Energinet.DataHub.PostOffice.Infrastructure.Repositories
{
    public class DataAvailableNotificationRepository : IDataAvailableNotificationRepository
    {
        private const string ContainerName = "dataavailable";
        private readonly Container _container;

        public DataAvailableNotificationRepository(
            [NotNull] CosmosClient cosmosClient,
            [NotNull] CosmosDatabaseConfig cosmosConfig)
        {
            _container = cosmosClient.GetContainer(cosmosConfig.DatabaseId, ContainerName);
        }

        public async Task SaveAsync(DataAvailableNotification dataAvailableNotification)
        {
            if (dataAvailableNotification is null)
                throw new ArgumentNullException(nameof(dataAvailableNotification));

            var cosmosDocument = new CosmosDataAvailable
            {
                Uuid = dataAvailableNotification.NotificationId.Value,
                Recipient = dataAvailableNotification.Recipient.Value,
                MessageType = dataAvailableNotification.ContentType.Type,
                Origin = dataAvailableNotification.Origin.ToString(),
                RelativeWeight = dataAvailableNotification.Weight.Value,
                Priority = 1M,
            };

            var response = await _container.CreateItemAsync(cosmosDocument).ConfigureAwait(false);
            if (response.StatusCode != HttpStatusCode.Created)
                throw new InvalidOperationException("Could not create document in cosmos");
        }

        public async Task<IEnumerable<DataAvailableNotification>> GetNextUnacknowledgedAsync(MarketOperator recipient, ContentType contentType)
        {
            if (recipient is null)
                throw new ArgumentNullException(nameof(recipient));
            if (contentType is null)
                throw new ArgumentNullException(nameof(contentType));

            const string queryString = "SELECT * FROM c WHERE c.recipient = @recipient AND c.acknowledge = false AND c.messageType = @messageType ORDER BY c._ts ASC OFFSET 0 LIMIT 1";
            var parameters = new List<KeyValuePair<string, string>> { new("recipient", recipient.Value), new("messageType", contentType.Type) };

            var documents = await GetDocumentsAsync(queryString, parameters).ConfigureAwait(false);
            return documents;
        }

        public async Task<DataAvailableNotification?> GetNextUnacknowledgedAsync(MarketOperator recipient)
        {
            if (recipient is null)
                throw new ArgumentNullException(nameof(recipient));

            const string queryString = "SELECT * FROM c WHERE c.recipient = @recipient AND c.acknowledge = false ORDER BY c._ts ASC OFFSET 0 LIMIT 1";
            var parameters = new List<KeyValuePair<string, string>> { new("recipient", recipient.Value) };

            var documents = await GetDocumentsAsync(queryString, parameters).ConfigureAwait(false);
            var document = documents.FirstOrDefault();

            return document;
        }

        public async Task AcknowledgeAsync(IEnumerable<Uuid> dataAvailableNotificationUuids)
        {
            if (dataAvailableNotificationUuids is null)
                throw new ArgumentNullException(nameof(dataAvailableNotificationUuids));

            foreach (var uuid in dataAvailableNotificationUuids)
            {
                var documentToUpdateResponse = _container.GetItemLinqQueryable<CosmosDataAvailable>(true)
                    .Where(document => document.Uuid == uuid.Value)
                    .AsEnumerable()
                    .FirstOrDefault();

                if (documentToUpdateResponse is null) // Or Throw
                    continue;

                documentToUpdateResponse.Acknowledge = true;
                await _container.ReplaceItemAsync(documentToUpdateResponse, documentToUpdateResponse.Id, new PartitionKey(documentToUpdateResponse.Recipient)).ConfigureAwait(false);
            }
        }

        private async Task<IEnumerable<DataAvailableNotification>> GetDocumentsAsync(string query, List<KeyValuePair<string, string>> parameters)
        {
            if (query is null)
                throw new ArgumentNullException(nameof(query));
            if (parameters is null)
                throw new ArgumentNullException(nameof(parameters));

            var documentQuery = new QueryDefinition(query);
            parameters.ForEach(item => documentQuery.WithParameter($"@{item.Key}", item.Value));

            var documentsResult = new List<DataAvailableNotification>();

            using (FeedIterator<CosmosDataAvailable> feedIterator = _container.GetItemQueryIterator<CosmosDataAvailable>(documentQuery))
            {
                while (feedIterator.HasMoreResults)
                {
                    var documentsFromCosmos = await feedIterator.ReadNextAsync().ConfigureAwait(false);
                    var documents = documentsFromCosmos
                        .Select(document => new DataAvailableNotification(
                            new Uuid(document.Uuid),
                            new MarketOperator(document.Recipient),
                            new ContentType(document.RelativeWeight, document.MessageType),
                            Enum.Parse<SubDomain>(document.Origin, true),
                            new Weight(document.RelativeWeight)));

                    documentsResult.AddRange(documents);
                }
            }

            return documentsResult;
        }
    }
}
