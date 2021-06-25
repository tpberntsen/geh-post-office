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
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Energinet.DataHub.PostOffice.Application;
using Energinet.DataHub.PostOffice.Common;
using Energinet.DataHub.PostOffice.Inbound.GreenEnergyHub;
using Energinet.DataHub.PostOffice.Inbound.Parsing;
using Energinet.DataHub.PostOffice.Infrastructure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Energinet.DataHub.PostOffice.Inbound
{
    public class Program
    {
        private static Task Main(string[] args)
        {
            var host = new HostBuilder()
                .ConfigureAppConfiguration(configurationBuilder =>
                {
                    configurationBuilder.AddCommandLine(args);
                })
                .ConfigureFunctionsWorkerDefaults()
                .ConfigureServices(services =>
                {
                    // Add Logging
                    services.AddLogging();

                    // Add HttpClient
                    services.AddHttpClient();

                    // Add Custom Services
                    services.AddScoped<IDocumentStore, CosmosDocumentStore>();
                    services.AddScoped<InputParser>();
                    services.AddSingleton<IMapper<Contracts.Document, Domain.Document>, DocumentMapper>();
                    services.AddDatabaseCosmosConfig();
                    services.AddCosmosClientBuilder(useBulkExecution: false);

                    services.DiscoverValidation(new[] { typeof(DocumentRules).Assembly });
                })
                .Build();

            return host.RunAsync();
        }
    }
}
