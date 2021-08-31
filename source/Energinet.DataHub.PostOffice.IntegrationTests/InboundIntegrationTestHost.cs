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
using Energinet.DataHub.PostOffice.EntryPoint.SubDomain;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SimpleInjector;
using SimpleInjector.Lifestyles;

namespace Energinet.DataHub.PostOffice.IntegrationTests
{
    public sealed class InboundIntegrationTestHost : IAsyncDisposable
    {
        private const string AzureCosmosEmulatorConnectionString = "AccountEndpoint=https://localhost:8081/;AccountKey=C2y6yDjf5/R+ob0N8A7Cgv30VRDJIWEHLM+4QDU5DE2nQ9nDuVTqobD4b8mGGyPMbIZnqyMsEcaGQy67XIw/Jw==";
        private const string ServiceBusConnectionString = "Endpoint=sb://sbn-postoffice-jj.servicebus.windows.net/;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=qFI3NLuZJN7Zodv+0mULCubm0EUTFvVacbSHQRPrmyI=";

        private readonly Startup _startup;

        private InboundIntegrationTestHost()
        {
            _startup = new Startup();
        }

        public static async Task<InboundIntegrationTestHost> InitializeAsync()
        {
            await InitCosmosTestDatabaseAsync().ConfigureAwait(false);
            //Set environment variable for ServiceBus
            Environment.SetEnvironmentVariable("ServiceBusConnectionString", ServiceBusConnectionString);
            var host = new InboundIntegrationTestHost();

            var serviceCollection = new ServiceCollection();
            serviceCollection.AddSingleton<IConfiguration>(BuildConfig());
            host._startup.ConfigureServices(serviceCollection);
            serviceCollection.BuildServiceProvider().UseSimpleInjector(host._startup.Container, o => o.Container.Options.EnableAutoVerification = false);

            return host;
        }

        public Scope BeginScope()
        {
            return AsyncScopedLifestyle.BeginScope(_startup.Container);
        }

        public async ValueTask DisposeAsync()
        {
            await _startup.DisposeAsync().ConfigureAwait(false);
        }

        private static IConfigurationRoot BuildConfig()
        {
            return new ConfigurationBuilder().AddEnvironmentVariables().Build();
        }

        private static async Task InitCosmosTestDatabaseAsync()
        {
            Environment.SetEnvironmentVariable("MESSAGES_DB_NAME", "post-office");
            Environment.SetEnvironmentVariable("MESSAGES_DB_CONNECTION_STRING", AzureCosmosEmulatorConnectionString);

            using var cosmosClient = new CosmosClient(AzureCosmosEmulatorConnectionString);
            var databaseResponse = await cosmosClient
                .CreateDatabaseIfNotExistsAsync("post-office")
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
