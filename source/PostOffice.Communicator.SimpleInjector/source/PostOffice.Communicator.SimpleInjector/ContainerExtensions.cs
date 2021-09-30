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
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SimpleInjector;

namespace GreenEnergyHub.PostOffice.Communicator.SimpleInjector
{
    public static class ContainerExtensions
    {
        public static void AddPostOfficeCommunication(this Container container, string serviceBusConnectionStringConfigKey)
        {
            container.AddServiceBus(serviceBusConnectionStringConfigKey);
            container.AddApplicationServices();
        }

        private static void AddServiceBus(this Container container, string serviceBusConnectionStringConfigKey)
        {
            if (container == null)
                throw new ArgumentNullException(nameof(container));

            container.RegisterSingleton<IServiceBusClientFactory>(() =>
            {
                var configuration = container.GetService<IConfiguration>();
                var connectionString = configuration.GetConnectionString(serviceBusConnectionStringConfigKey)
                                       ?? configuration?[serviceBusConnectionStringConfigKey];

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
    }
}
