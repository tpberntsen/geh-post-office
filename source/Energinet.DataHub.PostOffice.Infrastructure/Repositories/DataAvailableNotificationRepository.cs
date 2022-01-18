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
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Energinet.DataHub.PostOffice.Domain.Model;
using Energinet.DataHub.PostOffice.Domain.Repositories;
using Energinet.DataHub.PostOffice.Infrastructure.Common;
using Energinet.DataHub.PostOffice.Infrastructure.Documents;
using Energinet.DataHub.PostOffice.Infrastructure.Repositories.Containers;
using Energinet.DataHub.PostOffice.Utilities;
using Microsoft.Azure.Cosmos;

namespace Energinet.DataHub.PostOffice.Infrastructure.Repositories
{
    public class DataAvailableNotificationRepository : IDataAvailableNotificationRepository
    {
        private readonly IBundleRepositoryContainer _bundleRepositoryContainer;
        private readonly IDataAvailableNotificationRepositoryContainer _repositoryContainer;

        public DataAvailableNotificationRepository(
            IBundleRepositoryContainer bundleRepositoryContainer,
            IDataAvailableNotificationRepositoryContainer repositoryContainer)
        {
            _bundleRepositoryContainer = bundleRepositoryContainer;
            _repositoryContainer = repositoryContainer;
        }

        public async Task SaveAsync(DataAvailableNotification dataAvailableNotification)
        {
            Guard.ThrowIfNull(dataAvailableNotification, nameof(dataAvailableNotification));

            var uniqueId = new CosmosUniqueId
            {
                Id = dataAvailableNotification.NotificationId.ToString(),
                PartitionKey = (dataAvailableNotification.NotificationId.AsGuid().GetHashCode() % 10).ToString(CultureInfo.InvariantCulture),
                Content = Base64Content(dataAvailableNotification)
            };

            try
            {
                await _repositoryContainer.Container.CreateItemAsync(uniqueId).ConfigureAwait(false);
            }
            catch (CosmosException e) when (e.StatusCode == HttpStatusCode.Conflict)
            {
                var query =
                    from uniqueIdQuery in _repositoryContainer.Container.GetItemLinqQueryable<CosmosUniqueId>()
                    where
                        uniqueIdQuery.Id == uniqueId.Id &&
                        uniqueIdQuery.PartitionKey == uniqueId.PartitionKey
                    select uniqueIdQuery;

                await foreach (var existingUniqueId in query.AsCosmosIteratorAsync<CosmosUniqueId>())
                {
                    if (existingUniqueId.Content == uniqueId.Content)
                    {
                        return;
                    }

                    throw new ValidationException("ID already in use", e);
                }
            }

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

            await _repositoryContainer.Container.CreateItemAsync(cosmosDocument).ConfigureAwait(false);

            static string Base64Content(DataAvailableNotification dataAvailableNotification)
            {
                using var ms = new MemoryStream();
                ms.Write(Encoding.UTF8.GetBytes(dataAvailableNotification.ContentType.Value));
                ms.Write(BitConverter.GetBytes((int)dataAvailableNotification.Origin));
                ms.Write(Encoding.UTF8.GetBytes(dataAvailableNotification.Recipient.Gln.Value));
                ms.Write(BitConverter.GetBytes(dataAvailableNotification.SupportsBundling.Value));
                ms.Write(BitConverter.GetBytes(dataAvailableNotification.Weight.Value));
                return Convert.ToBase64String(ms.ToArray());
            }
        }

