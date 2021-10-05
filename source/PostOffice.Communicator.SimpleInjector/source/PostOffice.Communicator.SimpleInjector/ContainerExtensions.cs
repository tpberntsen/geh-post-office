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
using GreenEnergyHub.PostOffice.Communicator.DataAvailable;
using GreenEnergyHub.PostOffice.Communicator.Dequeue;
using GreenEnergyHub.PostOffice.Communicator.Factories;
using GreenEnergyHub.PostOffice.Communicator.Peek;
using GreenEnergyHub.PostOffice.Communicator.Storage;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SimpleInjector;

namespace GreenEnergyHub.PostOffice.Communicator.SimpleInjector
{
    public static class ContainerExtensions
    {
        public static void AddPostOfficeCommunication(this Container container, string serviceBusConnectionStringConfigKey, string storageServiceConnectionStringConfigKey)
        {
            container.AddServiceBus(serviceBusConnectionStringConfigKey);
            container.AddApplicationServices();
            container.AddStorageHandler(storageServiceConnectionStringConfigKey);
        }

        private static void AddServiceBus(this Container container, string serviceBusConnectionStringConfigKey)
        {
            if (container == null)
                throw new ArgumentNullException(nameof(container));

            container.RegisterSingleton<IServiceBusClientFactory>(() =>
            {
                var connectionString = GetConnectionString(container, serviceBusConnectionStringConfigKey);

                if (string.IsNullOrEmpty(connectionString))
                {
                    throw new InvalidOperationException(
                        "Please specify a valid ServiceBus in the appSettings.json file or your Azure Functions Settings.");
                }

                return new ServiceBusClientFactory(connectionString);
            });
        }

        private static void AddApplicationServices(this Container container)
        {
            if (container == null)
                throw new ArgumentNullException(nameof(container));

            container.Register<IDataAvailableNotificationSender, DataAvailableNotificationSender>(Lifestyle.Singleton);
            container.Register<IRequestBundleParser, RequestBundleParser>(Lifestyle.Singleton);
            container.Register<IResponseBundleParser, ResponseBundleParser>(Lifestyle.Singleton);
            container.Register<IDataBundleResponseSender, DataBundleResponseSender>(Lifestyle.Singleton);
            container.Register<IDequeueNotificationParser, DequeueNotificationParser>(Lifestyle.Singleton);
        }

        private static void AddStorageHandler(this Container container, string storageServiceConnectionStringConfigKey)
        {
            if (container == null)
                throw new ArgumentNullException(nameof(container));

            container.RegisterSingleton<IStorageServiceClientFactory>(() =>
            {
                var connectionString = GetConnectionString(container, storageServiceConnectionStringConfigKey);

                if (string.IsNullOrEmpty(connectionString))
                {
                    throw new InvalidOperationException(
                        "Please specify a valid BlobStorageConnectionString in the appSettings.json file or your Azure Functions Settings.");
                }

                return new StorageServiceClientFactory(connectionString);
            });

            container.Register<IStorageHandler, StorageHandler>(Lifestyle.Singleton);
        }

        private static string? GetConnectionString(Container container, string serviceConnectionStringConfigKey)
        {
            var configuration = container.GetService<IConfiguration>();
            var connectionString = configuration.GetConnectionString(serviceConnectionStringConfigKey)
                                   ?? configuration?[serviceConnectionStringConfigKey];
            return connectionString;
        }
    }
}
