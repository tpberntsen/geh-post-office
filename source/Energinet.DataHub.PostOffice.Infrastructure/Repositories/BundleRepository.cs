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
using Energinet.DataHub.PostOffice.Infrastructure.Mappers;
using Microsoft.Azure.Cosmos;

namespace Energinet.DataHub.PostOffice.Infrastructure.Repositories
{
    // todo : correct implementation #136
    public class BundleRepository : IBundleRepository
    {
        private readonly Container _container;

        public BundleRepository(Container container)
        {
            _container = container;
        }

        public Task<IBundle?> PeekAsync(Recipient recipient)
        {
            if (recipient is null)
                throw new ArgumentNullException(nameof(recipient));

            const string query = "SELECT * FROM c WHERE c.id = @recipient ORDER BY c._ts ASC OFFSET 0 LIMIT 1";
            var documentQuery = new QueryDefinition(query);
            documentQuery.WithParameter($"@recipient", recipient.Value);
            return Task.FromResult<IBundle?>(null);
        }

        public async Task<IBundle> CreateBundleAsync(IEnumerable<DataAvailableNotification> dataAvailableNotifications, Recipient recipient)
        {
            var bundle = new Bundle(new Uuid(Guid.NewGuid().ToString()), Enumerable.Empty<Uuid>());

            var messageDocument = BundleMapper.MapToDocument(bundle, recipient);

            var response = await _container.CreateItemAsync(messageDocument).ConfigureAwait(false);
            if (response.StatusCode != HttpStatusCode.Created)
                throw new InvalidOperationException("Could not create document in cosmos");

            return bundle;
        }

        public Task DequeueAsync(Uuid id)
        {
            return Task.CompletedTask;
        }
    }
}
