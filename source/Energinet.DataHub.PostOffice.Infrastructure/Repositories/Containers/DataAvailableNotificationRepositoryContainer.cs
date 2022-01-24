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

using System.Diagnostics.CodeAnalysis;
using Energinet.DataHub.PostOffice.Infrastructure.Repositories.Containers.CosmosClients;
using Microsoft.Azure.Cosmos;

namespace Energinet.DataHub.PostOffice.Infrastructure.Repositories.Containers
{
    public sealed class DataAvailableNotificationRepositoryContainer : IDataAvailableNotificationRepositoryContainer
    {
        private readonly CosmosClient _client;
        private readonly CosmosDatabaseConfig _cosmosDatabaseConfig;

        public DataAvailableNotificationRepositoryContainer([NotNull] ICosmosBulkClient clientProvider, CosmosDatabaseConfig cosmosDatabaseConfig)
        {
            _client = clientProvider.Client;
            _cosmosDatabaseConfig = cosmosDatabaseConfig;
        }

        public Container Catalog => _client.GetContainer(_cosmosDatabaseConfig.MessageHubDatabaseId, "catalog");
        public Container Cabinet => _client.GetContainer(_cosmosDatabaseConfig.MessageHubDatabaseId, "cabinet");
        public Container Idempotency => _client.GetContainer(_cosmosDatabaseConfig.MessageHubDatabaseId, "idempotency");
    }
}
