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

        public Task<Bundle?> GetNextUnacknowledgedAsync(MarketOperator recipient)
        {
            if (recipient is null)
                throw new ArgumentNullException(nameof(recipient));

            const string query =
                @"SELECT * FROM bundles
                  WHERE
                    bundles.recipient = @recipient AND
                    bundles.dequeued = false
                  ORDER BY bundles._ts ASC
                  OFFSET 0 LIMIT 1";

            var bundlesQuery = new QueryDefinition(query)
                .WithParameter("@recipient", recipient.Gln.Value);

            return GetNextUnacknowledgedAsync(recipient, bundlesQuery);
        }

        public Task<Bundle?> GetNextUnacknowledgedForDomainAsync(MarketOperator recipient, DomainOrigin domainOrigin)
        {
            if (recipient is null)
                throw new ArgumentNullException(nameof(recipient));

            const string query =
                @"SELECT * FROM bundles
                  WHERE
                    bundles.recipient = @recipient AND
                    bundles.origin = @domainOrigin AND
                    bundles.dequeued = false
                  ORDER BY bundles._ts ASC
                  OFFSET 0 LIMIT 1";

            var documentQuery = new QueryDefinition(query)
                    .WithParameter("@recipient", recipient.Gln.Value)
                    .WithParameter("@domainOrigin", domainOrigin.ToString());

            return GetNextUnacknowledgedAsync(recipient, documentQuery);
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

        public async Task AcknowledgeAsync(Uuid bundleId)
        {
            if (bundleId is null)
                throw new ArgumentNullException(nameof(bundleId));

            var documentQuery =
                new QueryDefinition("SELECT * FROM c WHERE c.id = @id ORDER BY c._ts ASC OFFSET 0 LIMIT 1")
                    .WithParameter("@id", bundleId.ToString());

            using var feedIterator = _repositoryContainer
                .Container
                .GetItemQueryIterator<BundleDocument>(documentQuery);

            var documentsFromCosmos =
                await feedIterator
                    .ReadNextAsync()
                    .ConfigureAwait(false);

            if (documentsFromCosmos.Any())
            {
                var dequeuedBundleDocument = documentsFromCosmos.First() with { Dequeued = true };
                var response =
                    await _repositoryContainer.Container
                        .ReplaceItemAsync(dequeuedBundleDocument, dequeuedBundleDocument.Id)
                        .ConfigureAwait(false);

                if (response.StatusCode != HttpStatusCode.OK)
                    throw new InvalidOperationException("Could not dequeue document in cosmos");
            }
        }

        public async Task SaveAsync(Bundle bundle)
        {
            if (bundle == null)
                throw new ArgumentNullException(nameof(bundle));

            var messageDocument = BundleMapper.MapToDocument(bundle);

            await _repositoryContainer.Container
                .ReplaceItemAsync(messageDocument, messageDocument.Id)
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

        private async Task<Bundle?> GetNextUnacknowledgedAsync(MarketOperator recipient, QueryDefinition bundleQuery)
        {
            using var feedIterator = _repositoryContainer
                .Container
                .GetItemQueryIterator<BundleDocument>(bundleQuery);

            var documentsFromCosmos = await feedIterator
                .ReadNextAsync()
                .ConfigureAwait(false);

            var document = documentsFromCosmos.FirstOrDefault();
            if (document == null)
                return null;

            IBundleContent? bundleContent = null;

            if (!string.IsNullOrWhiteSpace(document.ContentPath))
            {
                bundleContent = new AzureBlobBundleContent(_marketOperatorDataStorageService, new Uri(document.ContentPath));
            }

            return new Bundle(
                new Uuid(document.Id),
                Enum.Parse<DomainOrigin>(document.Origin),
                recipient,
                document.NotificationIds.Select(x => new Uuid(x)).ToList(),
                bundleContent);
        }
    }
}
