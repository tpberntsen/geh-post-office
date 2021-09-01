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
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Azure.Storage.Blobs;
using Energinet.DataHub.PostOffice.Domain.Model;
using Energinet.DataHub.PostOffice.Domain.Repositories;
using Energinet.DataHub.PostOffice.Infrastructure.Documents;
using Energinet.DataHub.PostOffice.Infrastructure.Mappers;
using Energinet.DataHub.PostOffice.Infrastructure.Repositories.Containers;
using Microsoft.Azure.Cosmos;

namespace Energinet.DataHub.PostOffice.Infrastructure.Repositories
{
    // todo : correct implementation #136
    public class BundleRepository : IBundleRepository
    {
        private readonly IBundleRepositoryContainer _repositoryContainer;

        public BundleRepository(IBundleRepositoryContainer repositoryContainer)
        {
            _repositoryContainer = repositoryContainer;
        }

        public async Task<IBundle?> GetNextUnacknowledgedAsync(MarketOperator recipient)
        {
            if (recipient is null)
                throw new ArgumentNullException(nameof(recipient));

            var documentQuery =
                new QueryDefinition(
                        "SELECT * FROM c WHERE c.recipient = @recipient AND c.dequeued = @dequeued ORDER BY c._ts ASC OFFSET 0 LIMIT 1")
                    .WithParameter("@recipient", recipient.Value)
                    .WithParameter("@dequeued", false);

            using FeedIterator<BundleDocument> feedIterator =
                _repositoryContainer.Container.GetItemQueryIterator<BundleDocument>(documentQuery);

            var documentsFromCosmos =
                await feedIterator
                    .ReadNextAsync()
                    .ConfigureAwait(false);

            var document = documentsFromCosmos
                .FirstOrDefault();

            if (document is null)
                return null;

            return new Bundle(new Uuid(document.Id), document.NotificationsIds.Select(x => new Uuid(x)), async () =>
            {
                var connectionString = Environment.GetEnvironmentVariable("BlobStorageConnectionString");
                var blobServiceClient = new BlobServiceClient(connectionString);
                var container = blobServiceClient.GetBlobContainerClient(Environment.GetEnvironmentVariable("BlobStorageContainerName"));
                await container.CreateIfNotExistsAsync().ConfigureAwait(false);
                var blob = container.GetBlobClient(recipient + "/" + document.Id);

                var response = await blob.DownloadStreamingAsync().ConfigureAwait(false);
                return response.Value.Content;
            });
        }

        public async Task<IBundle> CreateBundleAsync(
            IEnumerable<DataAvailableNotification> dataAvailableNotifications)
        {
            var availableNotifications = dataAvailableNotifications.ToList();

            if (!availableNotifications.Any())
                throw new ArgumentOutOfRangeException(nameof(dataAvailableNotifications));

            // TODO: Fetch data from subdomain here and add path to bundle document
            var bundle = new Bundle(
                new Uuid(Guid.NewGuid().ToString()),
                availableNotifications.Select(x => x.NotificationId),
                () => Task.FromResult(Stream.Null));
            var recipient = availableNotifications.First().Recipient;

            var messageDocument = BundleMapper.MapToDocument(bundle, recipient);

            var response =
                await _repositoryContainer.Container
                    .CreateItemAsync(messageDocument)
                    .ConfigureAwait(false);

            if (response.StatusCode != HttpStatusCode.Created)
                throw new InvalidOperationException("Could not create document in cosmos");

            return bundle;
        }

        public async Task AcknowledgeAsync(Uuid bundleId)
        {
            if (bundleId is null)
                throw new ArgumentNullException(nameof(bundleId));

            var documentQuery =
                new QueryDefinition("SELECT * FROM c WHERE c.id = @id ORDER BY c._ts ASC OFFSET 0 LIMIT 1")
                    .WithParameter("@id", bundleId.Value);

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
                        .ReplaceItemAsync(dequeuedBundleDocument, dequeuedBundleDocument.Id?.ToString())
                        .ConfigureAwait(false);

                if (response.StatusCode != HttpStatusCode.OK)
                    throw new InvalidOperationException("Could not dequeue document in cosmos");
            }
        }
    }
}
