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
using System.Threading.Tasks;
using Azure;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Energinet.DataHub.PostOffice.Application.GetMessage.Interfaces;

namespace Energinet.DataHub.PostOffice.Infrastructure.GetMessage
{
    public class StorageService : IStorageService
    {
        public async Task<string> GetStorageContentAsync(string? containerName, string fileName)
        {
            if (containerName is null) throw new ArgumentNullException(nameof(containerName));

            var connectionString = Environment.GetEnvironmentVariable("BlobStorageConnectionString");
            var blobServiceClient = new BlobServiceClient(connectionString);
            var container = blobServiceClient.GetBlobContainerClient(containerName);
            await container.CreateIfNotExistsAsync().ConfigureAwait(false);
            var blob = container.GetBlobClient(fileName);
            try
            {
                await using MemoryStream ms = new MemoryStream();
                await blob.DownloadToAsync(ms).ConfigureAwait(false);
                using StreamReader sr = new StreamReader(ms);
                var pathToContent = await sr.ReadToEndAsync().ConfigureAwait(false);
                return pathToContent;
            }
            catch (RequestFailedException e)
                when (e.ErrorCode == BlobErrorCode.BlobNotFound)
            {
                return string.Empty;
            }
        }
    }
}
