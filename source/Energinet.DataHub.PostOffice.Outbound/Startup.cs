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
using System.Collections.Generic;
using Energinet.DataHub.PostOffice.Application;
using Energinet.DataHub.PostOffice.EntryPoint;
using Energinet.DataHub.PostOffice.EntryPoint.Extensions;
using Energinet.DataHub.PostOffice.Infrastructure;
using Microsoft.Azure.Cosmos.Fluent;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

[assembly: FunctionsStartup(typeof(Startup))]

namespace Energinet.DataHub.PostOffice.EntryPoint
{
    internal class Startup : FunctionsStartup
    {
        public override void Configure(IFunctionsHostBuilder builder)
        {
            builder.Services.AddScoped<IDocumentStore, CosmosDocumentStore>();

            // TODO: "CosmosConfiguration" is probably different in azure
            builder.Services.AddSingleton<CosmosConfig>(sp => new CosmosConfig
            {
                DatabaseId = "DocumentsDb",
                TypeToContainerIdMap = new Dictionary<string, string>
                {
                    { "market", "MarketDataContainer" },
                    { "timeseries", "TimeSeriesContainer" },
                },
            });

            builder.Services.AddScoped(s =>
            {
                var connectionString = "AccountEndpoint=https://localhost:8081/;AccountKey=C2y6yDjf5/R+ob0N8A7Cgv30VRDJIWEHLM+4QDU5DE2nQ9nDuVTqobD4b8mGGyPMbIZnqyMsEcaGQy67XIw/Jw=="; // Configuration["CosmosDBConnection"];
                if (string.IsNullOrEmpty(connectionString))
                {
                    throw new InvalidOperationException(
                        "Please specify a valid CosmosDBConnection in the appSettings.json file or your Azure Functions Settings.");
                }

                return new CosmosClientBuilder(connectionString)
                    .Build();
            });
        }
    }
}
