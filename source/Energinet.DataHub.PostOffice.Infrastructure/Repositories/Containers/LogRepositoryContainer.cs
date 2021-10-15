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

using Microsoft.Azure.Cosmos;

namespace Energinet.DataHub.PostOffice.Infrastructure.Repositories.Containers
{
    public class LogRepositoryContainer : ILogRepositoryContainer
    {
        private readonly CosmosClient _client;
        private readonly CosmosDatabaseConfig _cosmosConfig;

        public LogRepositoryContainer(CosmosClient client, CosmosDatabaseConfig cosmosConfig)
        {
            _client = client;
            _cosmosConfig = cosmosConfig;
        }

        public Container Container => _client.GetContainer(_cosmosConfig.LogDatabaseId, "Logs");
    }
}
