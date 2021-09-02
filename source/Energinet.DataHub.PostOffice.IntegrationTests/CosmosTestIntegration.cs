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
using Microsoft.Azure.Cosmos;

namespace Energinet.DataHub.PostOffice.IntegrationTests
{
    internal static class CosmosTestIntegration
    {
        private const string AzureCosmosDatabaseName = "post-office";
        private const string AzureCosmosEmulatorConnectionString = "AccountEndpoint=https://localhost:8081/;AccountKey=C2y6yDjf5/R+ob0N8A7Cgv30VRDJIWEHLM+4QDU5DE2nQ9nDuVTqobD4b8mGGyPMbIZnqyMsEcaGQy67XIw/Jw==";

        public static async Task InitializeAsync()
        {
            Environment.SetEnvironmentVariable("MESSAGES_DB_NAME", AzureCosmosDatabaseName);
            Environment.SetEnvironmentVariable("MESSAGES_DB_CONNECTION_STRING", AzureCosmosEmulatorConnectionString);

            using var cosmosClient = new CosmosClient(AzureCosmosEmulatorConnectionString);

            var databaseResponse = await cosmosClient
                .CreateDatabaseIfNotExistsAsync(AzureCosmosDatabaseName)
                .ConfigureAwait(true);

            var testDatabase = databaseResponse.Database;

            await testDatabase
                .CreateContainerIfNotExistsAsync("dataavailable", "/recipient")
                .ConfigureAwait(true);

            await testDatabase
                .CreateContainerIfNotExistsAsync("bundles", "/pk")
                .ConfigureAwait(true);
        }
    }
}
