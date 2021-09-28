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
using Energinet.DataHub.PostOffice.Infrastructure;
using GreenEnergyHub.PostOffice.Communicator.Dequeue;
using GreenEnergyHub.PostOffice.Communicator.Factories;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Cosmos.Fluent;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Energinet.DataHub.PostOffice.Common
{
    public static class ServiceCollectionExtensions
    {
        public static void AddCosmosClientBuilder(this IServiceCollection serviceCollection, bool useBulkExecution)
        {
            serviceCollection.AddScoped(serviceProvider =>
            {
                var configuration = serviceProvider.GetService<IConfiguration>();
                var connectionString = configuration.GetConnectionStringOrSetting("MESSAGES_DB_CONNECTION_STRING");

                if (string.IsNullOrEmpty(connectionString))
                {
                    throw new InvalidOperationException(
                        "Please specify a valid CosmosDBConnection in the appSettings.json file or your Azure Functions Settings.");
                }

                return new CosmosClientBuilder(connectionString)
                    .WithBulkExecution(useBulkExecution)
                    .WithSerializerOptions(new CosmosSerializationOptions { PropertyNamingPolicy = CosmosPropertyNamingPolicy.CamelCase })
                    .Build();
            });
        }

        public static void AddServiceBus(this IServiceCollection serviceCollection)
        {
            serviceCollection.AddSingleton<IServiceBusClientFactory>(serviceProvider =>
            {
                var configuration = serviceProvider.GetService<IConfiguration>();
                var connectionString = configuration.GetConnectionStringOrSetting("ServiceBusConnectionString");

                if (string.IsNullOrEmpty(connectionString))
                {
                    throw new InvalidOperationException(
                        "Please specify a valid ServiceBus in the appSettings.json file or your Azure Functions Settings.");
                }

                return new ServiceBusClientFactory(connectionString);
            });
        }

        public static void AddDatabaseCosmosConfig(this IServiceCollection serviceCollection)
        {
            serviceCollection.AddSingleton(
                serviceProvider =>
                {
                    var configuration = serviceProvider.GetService<IConfiguration>();
                    var databaseId = configuration.GetValue<string>("MESSAGES_DB_NAME");

                    return new CosmosDatabaseConfig(databaseId);
                });
        }

        public static void AddServiceBusConfig(this IServiceCollection serviceCollection)
        {
            serviceCollection.AddSingleton(serviceProvider =>
            {
                var configuration = serviceProvider.GetService<IConfiguration>();

                return new ServiceBusConfig(
                    configuration.GetValue<string>(ServiceBusConfig.DataAvailableQueueNameKey),
                    configuration.GetValue<string>(ServiceBusConfig.DataAvailableQueueConnectionStringKey));
            });
        }
    }
}
