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
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Azure.Storage.Blobs.Models;
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

        public Task LogRequestAsync(string requestData)
        {
            var name = $"{_correlationContext.Id}-request";
            return CreateLogAsync(name, requestData);
        }

        public Task LogResponseAsync(string responseData)
        {
            var name = $"{_correlationContext.Id}-response";
            return CreateLogAsync(name, responseData);
        }

        private Task CreateLogAsync(string name, string data)
        {
            var storage = _storageServiceClientFactory.Create();
            var client = storage.GetBlobContainerClient("postoffice-reply");
            return client.UploadBlobAsync(name, BinaryData.FromString(data));
        }
    }
}
