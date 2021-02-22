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

using Energinet.DataHub.PostOffice.Application;
using Energinet.DataHub.PostOffice.Common;
using Energinet.DataHub.PostOffice.Inbound;
using Energinet.DataHub.PostOffice.Inbound.GreenEnergyHub;
using Energinet.DataHub.PostOffice.Inbound.Parsing;
using Energinet.DataHub.PostOffice.Infrastructure;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;

[assembly: FunctionsStartup(typeof(Startup))]

namespace Energinet.DataHub.PostOffice.Inbound
{
    internal class Startup : FunctionsStartup
    {
        public override void Configure(IFunctionsHostBuilder builder)
        {
            builder.Services.AddScoped<IDocumentStore, CosmosDocumentStore>();
            builder.Services.AddScoped<InputParser>();
            builder.Services.AddSingleton<IMapper<Contracts.Document, Domain.Document>, DocumentMapper>();

            builder.Services.AddCosmosConfig();
            builder.Services.AddCosmosClientBuilder(useBulkExecution: false);

            builder.Services.DiscoverValidation(new[] { typeof(DocumentRules).Assembly });
        }
    }
}
