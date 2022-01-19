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
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Energinet.DataHub.MessageHub.Core.Storage;
using Energinet.DataHub.PostOffice.Domain.Model;
using Energinet.DataHub.PostOffice.Domain.Repositories;
using Energinet.DataHub.PostOffice.Domain.Services;
using Energinet.DataHub.PostOffice.Infrastructure.Common;
using Energinet.DataHub.PostOffice.Infrastructure.Documents;
using Energinet.DataHub.PostOffice.Infrastructure.Mappers;
using Energinet.DataHub.PostOffice.Infrastructure.Model;
using Energinet.DataHub.PostOffice.Infrastructure.Repositories.Containers;
using Energinet.DataHub.PostOffice.Utilities;
using Microsoft.Azure.Cosmos;

namespace Energinet.DataHub.PostOffice.Infrastructure.Repositories
{
    public sealed class BundleRepository : IBundleRepository
    {
        private readonly IMarketOperatorDataStorageService _marketOperatorDataStorageService;
        private readonly IBundleRepositoryContainer _repositoryContainer;
        private readonly IStorageHandler _storageHandler;

        public BundleRepository(
            IStorageHandler storageHandler,
            IBundleRepositoryContainer repositoryContainer,
            IMarketOperatorDataStorageService marketOperatorDataStorageService)
        {
            _storageHandler = storageHandler;
            _repositoryContainer = repositoryContainer;
            _marketOperatorDataStorageService = marketOperatorDataStorageService;
        }

        public Task<Bundle?> GetNextUnacknowledgedAsync(MarketOperator recipient, params DomainOrigin[] domains)
        {
            Guard.ThrowIfNull(recipient, nameof(recipient));

            var asLinq = _repositoryContainer
                .Container
                .GetItemLinqQueryable<CosmosBundleDocument>();

            IQueryable<CosmosBundleDocument> domainFiltered = asLinq;

            if (domains is { Length: > 0 })
            {
                var selectedDomains = domains.Select(x => x.ToString());
                domainFiltered = asLinq.Where(x => selectedDomains.Contains(x.Origin));
            }

            var query =
                from bundle in domainFiltered
                where bundle.Recipient == recipient.Gln.Value && !bundle.Dequeued
                orderby bundle.Timestamp
                select bundle;

            return GetNextUnacknowledgedAsync(query);
        }

        public async Task<BundleCreatedResponse> TryAddNextUnacknowledgedAsync(Bundle bundle, ICabinetReader cabinetReader)
        {
            Guard.ThrowIfNull(bundle, nameof(bundle));
            Guard.ThrowIfNull(cabinetReader, nameof(cabinetReader));

            await _storageHandler
                .AddDataAvailableNotificationIdsToStorageAsync(bundle.ProcessId.ToString(), bundle.NotificationIds.Select(x => x.AsGuid()))
                .ConfigureAwait(false);

            var reader = (AsyncCabinetReader)cabinetReader;

            var cosmosBundleDocument = BundleMapper.Map(bundle, reader.GetChanges());
            var requestOptions = new ItemRequestOptions { PostTriggers = new[] { "EnsureSingleUnacknowledgedBundle" } };

            try
            {
                await _repositoryContainer.Container
                    .CreateItemAsync(cosmosBundleDocument, requestOptions: requestOptions)
                    .ConfigureAwait(false);
                return BundleCreatedResponse.Success;
            }
            catch (CosmosException ex) when (IsConcurrencyError(ex))
            {
                return BundleCreatedResponse.AnotherBundleExists;
            }
            catch (CosmosException ex) when (IsBundleIdDuplicateError(ex))
            {
                return BundleCreatedResponse.BundleIdAlreadyInUse;
            }
        }

        public async Task AcknowledgeAsync(MarketOperator recipient, Uuid bundleId)
        {
            Guard.ThrowIfNull(recipient, nameof(recipient));
            Guard.ThrowIfNull(bundleId, nameof(bundleId));

            var asLinq = _repositoryContainer
                .Container
                .GetItemLinqQueryable<CosmosBundleDocument>();

            var query =
                from bundle in asLinq
                where bundle.Id == bundleId.ToString() && bundle.Recipient == recipient.Gln.Value
                select bundle;

            var bundleToUpdate = await query
                .AsCosmosIteratorAsync()
                .SingleAsync()
                .ConfigureAwait(false);

            var updatedBundle = bundleToUpdate with { Dequeued = true };
            await _repositoryContainer.Container
                .ReplaceItemAsync(updatedBundle, updatedBundle.Id)
                .ConfigureAwait(false);
        }

        public async Task SaveAsync(Bundle bundle)
        {
            Guard.ThrowIfNull(bundle, nameof(bundle));

            var asLinq = _repositoryContainer
                .Container
                .GetItemLinqQueryable<CosmosBundleDocument>();

            var query =
                from cosmosBundleDocument in asLinq
                where
                    cosmosBundleDocument.Id == bundle.BundleId.ToString() &&
                    cosmosBundleDocument.Recipient == bundle.Recipient.Gln.Value
                select new { cosmosBundleDocument.AffectedDrawers };

            var changes = await query
                .AsCosmosIteratorAsync()
                .SingleAsync()
                .ConfigureAwait(false);

            var updatedBundle = BundleMapper.Map(bundle, changes.AffectedDrawers);

            await _repositoryContainer.Container
                .ReplaceItemAsync(updatedBundle, updatedBundle.Id)
                .ConfigureAwait(false);
        }

        private static bool IsConcurrencyError(CosmosException ex)
        {
            return ex.ResponseBody.Contains("SingleBundleViolation", StringComparison.OrdinalIgnoreCase);
        }

        private static bool IsBundleIdDuplicateError(CosmosException ex)
        {
            return ex.StatusCode == HttpStatusCode.Conflict;
        }

        private async Task<Bundle?> GetNextUnacknowledgedAsync(IQueryable<CosmosBundleDocument> query)
        {
            var bundleDocument = await query.AsCosmosIteratorAsync().FirstOrDefaultAsync().ConfigureAwait(false);
            if (bundleDocument == null)
                return null;

            IBundleContent? bundleContent = null;

            if (!string.IsNullOrWhiteSpace(bundleDocument.ContentPath))
                bundleContent = new AzureBlobBundleContent(_marketOperatorDataStorageService, new Uri(bundleDocument.ContentPath));

            return BundleMapper.Map(bundleDocument, bundleContent);
        }
    }
}
