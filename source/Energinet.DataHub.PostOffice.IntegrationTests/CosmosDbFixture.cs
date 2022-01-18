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

using System.Net;
using System.Threading.Tasks;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Cosmos.Scripts;
using Xunit;

namespace Energinet.DataHub.PostOffice.IntegrationTests
{
    internal sealed class CosmosDbFixture : IAsyncLifetime
    {
        public async Task InitializeAsync()
        {
            using var cosmosClient = new CosmosClient(LocalSettings.ConnectionString);

            var databaseResponse = await cosmosClient
                .CreateDatabaseIfNotExistsAsync(LocalSettings.DatabaseName)
                .ConfigureAwait(false);

            var logDatabaseResponse = await cosmosClient
                .CreateDatabaseIfNotExistsAsync(LocalSettings.LogDatabaseName)
                .ConfigureAwait(false);

            var testDatabase = databaseResponse.Database;
            var logDatabase = logDatabaseResponse.Database;

            await testDatabase
                .CreateContainerIfNotExistsAsync("dataavailable", "/partitionKey")
                .ConfigureAwait(false);

            await testDatabase
                .CreateContainerIfNotExistsAsync("dataavailable-archive", "/partitionKey")
                .ConfigureAwait(false);

            var bundlesResponse = await testDatabase
                .CreateContainerIfNotExistsAsync("bundles", "/recipient")
                .ConfigureAwait(false);

            await logDatabase
                .CreateContainerIfNotExistsAsync("Logs", "/marketOperator")
                .ConfigureAwait(false);

            var singleBundleViolationTrigger = new TriggerProperties
            {
                Id = "EnsureSingleUnacknowledgedBundle",
                TriggerOperation = TriggerOperation.Create,
                TriggerType = TriggerType.Post,
                Body = @"
function trigger() {

    var context = getContext();
    var container = context.getCollection();
    var response = context.getResponse();
    var createdItem = response.getBody();

    // Query for checking if there are other unacknowledged bundles for market operator.
    var filterQuery = `
    SELECT * FROM bundles b
    WHERE b.recipient = '${createdItem.recipient}' AND
          b.dequeued = false AND (
          b.origin = '${createdItem.origin}' OR
         ((b.origin = 'MarketRoles' OR b.origin = 'Charges') AND '${createdItem.origin}' = 'MeteringPoints') OR
         ((b.origin = 'Charges' OR b.origin = 'MeteringPoints') AND '${createdItem.origin}' = 'MarketRoles') OR
         ((b.origin = 'MarketRoles' OR b.origin = 'MeteringPoints') AND '${createdItem.origin}' = 'Charges'))`;

    var accept = container.queryDocuments(container.getSelfLink(), filterQuery, function(err, items, options)
    {
        if (err) throw err;
        if (items.length !== 0) throw 'SingleBundleViolation';
    });

    if (!accept) throw 'queryDocuments in trigger failed.';
}"
            };

            var bundles = bundlesResponse.Container;
            var scripts = bundles.Scripts;

            try
            {
                await scripts.DeleteTriggerAsync(singleBundleViolationTrigger.Id).ConfigureAwait(false);
            }
            catch (CosmosException ex) when (ex.StatusCode == HttpStatusCode.NotFound)
            {
                // Trigger not there, ignore.
            }

            await scripts.CreateTriggerAsync(singleBundleViolationTrigger).ConfigureAwait(false);
        }

        public Task DisposeAsync()
        {
            return Task.CompletedTask;
        }
    }
}
