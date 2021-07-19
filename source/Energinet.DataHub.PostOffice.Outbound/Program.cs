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
using Azure.Messaging.ServiceBus;
using Energinet.DataHub.PostOffice.Application;
using Energinet.DataHub.PostOffice.Application.GetMessage.Handlers;
using Energinet.DataHub.PostOffice.Application.GetMessage.Interfaces;
using Energinet.DataHub.PostOffice.Application.GetMessage.Queries;
using Energinet.DataHub.PostOffice.Application.Validation;
using Energinet.DataHub.PostOffice.Common;
using Energinet.DataHub.PostOffice.Infrastructure;
using Energinet.DataHub.PostOffice.Infrastructure.ContentPath;
using Energinet.DataHub.PostOffice.Infrastructure.GetMessage;
using Energinet.DataHub.PostOffice.Infrastructure.MessageReplyStorage;
using Energinet.DataHub.PostOffice.Infrastructure.Pipeline;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Energinet.DataHub.PostOffice.Outbound
{
    public static class Program
    {
        public static Task Main()
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
                    services.AddScoped(typeof(IPipelineBehavior<,>), typeof(GetMessagePipelineValidationBehavior<,>));
                    services.AddScoped(typeof(IValidator<GetMessageQuery>), typeof(GetMessageRuleSetValidator));

                    // Add Custom Services
                    services.AddScoped<IDocumentStore<Domain.DataAvailable>, CosmosDataAvailableStore>();
                    services.AddScoped<IDataAvailableStorageService, DataAvailableStorageService>();
                    services.AddScoped<IDataAvailableController, DataAvailableController>();
                    services.AddScoped<IMessageReplyStorage, MessageReplyTableStorage>();
                    services.AddScoped<ISendMessageToServiceBus, SendMessageToServiceBus>();
                    services.AddScoped<IGetPathToDataFromServiceBus, GetPathToDataFromServiceBus>();
                    services.AddScoped<IStorageService, StorageService>();

                    services.AddTransient<IGetContentPathStrategy, ContentPathFromSavedResponse>();
                    services.AddTransient<IGetContentPathStrategy, ContentPathFromSubDomain>();
                    services.AddScoped<IGetContentPathStrategyFactory, GetContentPathStrategyFactory>();

                    services.AddSingleton<ServiceBusClient>(t => new ServiceBusClient(Environment.GetEnvironmentVariable("ServiceBusConnectionString")));
                    services.AddDatabaseCosmosConfig();
                    services.AddCosmosContainerConfig();
                    services.AddCosmosClientBuilder(useBulkExecution: true);
                })
                .Build();

            return host.RunAsync();
        }
    }
}
