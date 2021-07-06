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

using System.Collections.Generic;
using System.Threading.Tasks;
using Energinet.DataHub.PostOffice.Application;
using Energinet.DataHub.PostOffice.Application.GetMessage;
using Energinet.DataHub.PostOffice.Domain;

namespace Energinet.DataHub.PostOffice.Infrastructure.GetMessage
{
    public class CosmosService : ICosmosService
    {
        private readonly IDocumentStore<DataAvailable> _cosmosDocumentStore;
        private readonly IList<string> _collection;

        public CosmosService(IDocumentStore<DataAvailable> cosmosDocumentStore)
        {
            _cosmosDocumentStore = cosmosDocumentStore;
            _collection = new List<string>();
        }

        public async Task<IList<string>> GetDataAvailableUuidsAsync(string recipient)
        {
            var documents = await _cosmosDocumentStore.GetDocumentsAsync(new GetMessageQuery(recipient))
                .ConfigureAwait(false);

            foreach (var document in documents)
            {
                if (document.uuid is not null)
                {
                    _collection.Add(document.uuid);
                }
            }

            return _collection;
        }
    }
}
