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
using System.Threading.Tasks;
using Energinet.DataHub.PostOffice.Application;
using Energinet.DataHub.PostOffice.Application.GetMessage.Interfaces;
using Energinet.DataHub.PostOffice.Application.GetMessage.Queries;
using Energinet.DataHub.PostOffice.Domain;

namespace Energinet.DataHub.PostOffice.Infrastructure.GetMessage
{
    public class DataAvailableStorageService : IDataAvailableStorageService
    {
        private readonly IDocumentStore<DataAvailable> _cosmosDocumentStore;

        public DataAvailableStorageService(IDocumentStore<DataAvailable> cosmosDocumentStore)
        {
            _cosmosDocumentStore = cosmosDocumentStore;
        }

        public async Task<RequestData> GetDataAvailableUuidsAsync(GetMessageQuery getMessageQuery)
        {
            if (getMessageQuery is null) throw new ArgumentNullException(nameof(getMessageQuery));

            const string queryString = "SELECT * FROM c WHERE c.recipient = @recipient ORDER BY c._ts ASC OFFSET 0 LIMIT 1";
            var parameters = new List<KeyValuePair<string, string>>() { new ("recipient", getMessageQuery.Recipient) };

            var documents = await _cosmosDocumentStore.GetDocumentsAsync(queryString, parameters).ConfigureAwait(false);
            var document = documents.FirstOrDefault();

            return document is not null ? new RequestData() { Origin = document.origin, Uuids = new List<string?>() { document.uuid } } : new RequestData();
        }
    }
}
