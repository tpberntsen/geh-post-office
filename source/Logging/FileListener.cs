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
using System.Linq;
using System.Threading.Tasks;
using Energinet.DataHub.Logging.SearchOptimizer.Models;
using Energinet.DataHub.Logging.SearchOptimizer.Storage;
using Logging.Models;
using Logging.Storage;
using Microsoft.Azure.Functions.Worker;

namespace Logging
{
    public class FileListener
    {
        private readonly ICosmosClientProvider _cosmosClientProvider;
        private readonly ILongTermBlobServiceClient _longTermBlobServiceClient;
        private CosmosConfig _config;

        public FileListener(
            ICosmosClientProvider cosmosClientProvider,
            ILongTermBlobServiceClient longTermBlobServiceClient,
            CosmosConfig config)
        {
            _cosmosClientProvider = cosmosClientProvider;
            _config = config;
            _longTermBlobServiceClient = longTermBlobServiceClient;
        }

        [Function("FileListener")]
        public async Task RunAsync(
            [BlobTrigger("fromstorage/{name}", Connection = "BlobStoreConnectionString")]
            byte[] file,
            string name,
            IDictionary<string, string> metadata)
        {
            if (file is null)
            {
                throw new ArgumentNullException(nameof(file));
            }

            if (name is null)
            {
                throw new ArgumentNullException(nameof(name));
            }

            if (metadata is null)
            {
                throw new ArgumentNullException(nameof(metadata));
            }

            var stream = new MemoryStream();
            await stream.WriteAsync(file).ConfigureAwait(false);

            // Pick properties and parse them to local object
            var properties = GetPropertiesFromMetaData(metadata);

            // Store file in long-term storage
            var destContainerClient = _longTermBlobServiceClient.Client.GetBlobContainerClient("tostorage");

            var destBlob = destContainerClient.GetBlobClient(GenerateFileName(name));
            var newBlob = new MemoryStream();
            stream.Position = 0;
            newBlob.Position = 0;
            await stream.CopyToAsync(newBlob).ConfigureAwait(false);
            newBlob.Position = 0;
            await destBlob.UploadAsync(newBlob).ConfigureAwait(false);

            // Store properties in search-optimized storage
            var container = _cosmosClientProvider.Client.GetContainer(_config.DatabaseId, _config.ContainerId);
            await container.CreateItemAsync(properties).ConfigureAwait(false);
        }

        private static Search GetPropertiesFromMetaData(IDictionary<string, string> metadata)
        {
            List<bool> bools = new();
            var messageIdParsed = metadata.TryGetValue("messageId", out var messageId);
            var documentTypeParsed = metadata.TryGetValue("documentType", out var type);
            var rsmNameParsed = metadata.TryGetValue("rsmName", out var rsmName);
            var processIdParsed = metadata.TryGetValue("processId", out var processId);
            var dateTimeFromParsed = metadata.TryGetValue("dateTimeFrom", out var dateTimeFrom);
            var dateTimeToParsed = metadata.TryGetValue("dateTimeTo", out var dateTimeTo);
            var senderIdParsed = metadata.TryGetValue("senderId", out var senderId);
            var receiverIdParsed = metadata.TryGetValue("receiverId", out var receiverId);
            var businessReasonCodeParsed = metadata.TryGetValue("businessReasonCode", out var businessReasonCode);

            bools.AddRange(new List<bool>
            {
                messageIdParsed,
                documentTypeParsed,
                rsmNameParsed,
                processIdParsed,
                dateTimeFromParsed,
                dateTimeToParsed,
                senderIdParsed,
                receiverIdParsed,
                businessReasonCodeParsed,
            });

            if (bools.All(item => item is true))
            {
                return new(
                    messageId,
                    new DocumentType(type, rsmName),
                    processId,
                    DateTime.ParseExact(dateTimeFrom, "dd/MM/yyyy", null),
                    DateTime.ParseExact(dateTimeTo, "dd/MM/yyyy", null),
                    senderId,
                    receiverId,
                    businessReasonCode);
            }

            throw new ArgumentException("One or more of the provided meta data properties are not valid.");
        }

        private string GenerateFileName(string originalFilename)
        {
            var filenameArray = originalFilename.Split('.');
            var rand = new Random();
            return filenameArray[0] + rand.Next(0, 1000) + filenameArray[1];
        }
    }
}
