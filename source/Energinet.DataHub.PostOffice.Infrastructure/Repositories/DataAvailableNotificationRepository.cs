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
using Energinet.DataHub.MessageHub.Model.Exceptions;
using Energinet.DataHub.PostOffice.Domain.Model;
using Energinet.DataHub.PostOffice.Domain.Repositories;
using Energinet.DataHub.PostOffice.Infrastructure.Common;
using Energinet.DataHub.PostOffice.Infrastructure.Documents;
using Energinet.DataHub.PostOffice.Infrastructure.Repositories.Containers;
using Microsoft.Azure.Cosmos;

namespace Energinet.DataHub.PostOffice.Infrastructure.Repositories
{
    public class DataAvailableNotificationRepository : IDataAvailableNotificationRepository
    {
        private readonly IDataAvailableNotificationRepositoryContainer _repositoryContainer;

        public DataAvailableNotificationRepository(IDataAvailableNotificationRepositoryContainer repositoryContainer)
        {
            _repositoryContainer = repositoryContainer;
        }

        public async Task SaveAsync(DataAvailableNotification dataAvailableNotification)
        {
            if (dataAvailableNotification is null)
                throw new ArgumentNullException(nameof(dataAvailableNotification));

            var cosmosDocument = new CosmosDataAvailable
            {
                Id = dataAvailableNotification.NotificationId.ToString(),
                Recipient = dataAvailableNotification.Recipient.Gln.Value,
                ContentType = dataAvailableNotification.ContentType.Value,
                Origin = dataAvailableNotification.Origin.ToString(),
                SupportsBundling = dataAvailableNotification.SupportsBundling.Value,
                RelativeWeight = dataAvailableNotification.Weight.Value,
                Acknowledge = false,
                PartitionKey = dataAvailableNotification.Recipient.Gln.Value + dataAvailableNotification.Origin + dataAvailableNotification.ContentType.Value
            };

            try
            {
                await _repositoryContainer.Container.CreateItemAsync(cosmosDocument).ConfigureAwait(false);
            }
            catch (CosmosException e) when (e.StatusCode == HttpStatusCode.Conflict)
            {
                using var feedIterator = _repositoryContainer.Container.GetItemQueryIterator<CosmosDataAvailable>(
                    new QueryDefinition("select * from dataavailable da where da.id = @id")
                        .WithParameter("@id", cosmosDocument.Id),
                    null,
                    new QueryRequestOptions
                    {
                        PartitionKey = new PartitionKey(cosmosDocument.PartitionKey)
                    });

                while (feedIterator.HasMoreResults)
                {
                    foreach (var item in await feedIterator.ReadNextAsync().ConfigureAwait(false))
                    {
                        if (cosmosDocument with { Timestamp = item.Timestamp } == item)
                        {
                            return;
                        }
                    }
                }

                throw new MessageHubStorageException("A data available notification already exists with the given ID");
            }
        }

        public async Task SaveAsync(IEnumerable<DataAvailableNotification> dataAvailableNotifications)
        {
            if (dataAvailableNotifications is null)
                throw new ArgumentNullException(nameof(dataAvailableNotifications));

            var concurrentTasks = new List<Task>();

            foreach (var dataAvailableNotification in dataAvailableNotifications)
            {
                var item = new CosmosDataAvailable
                {
                    Id = dataAvailableNotification.NotificationId.ToString(),
                    Recipient = dataAvailableNotification.Recipient.Gln.Value,
                    ContentType = dataAvailableNotification.ContentType.Value,
                    Origin = dataAvailableNotification.Origin.ToString(),
                    SupportsBundling = dataAvailableNotification.SupportsBundling.Value,
                    RelativeWeight = dataAvailableNotification.Weight.Value,
                    Acknowledge = false,
                    PartitionKey = dataAvailableNotification.Recipient.Gln.Value + dataAvailableNotification.Origin + dataAvailableNotification.ContentType.Value
                };

                concurrentTasks.Add(_repositoryContainer.Container.CreateItemAsync(item));
            }

            await Task.WhenAll(concurrentTasks).ConfigureAwait(false);
        }

        public Task<DataAvailableNotification?> GetNextUnacknowledgedAsync(MarketOperator recipient, params DomainOrigin[] domains)
        {
            if (recipient is null)
                throw new ArgumentNullException(nameof(recipient));

            var asLinq = _repositoryContainer
                .Container
                .GetItemLinqQueryable<CosmosDataAvailable>();

            IQueryable<CosmosDataAvailable> domainFiltered = asLinq;

            if (domains is { Length: > 0 })
            {
                var selectedDomains = domains.Select(x => x.ToString());
                domainFiltered = asLinq.Where(x => selectedDomains.Contains(x.Origin));
            }

            var query =
                from dataAvailable in domainFiltered
                where
                    dataAvailable.Recipient == recipient.Gln.Value &&
                    !dataAvailable.Acknowledge
                orderby dataAvailable.Timestamp
                select dataAvailable;

            return ExecuteQueryAsync(query).FirstOrDefaultAsync();
        }

