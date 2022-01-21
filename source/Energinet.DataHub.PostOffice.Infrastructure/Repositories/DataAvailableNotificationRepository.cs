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
using Energinet.DataHub.PostOffice.Infrastructure.Mappers;
using Energinet.DataHub.PostOffice.Infrastructure.Model;
using Energinet.DataHub.PostOffice.Infrastructure.Repositories.Containers;
using Energinet.DataHub.PostOffice.Utilities;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Cosmos.Linq;

namespace Energinet.DataHub.PostOffice.Infrastructure.Repositories
{
    public sealed class DataAvailableNotificationRepository : IDataAvailableNotificationRepository
    {
        private const int MaximumCabinetDrawerItemCount = 10000;
        private const int MaximumCabinetDrawersInRequest = 6;

        private readonly IDataAvailableNotificationRepositoryContainer _repositoryContainer;
        private readonly IBundleRepositoryContainer _bundleRepositoryContainer;
        private readonly ISequenceNumberRepository _sequenceNumberRepository;

        public DataAvailableNotificationRepository(
            IBundleRepositoryContainer bundleRepositoryContainer,
            IDataAvailableNotificationRepositoryContainer repositoryContainer,
            ISequenceNumberRepository sequenceNumberRepository)
        {
            _bundleRepositoryContainer = bundleRepositoryContainer;
            _repositoryContainer = repositoryContainer;
            _sequenceNumberRepository = sequenceNumberRepository;
        }

        public async Task SaveAsync(IEnumerable<DataAvailableNotification> dataAvailableNotifications, CabinetKey key)
        {
            Guard.ThrowIfNull(key, nameof(key));
            Guard.ThrowIfNull(dataAvailableNotifications, nameof(dataAvailableNotifications));

            var nextDrawer = await FindNextAvailableDrawerAsync(key).ConfigureAwait(false);

            var nextDrawerSize = nextDrawer is null ? 0 : await GetDrawerSizeAsync(nextDrawer.Id).ConfigureAwait(false);

            foreach (var notification in dataAvailableNotifications)
            {
                if (nextDrawer is null)
                {
                    nextDrawer = CreateNewDrawer(key, notification);
                    nextDrawerSize = 0;

                    await _repositoryContainer.Cabinet.CreateItemAsync(nextDrawer).ConfigureAwait(false);
                }

                if (nextDrawer.Position == nextDrawerSize)
                {
                    var catalogEntry = CreateCatalogEntry(notification);
                    await _repositoryContainer.Catalog.CreateItemAsync(catalogEntry).ConfigureAwait(false);
                }

                var cosmosDataAvailable = DataAvailableNotificationMapper.Map(notification, nextDrawer.Id);
                await _repositoryContainer.Cabinet.CreateItemAsync(cosmosDataAvailable).ConfigureAwait(false);
                nextDrawerSize++;

                if (nextDrawerSize.Equals(10000))
                {
                    nextDrawer = null;
                }
            }
        }

        public async Task<CabinetKey?> ReadCatalogForNextUnacknowledgedAsync(MarketOperator recipient, params DomainOrigin[] domains)
        {
            Guard.ThrowIfNull(recipient, nameof(recipient));
            Guard.ThrowIfNull(domains, nameof(domains));

            if (domains.Length == 0)
            {
                domains = Enum.GetValues<DomainOrigin>();
            }

            var catalogTasks = domains
                .Where(origin => origin != DomainOrigin.Unknown)
                .Select(async origin =>
                {
                    var entry = await ReadFromCatalogAsync(recipient, origin).ConfigureAwait(false);
                    return new { domainOrigin = origin, entry };
                }).ToList();

            await Task.WhenAll(catalogTasks).ConfigureAwait(false);

            var entryDomain = DomainOrigin.Unknown;
            CosmosCatalogEntry? smallestEntry = null;

            foreach (var catalogTask in catalogTasks)
            {
                var result = await catalogTask.ConfigureAwait(false);
                if (result.entry == null)
                    continue;

                if (smallestEntry == null || smallestEntry.NextSequenceNumber > result.entry.NextSequenceNumber)
                {
                    smallestEntry = result.entry;
                    entryDomain = result.domainOrigin;
                }
            }

            var maximumSequenceNumber = await _sequenceNumberRepository
                .GetMaximumSequenceNumberAsync()
                .ConfigureAwait(false);

            return smallestEntry == null || smallestEntry.NextSequenceNumber > maximumSequenceNumber.Value
                ? null
                : new CabinetKey(recipient, entryDomain, new ContentType(smallestEntry.ContentType));
        }

        public async Task<ICabinetReader> GetCabinetReaderAsync(CabinetKey cabinetKey)
        {
            Guard.ThrowIfNull(cabinetKey, nameof(cabinetKey));

            var drawers = new List<CosmosCabinetDrawer>();
            var content = new List<Task<IEnumerable<CosmosDataAvailable>>>();

            await foreach (var drawer in GetCabinetDrawersAsync(cabinetKey).ConfigureAwait(false))
            {
                var drawerContent = GetCabinetDrawerContentsAsync(drawer);
                drawers.Add(drawer);
                content.Add(drawerContent);
            }

            var cabinetReader = new AsyncCabinetReader(cabinetKey, drawers, content);
            await cabinetReader.InitializeAsync().ConfigureAwait(false);
            return cabinetReader;
        }

