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
using Energinet.DataHub.MessageHub.Client;
using Energinet.DataHub.MessageHub.Client.Factories;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SimpleInjector;

namespace Energinet.DataHub.PostOffice.Common
{
    internal static class AzureBlobStorageRegistration
    {
        public static void AddAzureBlobStorage(this Container container)
        {
            container.RegisterSingleton<IStorageServiceClientFactory>(() =>
            {
                var configuration = container.GetService<IConfiguration>();
                var connectionString = configuration.GetConnectionStringOrSetting("BlobStorageConnectionString");

                if (string.IsNullOrWhiteSpace(connectionString))
                {
                    throw new InvalidOperationException("Please specify a valid BlobStorageConnectionString in the appSettings.json file or your Azure Functions Settings.");
                }

                return new StorageServiceClientFactory(connectionString);
            });
        }

        public static void AddAzureBlobStorageConfig(this Container container)
        {
            container.RegisterSingleton(() =>
            {
                var configuration = container.GetService<IConfiguration>();
                var containerName = configuration.GetValue<string>("BlobStorageContainerName");

                if (string.IsNullOrWhiteSpace(containerName))
                {
                    throw new InvalidOperationException("Please specify a valid BlobStorageContainerName in the appSettings.json file or your Azure Functions Settings.");
                }

                return new StorageConfig(containerName);
            });
        }
    }
}