        public Task<DataAvailableNotification?> GetNextUnacknowledgedAsync(MarketOperator recipient, params DomainOrigin[] domains)
        {
            Guard.ThrowIfNull(recipient, nameof(recipient));

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
            Guard.ThrowIfNull(recipient, nameof(recipient));
            Guard.ThrowIfNull(contentType, nameof(contentType));

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
            Guard.ThrowIfNull(recipient, nameof(recipient));
            Guard.ThrowIfNull(dataAvailableNotificationUuids, nameof(dataAvailableNotificationUuids));

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

        public async Task AcknowledgeAsync(Bundle bundle)
        {
            Guard.ThrowIfNull(bundle, nameof(bundle));

            var asLinq = _bundleRepositoryContainer
                .Container
                .GetItemLinqQueryable<CosmosBundleDocument2>();

            var recipient = bundle.Recipient.Gln.Value;
            var bundleId = bundle.BundleId.ToString();

            var query =
                from cosmosBundle in asLinq
                where cosmosBundle.Recipient == recipient &&
                      cosmosBundle.Id == bundleId
                select cosmosBundle;

            var fetchedBundle = await query
                .AsCosmosIteratorAsync()
                .SingleAsync()
                .ConfigureAwait(false);

            var updateTasks = fetchedBundle
                .AffectedDrawers
                .Select(changes => Task.WhenAll(
                    UpdateDrawerAsync(changes),
                    UpdateCatalogAsync(changes),
                    DeleteOldCatalogEntriesAsync(fetchedBundle, changes)));

            await Task.WhenAll(updateTasks).ConfigureAwait(false);
        }

        public async Task WriteToArchiveAsync(IEnumerable<Uuid> dataAvailableNotifications, string partitionKey)
        {
            Guard.ThrowIfNull(partitionKey, nameof(partitionKey));

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

        private async Task UpdateDrawerAsync(CosmosCabinetDrawerChanges changes)
        {
            var options = new ItemRequestOptions
            {
                IfMatchEtag = changes.UpdatedDrawer.ETag
            };

            var updatedDrawer = changes.UpdatedDrawer;

            try
            {
                await _repositoryContainer
                     .Container
                     .ReplaceItemAsync(updatedDrawer, updatedDrawer.Id, requestOptions: options)
                     .ConfigureAwait(false);
            }
            catch (CosmosException ex) when (ex.StatusCode == HttpStatusCode.PreconditionFailed)
            {
                // When two Acknowledge are executing, they must not overwrite each others values.
                // The failed ReplaceItemAsync is discarded.
            }
        }

        private async Task UpdateCatalogAsync(CosmosCabinetDrawerChanges changes)
        {
            var updatedCatalogEntry = changes.UpdatedCatalogEntry;
            if (updatedCatalogEntry == null)
                return;

            await _repositoryContainer
                .Container
                .UpsertItemAsync(updatedCatalogEntry)
                .ConfigureAwait(false);
        }

        private async Task DeleteOldCatalogEntriesAsync(CosmosBundleDocument2 bundle, CosmosCabinetDrawerChanges changes)
        {
            var asLinq = _repositoryContainer
                .Container
                .GetItemLinqQueryable<CosmosCatalogEntry>();

            var partitionKey = string.Join('_', bundle.Recipient, bundle.Origin);
            var contentType = bundle.MessageType;

            var query =
                from catalogEntry in asLinq
                where catalogEntry.PartitionKey == partitionKey &&
                      catalogEntry.ContentType == contentType &&
                      catalogEntry.NextSequenceNumber == changes.InitialCatalogEntrySequenceNumber
                select new { catalogEntry.Id, catalogEntry.PartitionKey };

            var entry = await query
                .AsCosmosIteratorAsync()
                .SingleOrDefaultAsync()
                .ConfigureAwait(false);

            // The entry will always be present initially.
            // The null-check handles case of two concurrent Acknowledge.
            if (entry != null)
            {
                try
                {
                    await _repositoryContainer
                         .Container
                         .DeleteItemAsync<CosmosCatalogEntry>(entry.Id, new PartitionKey(entry.PartitionKey))
                         .ConfigureAwait(false);
                }
                catch (CosmosException ex) when (ex.StatusCode == HttpStatusCode.NotFound)
                {
                    // Concurrent Acknowledge already removed the file.
                }
            }
        }

        private Task ArchiveDocumentAsync(CosmosDataAvailable documentToWrite)
        {
            return _repositoryContainer
                .ArchiveContainer.UpsertItemAsync(documentToWrite, new PartitionKey(documentToWrite.PartitionKey));
        }
    }
}
