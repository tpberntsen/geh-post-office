using System;
using System.Threading.Tasks;
using Azure.Storage.Blobs;
using Energinet.DataHub.Logging.SearchOptimizer.Models;
using Energinet.DataHub.Logging.SearchOptimizer.Storage;
using Logging.Models;
using Logging.Storage;
using Microsoft.Azure.Cosmos.Fluent;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Logging
{
    public static class Program
    {
        public static async Task Main()
        {
            var host = new HostBuilder()
                .ConfigureAppConfiguration(builder =>
                {
                    builder.AddEnvironmentVariables();
                })
                .ConfigureFunctionsWorkerDefaults()
                .ConfigureServices(services =>
                {
                    services.AddLogging();

                    var databaseId = Environment.GetEnvironmentVariable("CosmosDatabaseId");
                    var containerId = Environment.GetEnvironmentVariable("CosmosContainerId");

                    services.AddScoped(_ => new CosmosConfig(databaseId!, containerId!));
                    services.AddSingleton<ICosmosClientProvider>(_ => GetCosmosClientProvider());

                    var longTermStorageConnectionString =
                        Environment.GetEnvironmentVariable("BlobStoreConnectionString");

                    services.AddSingleton<ILongTermBlobServiceClient>(_ =>
                        new FileStoreClientProvider(new BlobServiceClient(longTermStorageConnectionString)));
                })
                .Build();

            await host.RunAsync().ConfigureAwait(false);
        }

        private static CosmosClientProvider GetCosmosClientProvider()
        {
            var connectionString = Environment.GetEnvironmentVariable("CosmosConnectionString");
            if (string.IsNullOrEmpty(connectionString))
            {
                throw new InvalidOperationException("Please specify a connection string for the Cosmos client");
            }

            var cosmosClient = new CosmosClientBuilder(connectionString)
                .WithBulkExecution(false)
                .Build();

            return new CosmosClientProvider(cosmosClient);
        }
    }
}
