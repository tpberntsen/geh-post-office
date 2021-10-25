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
using Energinet.DataHub.PostOffice.Domain.Model;
using Energinet.DataHub.PostOffice.Domain.Repositories;
using Energinet.DataHub.PostOffice.Domain.Services;
using Energinet.DataHub.PostOffice.Infrastructure.Common;
using Energinet.DataHub.PostOffice.Infrastructure.Documents;
using Energinet.DataHub.PostOffice.Infrastructure.Mappers;
using Energinet.DataHub.PostOffice.Infrastructure.Model;
using Energinet.DataHub.PostOffice.Infrastructure.Repositories.Containers;
using Microsoft.Azure.Cosmos;

namespace Energinet.DataHub.PostOffice.Infrastructure.Repositories
{
    public sealed class BundleRepository : IBundleRepository
    {
        private readonly IBundleRepositoryContainer _repositoryContainer;
        private readonly IMarketOperatorDataStorageService _marketOperatorDataStorageService;

        public BundleRepository(
            IBundleRepositoryContainer repositoryContainer,
            IMarketOperatorDataStorageService marketOperatorDataStorageService)
        {
            _repositoryContainer = repositoryContainer;
            _marketOperatorDataStorageService = marketOperatorDataStorageService;
        }

        public Task<Bundle?> GetNextUnacknowledgedAsync(MarketOperator recipient, params DomainOrigin[] domains)
        {
            if (recipient is null)
                throw new ArgumentNullException(nameof(recipient));

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
                where
                    bundle.Recipient == recipient.Gln.Value &&
                    !bundle.Dequeued
                orderby bundle.Timestamp
                select bundle;

            return GetNextUnacknowledgedAsync(recipient, query);
        }

        public async Task<BundleCreatedResponse> TryAddNextUnacknowledgedAsync(Bundle bundle)
        {
            if (bundle == null)
                throw new ArgumentNullException(nameof(bundle));

            var messageDocument = BundleMapper.MapToDocument(bundle);
            var requestOptions = new ItemRequestOptions
            {
                PostTriggers = new[] { "EnsureSingleUnacknowledgedBundle" }
            };

            try
            {
                await _repositoryContainer.Container
                    .CreateItemAsync(messageDocument, requestOptions: requestOptions)
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
            if (recipient is null)
                throw new ArgumentNullException(nameof(recipient));

            if (bundleId is null)
                throw new ArgumentNullException(nameof(bundleId));

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

        public Task SaveAsync(Bundle bundle)
        {
            if (bundle == null)
                throw new ArgumentNullException(nameof(bundle));

            var messageDocument = BundleMapper.MapToDocument(bundle);

            return _repositoryContainer.Container.ReplaceItemAsync(messageDocument, messageDocument.Id);
        }

        private static bool IsConcurrencyError(CosmosException ex)
        {
            return ex.ResponseBody.Contains("SingleBundleViolation", StringComparison.OrdinalIgnoreCase);
        }

        private static bool IsBundleIdDuplicateError(CosmosException ex)
        {
            return ex.StatusCode == HttpStatusCode.Conflict;
        }

        private async Task<Bundle?> GetNextUnacknowledgedAsync(MarketOperator recipient, IQueryable<CosmosBundleDocument> query)
        {
            var bundleDocument = await query.AsCosmosIteratorAsync().FirstOrDefaultAsync().ConfigureAwait(false);
            if (bundleDocument == null)
                return null;

            IBundleContent? bundleContent = null;

            if (!string.IsNullOrWhiteSpace(bundleDocument.ContentPath))
            {
                bundleContent = new AzureBlobBundleContent(_marketOperatorDataStorageService, new Uri(bundleDocument.ContentPath));
            }

            return new Bundle(
                new Uuid(bundleDocument.Id),
                Enum.Parse<DomainOrigin>(bundleDocument.Origin),
                recipient,
                bundleDocument.NotificationIds.Select(x => new Uuid(x)).ToList(),
                bundleContent);
        }
    }
}
