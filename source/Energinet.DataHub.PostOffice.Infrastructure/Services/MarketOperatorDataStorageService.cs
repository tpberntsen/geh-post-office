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
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Azure.Storage.Blobs;
using Energinet.DataHub.PostOffice.Domain.Services;

namespace Energinet.DataHub.PostOffice.Infrastructure.Services
{
    public class MarketOperatorDataStorageService : IMarketOperatorDataStorageService
    {
        private static readonly string? _containerName = Environment.GetEnvironmentVariable("BlobStorageContainerName");
        private static readonly string? _connectionString = Environment.GetEnvironmentVariable("BlobStorageConnectionString");

        public async Task<Stream> GetMarkedOperatorDataAsync(Uri contentPath)
        {
            var container = new BlobContainerClient(_connectionString, _containerName);
            var blob = container.GetBlobClient(contentPath.Segments.Last());
            var response = await blob.DownloadStreamingAsync().ConfigureAwait(false);
            return response.Value.Content;
        }
    }
}