        public async Task AcknowledgeAsync(Bundle bundle)
        {
            Guard.ThrowIfNull(bundle, nameof(bundle));

            var asLinq = _bundleRepositoryContainer
                .Container
                .GetItemLinqQueryable<CosmosBundleDocument>();

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

        private static async IAsyncEnumerable<T> ExecuteQueryAsync<T>(IQueryable<T> query)
        {
            await foreach (var document in query.AsCosmosIteratorAsync().ConfigureAwait(false))
            {
                yield return document;
            }
        }

        private static CosmosCatalogEntry CreateCatalogEntry(DataAvailableNotification notification)
        {
            return new CosmosCatalogEntry()
            {
                Id = Guid.NewGuid().ToString(),
                ContentType = notification.ContentType.Value,
                NextSequenceNumber = notification.SequenceNumber.Value,
                PartitionKey = string.Join("_", notification.Recipient.Gln.Value, notification.Origin)
            };
        }

        private static CosmosCabinetDrawer CreateNewDrawer(CabinetKey key, DataAvailableNotification notification)
        {
            var partitionKey = string.Join('_', key.Recipient.Gln.Value, key.Origin, key.ContentType.Value);

            return new CosmosCabinetDrawer
            {
                Id = Guid.NewGuid().ToString(),
                PartitionKey = partitionKey,
                OrderBy = notification.SequenceNumber.Value,
                Position = 0
            };
        }

        private Task<CosmosCatalogEntry?> ReadFromCatalogAsync(MarketOperator recipient, DomainOrigin domain)
        {
            var asLinq = _repositoryContainer
                    .Catalog
                    .GetItemLinqQueryable<CosmosCatalogEntry>();

            var partitionKey = string.Join('_', recipient.Gln.Value, domain);

            var query =
                from catalogEntry in asLinq
                where catalogEntry.PartitionKey == partitionKey
                orderby catalogEntry.NextSequenceNumber
                select catalogEntry;

            return query
                .Take(1)
                .AsCosmosIteratorAsync()
                .FirstOrDefaultAsync();
        }

        private IAsyncEnumerable<CosmosCabinetDrawer> GetCabinetDrawersAsync(CabinetKey cabinetKey)
        {
            var partitionKey = string.Join(
                '_',
                cabinetKey.Recipient.Gln.Value,
                cabinetKey.Origin,
                cabinetKey.ContentType.Value);

            var asLinq = _repositoryContainer
                .Cabinet
                .GetItemLinqQueryable<CosmosCabinetDrawer>();

            var query =
                from cabinetDrawer in asLinq
                where
                    cabinetDrawer.PartitionKey == partitionKey &&
                    cabinetDrawer.Position < MaximumCabinetDrawerItemCount
                orderby cabinetDrawer.OrderBy
                select cabinetDrawer;

            return query.Take(MaximumCabinetDrawersInRequest).AsCosmosIteratorAsync();
        }

        private async Task<IEnumerable<CosmosDataAvailable>> GetCabinetDrawerContentsAsync(CosmosCabinetDrawer drawer)
        {
            var maximumSequenceNumber = await _sequenceNumberRepository
                .GetMaximumSequenceNumberAsync()
                .ConfigureAwait(false);

            var asLinq = _repositoryContainer
                .Cabinet
                .GetItemLinqQueryable<CosmosDataAvailable>();

            var query =
                from dataAvailableNotification in asLinq
                where
                    dataAvailableNotification.PartitionKey == drawer.Id &&
                    dataAvailableNotification.SequenceNumber <= maximumSequenceNumber.Value
                orderby dataAvailableNotification.SequenceNumber
                select dataAvailableNotification;

            return await query
                .Skip(drawer.Position)
                .AsCosmosIteratorAsync()
                .ToListAsync()
                .ConfigureAwait(false);
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
                     .Cabinet
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
                .Catalog
                .UpsertItemAsync(updatedCatalogEntry)
                .ConfigureAwait(false);
        }

        private async Task DeleteOldCatalogEntriesAsync(CosmosBundleDocument bundle, CosmosCabinetDrawerChanges changes)
        {
            var asLinq = _repositoryContainer
                .Catalog
                .GetItemLinqQueryable<CosmosCatalogEntry>();

            var partitionKey = string.Join('_', bundle.Recipient, bundle.Origin);
            var contentType = bundle.ContentType;

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
                         .Catalog
                         .DeleteItemAsync<CosmosCatalogEntry>(entry.Id, new PartitionKey(entry.PartitionKey))
                         .ConfigureAwait(false);
                }
                catch (CosmosException ex) when (ex.StatusCode == HttpStatusCode.NotFound)
                {
                    // Concurrent Acknowledge already removed the file.
                }
            }
        }

        private async Task<CosmosCabinetDrawer?> FindNextAvailableDrawerAsync(CabinetKey cabinetKey)
        {
            var asLinq = _repositoryContainer
                .Cabinet
                .GetItemLinqQueryable<CosmosCabinetDrawer>();

            var partitionKey = string.Join(cabinetKey.Recipient.Gln.Value, cabinetKey.Origin, cabinetKey.ContentType.Value);

            var query =
                from cabinetDrawer in asLinq
                where
                    cabinetDrawer.PartitionKey == partitionKey && cabinetDrawer.Position < 10000
                orderby cabinetDrawer.OrderBy descending
                select cabinetDrawer;

            return await ExecuteQueryAsync(query)
                .FirstOrDefaultAsync()
                .ConfigureAwait(false);
        }

        private async Task<int> GetDrawerSizeAsync(string partitionKey)
        {
            var asLinq = _repositoryContainer
                .Cabinet
                .GetItemLinqQueryable<CosmosDataAvailable>();

            var query =
                from dataAvailable in asLinq
                where
                    dataAvailable.PartitionKey == partitionKey
                select dataAvailable;

            return await query.CountAsync().ConfigureAwait(false);
        }
    }
}
