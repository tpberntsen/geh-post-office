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
using Energinet.DataHub.PostOffice.Domain.Model;
using Energinet.DataHub.PostOffice.Domain.Repositories;
using Energinet.DataHub.PostOffice.Infrastructure.Common;
using Energinet.DataHub.PostOffice.Infrastructure.Documents;
using Energinet.DataHub.PostOffice.Infrastructure.Repositories.Containers;
using FluentValidation;
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
                await foreach (var x in FindAlreadyExistingDocumentsOnIdAsync(
                    _repositoryContainer.Container,
                    new Dictionary<string, (DataAvailableNotification, CosmosDataAvailable)> { { cosmosDocument.Id + cosmosDocument.PartitionKey, (dataAvailableNotification, cosmosDocument) } }))
                {
                    if (!x.IsIdempotent)
                        throw new ValidationException("A data available notification already exists with the given ID");
                }
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

        public async Task WriteToArchiveAsync(IEnumerable<Uuid> dataAvailableNotifications, string partitionKey)
        {
            if (partitionKey is null)
                throw new ArgumentNullException(nameof(partitionKey));

            var documentPartitionKey = new PartitionKey(partitionKey);
            var documentsToRead = dataAvailableNotifications.Select(e => (e.ToString(), documentPartitionKey)).ToList();

            var documentsToArchive = await _repositoryContainer
                .Container
                .ReadManyItemsAsync<CosmosDataAvailable>(documentsToRead).ConfigureAwait(false);

            if (documentsToArchive.StatusCode != HttpStatusCode.OK)
            {
                throw new CosmosException("ReadManyItemsAsync failed", documentsToArchive.StatusCode, -1, documentsToArchive.ActivityId, documentsToArchive.RequestCharge);
            }

            var archiveWriteTasks = documentsToArchive.Select(ArchiveDocumentAsync);
            await Task.WhenAll(archiveWriteTasks).ConfigureAwait(false);
        }

        public Task DeleteAsync(IEnumerable<Uuid> dataAvailableNotifications, string partitionKey)
        {
            var documentPartitionKey = new PartitionKey(partitionKey);
            var deleteTasks = dataAvailableNotifications
                .Select(dataAvailableNotification =>
                    _repositoryContainer.Container.DeleteItemStreamAsync(dataAvailableNotification.ToString(), documentPartitionKey)).ToList();

            return Task.WhenAll(deleteTasks);
        }

        public IAsyncEnumerable<(DataAvailableNotification Command, bool IsIdempotent)> ValidateAgainstArchiveAsync(IEnumerable<DataAvailableNotification> dataAvailableNotifications)
        {
            return FindAlreadyExistingDocumentsOnIdAsync(_repositoryContainer.ArchiveContainer, dataAvailableNotifications.Select(notification =>
                (notification,
                    doc: new CosmosDataAvailable
                    {
                        Id = notification.NotificationId.ToString(),
                        Recipient = notification.Recipient.Gln.Value,
                        ContentType = notification.ContentType.Value,
                        Origin = notification.Origin.ToString(),
                        SupportsBundling = notification.SupportsBundling.Value,
                        RelativeWeight = notification.Weight.Value,
                        Acknowledge = false,
                        PartitionKey = notification.Recipient.Gln.Value + notification.Origin + notification.ContentType.Value
                    }))
                .GroupBy(x => x.doc.Id + x.doc.PartitionKey).Select(x => x.First())
                .ToDictionary(x => x.doc.Id + x.doc.PartitionKey, x => (x.notification, x.doc)));
        }

        private static async IAsyncEnumerable<(DataAvailableNotification Command, bool IsIdempotent)> FindAlreadyExistingDocumentsOnIdAsync(
            Container container,
            IDictionary<string, (DataAvailableNotification Notification, CosmosDataAvailable Document)> cosmosDataAvailable)
        {
            const int batchSize = 1000;
            var taken = 0;

            var allIds = cosmosDataAvailable.Values.Select(x => x.Document.Id).ToArray();
            string[] ids;

            do
            {
                ids = allIds.Skip(taken).Take(batchSize).ToArray();
                var localIds = ids;
                var query =
                    from da in container.GetItemLinqQueryable<CosmosDataAvailable>()
                    where localIds.Contains(da.Id)
                    select da;

                await foreach (var doc in query.AsCosmosIteratorAsync())
                {
                    var key = doc.Id + doc.PartitionKey;
                    if (cosmosDataAvailable.ContainsKey(key))
                        yield return (cosmosDataAvailable[key].Notification, cosmosDataAvailable[key].Document with { Timestamp = doc.Timestamp } == doc);
                }

                taken += ids.Length;
            }
            while (ids.Any());
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

        private Task ArchiveDocumentAsync(CosmosDataAvailable documentToWrite)
        {
            return _repositoryContainer
                .ArchiveContainer.UpsertItemAsync(documentToWrite, new PartitionKey(documentToWrite.PartitionKey));
        }
    }
}
