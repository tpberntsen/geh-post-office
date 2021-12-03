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
using System.Threading.Tasks;
using Energinet.DataHub.MessageHub.Core.Factories;
using Energinet.DataHub.PostOffice.Infrastructure.Correlation;

namespace Energinet.DataHub.PostOffice.Infrastructure.Services
{
    public class LogResourceService
    {
        private readonly IStorageServiceClientFactory _storageServiceClientFactory;
        private readonly ICorrelationContext _correlationContext;

        public LogResourceService(
            IStorageServiceClientFactory storageServiceClientFactory,
            ICorrelationContext correlationContext)
        {
            _storageServiceClientFactory = storageServiceClientFactory;
            _correlationContext = correlationContext;
        }

        public Task LogRequestAsync(Stream body, Dictionary<string, string> metaData)
        {
            var name = $"{DateTime.UtcNow.ToShortDateString()}-request-{_correlationContext.Id}";
            return CreateLogAsync(name, body, metaData);
        }

        public Task LogResponseAsync(Stream body, Dictionary<string, string> metaData)
        {
            var name = $"{DateTime.UtcNow.ToShortDateString()}-response-{_correlationContext.Id}";
            return CreateLogAsync(name, body, metaData);
        }

        private Task CreateLogAsync(string name, Stream body, Dictionary<string, string> metaData)
        {
            var storage = _storageServiceClientFactory.Create();
            var client = storage.GetBlobContainerClient("postoffice-reply");
            var blobClient = client.GetBlobClient(name);
            return blobClient.UploadAsync(body, null,  metaData);
        }
    }
}