        public async Task<IEnumerable<DataAvailableNotification>> GetNextUnacknowledgedAsync(
            MarketOperator recipient,
            DomainOrigin domainOrigin,
            ContentType contentType,
            Weight maxWeight)
        {
            if (recipient is null)
                throw new ArgumentNullException(nameof(recipient));

            if (contentType is null)
                throw new ArgumentNullException(nameof(contentType));

            var asLinq = _repositoryContainer
                .Container
                .GetItemLinqQueryable<CosmosDataAvailable>();

            var query =
                from dataAvailable in asLinq
                where
                    dataAvailable.Recipient == recipient.Gln.Value &&
                    dataAvailable.ContentType == contentType.Value &&
                    dataAvailable.Origin == domainOrigin.ToString() &&
                    !dataAvailable.Acknowledge
                orderby dataAvailable.Timestamp
                select dataAvailable;

            var currentWeight = new Weight(0);
            var allUnacknowledged = new List<DataAvailableNotification>();

            await foreach (var item in ExecuteBatchAsync(query).ConfigureAwait(false))
            {
                if (allUnacknowledged.Count == 0 || (currentWeight + item.Weight <= maxWeight && item.SupportsBundling.Value))
                {
                    // TODO: Bundles are limited to 800 guids * 296 bytes ~ 231,25 KB until
                    // issue with ServiceBus message size is resolved.
                    if (allUnacknowledged.Count == 800)
                        break;

                    currentWeight += item.Weight;
                    allUnacknowledged.Add(item);
                }
                else
                {
                    break;
                }
            }

            return allUnacknowledged;
        }

        public async Task AcknowledgeAsync(MarketOperator recipient, IEnumerable<Uuid> dataAvailableNotificationUuids)
        {
            if (recipient is null)
                throw new ArgumentNullException(nameof(recipient));

            if (dataAvailableNotificationUuids is null)
                throw new ArgumentNullException(nameof(dataAvailableNotificationUuids));

            var stringIds = dataAvailableNotificationUuids
                .Select(x => x.ToString());

            var container = _repositoryContainer.Container;
            var asLinq = container
                .GetItemLinqQueryable<CosmosDataAvailable>();

            var query =
                from dataAvailable in asLinq
                where dataAvailable.Recipient == recipient.Gln.Value && stringIds.Contains(dataAvailable.Id)
                select dataAvailable;

            TransactionalBatch? batch = null;

            var batchSize = 0;

            await foreach (var document in query.AsCosmosIteratorAsync().ConfigureAwait(false))
            {
                var updatedDocument = document with { Acknowledge = true };

                batch ??= container.CreateTransactionalBatch(new PartitionKey(updatedDocument.PartitionKey));
                batch.ReplaceItem(updatedDocument.Id, updatedDocument);

                batchSize++;

                // Microsoft decided on an arbitrary batch limit of 100.
                if (batchSize == 100)
                {
                    using var innerResult = await batch.ExecuteAsync().ConfigureAwait(false);

                    // As written in docs, _this_ API does not throw exceptions and has to be checked.
                    if (!innerResult.IsSuccessStatusCode)
                    {
                        throw new InvalidOperationException(innerResult.ErrorMessage);
                    }

                    batch = null;
                    batchSize = 0;
                }
            }

            if (batch != null)
            {
                using var outerResult = await batch.ExecuteAsync().ConfigureAwait(false);

                // As written in docs, _this_ API does not throw exceptions and has to be checked.
                if (!outerResult.IsSuccessStatusCode)
                {
                    throw new InvalidOperationException(outerResult.ErrorMessage);
                }
            }
        }

        private static async IAsyncEnumerable<DataAvailableNotification> ExecuteBatchAsync(IQueryable<CosmosDataAvailable> query)
        {
            const int batchSize = 10000;

            var batchStart = 0;
            bool canHaveMoreItems;

            do
            {
                var nextBatchQuery = query.Skip(batchStart).Take(batchSize);
                var returnedItems = 0;

                await foreach (var item in ExecuteQueryAsync(nextBatchQuery).ConfigureAwait(false))
                {
                    yield return item;
                    returnedItems++;
                }

                batchStart += batchSize;
                canHaveMoreItems = returnedItems == batchSize;
            }
            while (canHaveMoreItems);
        }

        private static async IAsyncEnumerable<DataAvailableNotification> ExecuteQueryAsync(IQueryable<CosmosDataAvailable> query)
        {
            await foreach (var document in query.AsCosmosIteratorAsync().ConfigureAwait(false))
            {
                yield return new DataAvailableNotification(
                    new Uuid(document.Id),
                    new MarketOperator(new GlobalLocationNumber(document.Recipient)),
                    new ContentType(document.ContentType),
                    Enum.Parse<DomainOrigin>(document.Origin, true),
                    new SupportsBundling(document.SupportsBundling),
                    new Weight(document.RelativeWeight));
            }
        }
    }
}
