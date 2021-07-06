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

using System.Threading.Tasks;
using Azure.Messaging.ServiceBus;
using Energinet.DataHub.PostOffice.Application;
using Energinet.DataHub.PostOffice.Application.GetMessage;
using Energinet.DataHub.PostOffice.Common;
using Energinet.DataHub.PostOffice.Infrastructure;
using Energinet.DataHub.PostOffice.Infrastructure.GetMessage;
using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Energinet.DataHub.PostOffice.Outbound
{
    public static class Program
    {
        public static Task Main(string[] args)
        {
            var host = new HostBuilder()
                .ConfigureAppConfiguration(configurationBuilder =>
                {
                    configurationBuilder.AddEnvironmentVariables();
                })
                .ConfigureFunctionsWorkerDefaults()
                .ConfigureServices(services =>
                {
                    // Add Logging
                    services.AddLogging();

                    // Add MediatR
                    services.AddMediatR(typeof(GetMessageHandler).Assembly);
                    services.AddScoped(typeof(IRequest<string>), typeof(GetMessageHandler));

                    // Add Custom Services
                    services.AddScoped<IDocumentStore<Domain.DataAvailable>, CosmosDataAvailableStore>();
                    services.AddScoped<ICosmosService, CosmosService>();
                    services.AddScoped<ISendMessageToServiceBus, SendMessageToServiceBus>();
                    services.AddScoped<IGetPathToDataFromServiceBus, GetPathToDataFromServiceBus>();
                    services.AddScoped<IBlobStorageService, BlobStorageService>();
                    services.AddScoped<ServiceBusClient>(t => new ServiceBusClient("Endpoint=sb://sbn-inbound-postoffice-endk-u.servicebus.windows.net/;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=YMwhKlYdf2hXZ+ufhk/EZ42kYh6RyJzeHxTPt+Stwc0="));
                    services.AddDatabaseCosmosConfig();
                    services.AddCosmosContainerConfig();
                    services.AddCosmosClientBuilder(useBulkExecution: true);
                })
                .Build();

            return host.RunAsync();
        }
    }
}
