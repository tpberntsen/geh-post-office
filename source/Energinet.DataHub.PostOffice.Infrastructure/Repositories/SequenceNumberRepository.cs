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
using Microsoft.Azure.Cosmos;

namespace Energinet.DataHub.PostOffice.Infrastructure.Repositories
{
    public class SequenceNumberRepository : ISequenceNumberRepository
    {
        private readonly IDataAvailableNotificationRepositoryContainer _repositoryContainer;

        public SequenceNumberRepository(IDataAvailableNotificationRepositoryContainer repositoryContainer)
        {
            _repositoryContainer = repositoryContainer;
        }

        public async Task<SequenceNumber> GetMaximumSequenceNumberAsync()
        {
            try
            {
                var response = await _repositoryContainer.Container.ReadItemAsync<CosmosSequenceNumber>(
                        "1",
                        new PartitionKey("SequenceNumber"))
                    .ConfigureAwait(false);

                return new SequenceNumber(response.Resource.SequenceNumber);
            }
            catch (CosmosException e) when (e.StatusCode == HttpStatusCode.NotFound)
            {
                return new SequenceNumber(0);
            }
        }

        public async Task AdvanceSequenceNumberAsync(SequenceNumber sequenceNumber)
        {
            if (sequenceNumber is null)
                throw new ArgumentNullException(nameof(sequenceNumber));

            var s = new CosmosSequenceNumber(sequenceNumber.Value);

            await _repositoryContainer.Container.UpsertItemAsync(s).ConfigureAwait(false);
        }
    }
}
