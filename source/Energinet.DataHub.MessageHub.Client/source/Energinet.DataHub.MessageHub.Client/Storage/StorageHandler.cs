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
using Azure;
using Energinet.DataHub.MessageHub.Client.Exceptions;
using Energinet.DataHub.MessageHub.Client.Factories;
using Energinet.DataHub.MessageHub.Client.Model;

namespace Energinet.DataHub.MessageHub.Client.Storage
{
    public class StorageHandler : IStorageHandler
    {
        private readonly IStorageServiceClientFactory _storageServiceClientFactory;
        private readonly StorageConfig _storageConfig;

        public StorageHandler(IStorageServiceClientFactory storageServiceClientFactory, StorageConfig storageConfig)
        {
            _storageServiceClientFactory = storageServiceClientFactory;
            _storageConfig = storageConfig;
        }

        public async Task<Uri> AddStreamToStorageAsync(Stream stream, DataBundleRequestDto requestDto)
        {
            if (requestDto is null)
                throw new ArgumentNullException(nameof(requestDto));

            if (stream is not { Length: > 0 })
            {
                throw new ArgumentException($"{nameof(stream)} must be not null and have content", nameof(stream));
            }

            try
            {
                var storageClient = _storageServiceClientFactory.Create();
                var containerClient = storageClient.GetBlobContainerClient(_storageConfig.AzureBlobStorageContainerName);
                var blobName = requestDto.IdempotencyId;
                var blobClient = containerClient.GetBlobClient(blobName);
                await blobClient.UploadAsync(stream, true).ConfigureAwait(false);
                var blobUri = blobClient.Uri;
                return blobUri;
            }
            catch (RequestFailedException e)
            {
                throw new MessageHubStorageException("Error uploading file to storage", e);
            }
        }

        public async Task<Stream> GetStreamFromStorageAsync(Uri contentPath)
        {
            try
            {
                if (contentPath is null)
                    throw new ArgumentNullException(nameof(contentPath));

                var storageClient = _storageServiceClientFactory.Create();
                var containerClient = storageClient.GetBlobContainerClient(_storageConfig.AzureBlobStorageContainerName);
                var blob = containerClient.GetBlobClient(contentPath.Segments.Last());
                var response = await blob.DownloadStreamingAsync().ConfigureAwait(false);
                return response.Value.Content;
            }
            catch (RequestFailedException e)
            {
                throw new MessageHubStorageException("Error uploading file to storage", e);
            }
        }
    }
}
