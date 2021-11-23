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
using Azure;
using Azure.Storage.Blobs;
using Energinet.DataHub.MessageHub.Client.Factories;
using Energinet.DataHub.MessageHub.Model.Exceptions;
using Energinet.DataHub.MessageHub.Model.Model;

namespace Energinet.DataHub.MessageHub.Client.Storage
{
    public sealed class StorageHandler : IStorageHandler
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
                var blobClient = CreateBlobClient($"{requestDto.IdempotencyId}_data");
                await blobClient.UploadAsync(stream, true).ConfigureAwait(false);
                return blobClient.Uri;
            }
            catch (RequestFailedException e)
            {
                throw new MessageHubStorageException("Error uploading file to storage", e);
            }
        }

        public Task<IReadOnlyList<Guid>> GetDataAvailableNotificationIdsAsync(DataBundleRequestDto bundleRequest)
        {
            return bundleRequest != null
                ? GetDataAvailableNotificationIdsAsync(bundleRequest.DataAvailableNotificationReferenceId)
                : throw new ArgumentNullException(nameof(bundleRequest));
        }

        public Task<IReadOnlyList<Guid>> GetDataAvailableNotificationIdsAsync(DequeueNotificationDto dequeueNotification)
        {
            return dequeueNotification != null
                ? GetDataAvailableNotificationIdsAsync(dequeueNotification.DataAvailableNotificationReferenceId)
                : throw new ArgumentNullException(nameof(dequeueNotification));
        }

        private async Task<IReadOnlyList<Guid>> GetDataAvailableNotificationIdsAsync(string referenceId)
        {
            try
            {
                var blobClient = CreateBlobClient(referenceId);
                var guidResults = await blobClient
                    .DownloadContentAsync()
                    .ConfigureAwait(false);

                var rawBytes = guidResults.Value.Content.ToArray();
                var guids = new List<Guid>();

                for (var i = 0; i < rawBytes.Length;)
                {
                    var j = i + 16;
                    guids.Add(new Guid(rawBytes[i..j]));
                    i = j;
                }

                return guids;
            }
            catch (RequestFailedException e)
            {
                throw new MessageHubStorageException("Error downloading file from storage", e);
            }
        }

        private BlobClient CreateBlobClient(string blobFileName)
        {
            return _storageServiceClientFactory
                .Create()
                .GetBlobContainerClient(_storageConfig.AzureBlobStorageContainerName)
                .GetBlobClient(blobFileName);
        }
    }
}
