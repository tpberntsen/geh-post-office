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
using Azure.Messaging.ServiceBus;
using Azure.Storage.Blobs;
using Energinet.DataHub.MessageHub.Client.Dequeue;
using Energinet.DataHub.MessageHub.Client.Factories;
using Energinet.DataHub.MessageHub.Client.Peek;
using Energinet.DataHub.MessageHub.Client.Storage;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace GetMessage
{
    public static class Program
    {
        public static void Main()
        {
            var host = new HostBuilder()
                .ConfigureAppConfiguration(configurationBuilder =>
                {
                    configurationBuilder.AddEnvironmentVariables();
                })
                .ConfigureFunctionsWorkerDefaults()
                .ConfigureServices(services =>
                {
                    // Add logging
                    services.AddLogging();

                    var serviceBusConnectionString = Environment.GetEnvironmentVariable("ServiceBusConnectionString");
                    var blobStorageConnectionString = Environment.GetEnvironmentVariable("BlobStorageConnectionString");

                    // Add custom services
                    services.AddSingleton<ServiceBusClient>(_ =>
                        new ServiceBusClient(serviceBusConnectionString));
                    services.AddSingleton<BlobServiceClient>(_ =>
                        new BlobServiceClient(blobStorageConnectionString));
                    services.AddSingleton<IStorageHandler, StorageHandler>();

                    services.AddScoped<IRequestBundleParser>(_ => new RequestBundleParser());
                    services.AddScoped<IDataBundleResponseSender>(_ => new DataBundleResponseSender(new ResponseBundleParser(), new ServiceBusClientFactory(serviceBusConnectionString)));
                    services.AddSingleton<IStorageServiceClientFactory>(_ =>
                        new StorageServiceClientFactory(blobStorageConnectionString));

                    services.AddScoped(typeof(IDequeueNotificationParser), typeof(DequeueNotificationParser));
                })
                .Build();

            host.Run();
        }
    }
}
