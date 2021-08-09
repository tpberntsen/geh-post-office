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
using System.Threading.Tasks;
using Energinet.DataHub.PostOffice.Application.GetMessage.Interfaces;
using Microsoft.Azure.Cosmos.Table;

namespace Energinet.DataHub.PostOffice.Infrastructure.MessageReplyStorage
{
    public class MessageReplyTableStorage : IMessageReplyStorage
    {
        private const string TableName = "MessageReply";
        private readonly CloudTableClient _serviceClient;

        public MessageReplyTableStorage()
        {
            var connectionString = Environment.GetEnvironmentVariable("StorageAccountConnectionString");
            var storageAccount = CloudStorageAccount.Parse(connectionString);
            _serviceClient = storageAccount.CreateCloudTableClient();
        }

        public async Task<string?> GetMessageReplyAsync(string messageKey)
        {
            var cloudTable = await InstantiateCloudTableAsync().ConfigureAwait(false);

            var getOperation = TableOperation.Retrieve<MessageReplyTableEntity>(messageKey, messageKey);
            var operationResult = await cloudTable.ExecuteAsync(getOperation).ConfigureAwait(false);
            var messageReplyTableEntity = operationResult.Result as MessageReplyTableEntity;

            return await Task.FromResult(messageReplyTableEntity?.ContentPath ?? null).ConfigureAwait(false);
        }

        public async Task SaveMessageReplyAsync(string messageKey, Uri contentUri)
        {
            if (contentUri is null) throw new ArgumentNullException(nameof(contentUri));

            var cloudTable = await InstantiateCloudTableAsync().ConfigureAwait(false);

            var messageReplyTableEntity = new MessageReplyTableEntity(messageKey, messageKey) { ContentPath = contentUri.AbsoluteUri };
            var tableOperation = TableOperation.InsertOrReplace(messageReplyTableEntity);

            var tableResult = await cloudTable.ExecuteAsync(tableOperation).ConfigureAwait(false);
            ValidateTableOperationResult(tableResult);
        }

        private static void ValidateTableOperationResult(TableResult result)
        {
            if (result.Result is null)
            {
#pragma warning disable CA2201 // Do not raise reserved exception types
                throw new Exception($"Could not save MessageReply to storage, response code: {result.HttpStatusCode}");
#pragma warning restore CA2201 // Do not raise reserved exception types
            }
        }

        private async Task<CloudTable> InstantiateCloudTableAsync()
        {
            var cloudTable = _serviceClient.GetTableReference(TableName);
            await cloudTable.CreateIfNotExistsAsync().ConfigureAwait(false);
            return cloudTable;
        }
    }
}
