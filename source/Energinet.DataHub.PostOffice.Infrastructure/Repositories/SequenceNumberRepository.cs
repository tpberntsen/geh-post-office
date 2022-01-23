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

using System.Net;
using System.Threading.Tasks;
using Energinet.DataHub.PostOffice.Domain.Model;
using Energinet.DataHub.PostOffice.Domain.Repositories;
using Energinet.DataHub.PostOffice.Infrastructure.Documents;
using Energinet.DataHub.PostOffice.Infrastructure.Repositories.Containers;
using Energinet.DataHub.PostOffice.Utilities;
using Microsoft.Azure.Cosmos;

namespace Energinet.DataHub.PostOffice.Infrastructure.Repositories
{
    public sealed class SequenceNumberRepository : ISequenceNumberRepository
    {
        private readonly IDataAvailableNotificationRepositoryContainer _repositoryContainer;
        private SequenceNumber? _sequenceNumberInScope;

        public SequenceNumberRepository(IDataAvailableNotificationRepositoryContainer repositoryContainer)
        {
            _repositoryContainer = repositoryContainer;
        }

        public async Task<SequenceNumber> GetMaximumSequenceNumberAsync()
        {
            if (_sequenceNumberInScope != null)
            {
                return _sequenceNumberInScope;
            }

            try
            {
                var response = await _repositoryContainer
                    .Catalog
                    .ReadItemAsync<CosmosSequenceNumber>(
                        CosmosSequenceNumber.CosmosSequenceNumberIdentifier,
                        new PartitionKey(CosmosSequenceNumber.CosmosSequenceNumberPartitionKey))
                    .ConfigureAwait(false);

                return _sequenceNumberInScope = new SequenceNumber(response.Resource.SequenceNumber);
            }
            catch (CosmosException e) when (e.StatusCode == HttpStatusCode.NotFound)
            {
                return new SequenceNumber(0);
            }
        }

        public Task AdvanceSequenceNumberAsync(SequenceNumber sequenceNumber)
        {
            Guard.ThrowIfNull(sequenceNumber, nameof(sequenceNumber));

            _sequenceNumberInScope = sequenceNumber;

            return _repositoryContainer
                .Catalog
                .UpsertItemAsync(new CosmosSequenceNumber(sequenceNumber.Value));
        }
    }
}
