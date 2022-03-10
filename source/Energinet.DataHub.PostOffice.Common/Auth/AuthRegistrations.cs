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
using Energinet.DataHub.Core.App.Common.Abstractions.Identity;
using Energinet.DataHub.Core.App.Common.Identity;
using Energinet.DataHub.Core.App.Common.Security;
using Energinet.DataHub.Core.App.FunctionApp.Middleware;
using Energinet.DataHub.Core.App.FunctionApp.SimpleInjector;
using Energinet.DataHub.PostOffice.Utilities;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SimpleInjector;

namespace Energinet.DataHub.PostOffice.Common.Auth
{
    public static class HttpAuthenticationRegistrations
    {
        public static void AddHttpAuthentication(this Container container)
        {
            Guard.ThrowIfNull(container, nameof(container));

            container.Register<IMarketOperatorIdentity, MarketOperatorIdentity>(Lifestyle.Scoped);
            container.Register<JwtAuthenticationMiddleware>(Lifestyle.Scoped);
            container.Register<QueryAuthenticationMiddleware>(Lifestyle.Scoped);
            RegisterJwt(container);

            container.AddMarketParticipantConfig();
            container.AddActorContext<ActorProvider>();
        }

        public static void AddMarketParticipantConfig(this Container container)
        {
            Guard.ThrowIfNull(container, nameof(container));

            container.Register(() =>
            {
                const string connectionStringKey = "SQL_ACTOR_DB_CONNECTION_STRING";
                var connectionString = Environment.GetEnvironmentVariable(connectionStringKey) ?? throw new InvalidOperationException($"{connectionStringKey} is required");
                return new ActorDbConfig(connectionString);
            });
        }

        private static void RegisterJwt(Container container)
        {
            container.Register<JwtTokenMiddleware>(Lifestyle.Scoped);
            container.Register<IClaimsPrincipalAccessor, ClaimsPrincipalAccessor>(Lifestyle.Scoped);
            container.Register<ClaimsPrincipalContext>(Lifestyle.Scoped);
            container.Register(() =>
            {
                var configuration = container.GetService<IConfiguration>();
                var tenantId = configuration.GetValue<string>("B2C_TENANT_ID") ?? throw new InvalidOperationException("B2C tenant id not found.");
                var audience = configuration.GetValue<string>("BACKEND_SERVICE_APP_ID") ?? throw new InvalidOperationException("Backend service app id not found.");
                return new OpenIdSettings($"https://login.microsoftonline.com/{tenantId}/v2.0/.well-known/openid-configuration", audience);
            });
        }
    }
}
