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
using Energinet.DataHub.PostOffice.Infrastructure.Entities;
using Energinet.DataHub.PostOffice.Infrastructure.Mappers;
using Energinet.DataHub.PostOffice.Infrastructure.Repositories.Containers;
using Microsoft.Azure.Cosmos;

namespace Energinet.DataHub.PostOffice.Infrastructure.Repositories
{
    // todo : correct implementation #136
    public class BundleRepository : IBundleRepository
    {
        private readonly Container _container;

        public BundleRepository(IBundleRepositoryContainer repositoryContainer)
        {
            if (repositoryContainer is null)
                throw new ArgumentNullException(nameof(repositoryContainer));

            _container = repositoryContainer.Container;
        }

        public async Task<IBundle?> PeekAsync(Recipient recipient)
        {
            if (recipient is null)
                throw new ArgumentNullException(nameof(recipient));

            var documentQuery =
                new QueryDefinition(
                    "SELECT * FROM c WHERE c.recipient = @recipient AND c.dequeued = @dequeued ORDER BY c._ts ASC OFFSET 0 LIMIT 1")
                    .WithParameter($"@recipient", recipient.Value)
                    .WithParameter("@dequeued", false);

            using FeedIterator<BundleDocument> feedIterator =
                _container.GetItemQueryIterator<BundleDocument>(documentQuery);

            var documentsFromCosmos =
                await feedIterator
                    .ReadNextAsync()
                    .ConfigureAwait(false);

            var documents = documentsFromCosmos
                .Select(BundleMapper.MapFromDocument);

            return documents?.FirstOrDefault();
        }

        public async Task<IBundle> CreateBundleAsync(
            IEnumerable<DataAvailableNotification> dataAvailableNotifications,
            Recipient recipient)
        {
            var availableNotifications = dataAvailableNotifications.ToList();

            if (!availableNotifications.Any())
                throw new ArgumentOutOfRangeException(nameof(dataAvailableNotifications));

            // TODO: Fetch data from subdomain here and add path to bundle document
            var bundle = new Bundle(
                new Uuid(Guid.NewGuid().ToString()),
                availableNotifications.Select(x => x.Id));

            var messageDocument = BundleMapper.MapToDocument(bundle, recipient);

            var response =
                await _container
                    .CreateItemAsync(messageDocument)
                    .ConfigureAwait(false);

            if (response.StatusCode != HttpStatusCode.Created)
                throw new InvalidOperationException("Could not create document in cosmos");

            return bundle;
        }

        public async Task DequeueAsync(Uuid id)
        {
            if (id is null)
                throw new ArgumentNullException(nameof(id));

            var documentQuery =
                new QueryDefinition("SELECT * FROM c WHERE c.id = @id ORDER BY c._ts ASC OFFSET 0 LIMIT 1")
                    .WithParameter($"@id", id.Value);

            using FeedIterator<BundleDocument> feedIterator =
                _container.GetItemQueryIterator<BundleDocument>(documentQuery);

            var documentsFromCosmos =
                await feedIterator
                    .ReadNextAsync()
                    .ConfigureAwait(false);

            if (documentsFromCosmos.Any())
            {
                var dequeuedBundleDocument = documentsFromCosmos.First() with { Dequeued = true };
                var response =
                    await _container
                        .ReplaceItemAsync(dequeuedBundleDocument, dequeuedBundleDocument.Id?.ToString())
                        .ConfigureAwait(false);

                if (response.StatusCode != HttpStatusCode.OK)
                    throw new InvalidOperationException("Could not dequeue document in cosmos");
            }
        }
    }
}
