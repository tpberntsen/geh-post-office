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
using Energinet.DataHub.Core.Logging.RequestResponseMiddleware;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SimpleInjector;

namespace Energinet.DataHub.PostOffice.Common
{
    internal static class RequestResponseLoggingStorageRegistration
    {
        public static void AddRequestResponseLoggingStorage(this Container container)
        {
            container.RegisterSingleton<IRequestResponseLogging>(() =>
            {
                var configuration = container.GetService<IConfiguration>();
                var connectionString = configuration.GetConnectionStringOrSetting("RequestResponseLogConnectionString");
                var containerName = configuration.GetConnectionStringOrSetting("RequestResponseLogContainerName");

                if (string.IsNullOrWhiteSpace(connectionString))
                {
                    throw new InvalidOperationException("Please specify a valid RequestResponseLogConnectionString in the appSettings.json file or your Azure Functions Settings.");
                }

                if (string.IsNullOrWhiteSpace(containerName))
                {
                    throw new InvalidOperationException("Please specify a valid RequestResponseLogContainerName in the appSettings.json file or your Azure Functions Settings.");
                }

                return new RequestResponseLoggingBlobStorage(connectionString, containerName);
            });
        }
    }
}
