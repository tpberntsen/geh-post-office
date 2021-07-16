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
using Energinet.DataHub.PostOffice.Domain;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;

namespace Energinet.DataHub.PostOffice.Infrastructure.MessageReplyStorage
{
    public class MessageReplyTableStorage : IMessageReplyStorage
    {
        private readonly CloudTableClient _serviceClient;

        public MessageReplyTableStorage()
        {
            var connectionString = Environment.GetEnvironmentVariable("StorageAccountConnectionString");
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(connectionString);
            _serviceClient = storageAccount.CreateCloudTableClient();
        }

        public async Task<string?> GetMessageReplyAsync(string messageKey)
        {
            var cloudTable = _serviceClient.GetTableReference("MessageReply");

            TableOperation getOperation = TableOperation.Retrieve<MessageReplyTableEntity>(messageKey, messageKey);
            TableResult operationResult = await cloudTable.ExecuteAsync(getOperation).ConfigureAwait(false);
            var messageReplyTableEntity = operationResult.Result as MessageReplyTableEntity;

            return await Task.FromResult(messageReplyTableEntity?.ContentPath ?? null).ConfigureAwait(false);
        }

        public async Task SaveMessageReplyAsync(string messageKey, Uri contentUri)
        {
            if (contentUri is null) throw new ArgumentNullException(nameof(contentUri));

            var cloudTable = _serviceClient.GetTableReference("MessageReply");

            await cloudTable.CreateIfNotExistsAsync().ConfigureAwait(false);

            var messageReplyTableEntity = new MessageReplyTableEntity(messageKey, messageKey) { ContentPath = contentUri.AbsoluteUri };

            TableOperation tableOperation = TableOperation.InsertOrReplace(messageReplyTableEntity);

            var tableResult = await cloudTable.ExecuteAsync(tableOperation).ConfigureAwait(false);
        }
    }
}
