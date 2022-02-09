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

using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Energinet.DataHub.PostOffice.Common.Auth;
using Energinet.DataHub.PostOffice.EntryPoint.Operations.HealthCheck;
using Energinet.DataHub.PostOffice.Infrastructure;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;

namespace Energinet.DataHub.PostOffice.EntryPoint.Operations.Functions
{
    public class HealthFunction
    {
        private const string FunctionName = nameof(HealthFunction);

        private readonly CosmosDatabaseConfig _cosmosDatabaseConfig;
        private readonly ServiceBusConfig _serviceBusConfig;
        private readonly ActorDbConfig _actorDbConfig;
        private readonly IHealth _health;

        public HealthFunction(CosmosDatabaseConfig cosmosDatabaseConfig, ServiceBusConfig serviceBusConfig, ActorDbConfig actorDbConfig, IHealth health)
        {
            _cosmosDatabaseConfig = cosmosDatabaseConfig;
            _serviceBusConfig = serviceBusConfig;
            _actorDbConfig = actorDbConfig;
            _health = health;
        }

        [Function(FunctionName)]
        public async Task<HttpResponseData> RunAsync([HttpTrigger(AuthorizationLevel.Anonymous, "get")] HttpRequestData request)
        {
            Guard.ThrowIfNull(request, nameof(request));

            var cosmosConnectionString = _cosmosDatabaseConfig.ConnectionsString;
            var serviceBusConnectionString = _serviceBusConfig.DataAvailableQueueConnectionString;

            var results = await _health.NewFluentHealthVerifier
                .VerifyCosmosDatabase("MESSAGEHUB_COSMOS_DB", cosmosConnectionString, _cosmosDatabaseConfig.MessageHubDatabaseId)
                .VerifyCosmosDatabase("MESSAGEHUB_COSMOS_LOG_DB", cosmosConnectionString, _cosmosDatabaseConfig.LogDatabaseId)
                .VerifyMessageBus("MESSAGE_HUB_DATAAVAILABLE_QUEUE", serviceBusConnectionString, _serviceBusConfig.DataAvailableQueueName)
                .VerifyMessageBus("MESSAGE_HUB_DATAAVAILABLE_ARCHIVE_QUEUE", serviceBusConnectionString, _serviceBusConfig.DequeueCleanUpQueueName)
                .VerifySqlDatabase("ACTOR_DB", _actorDbConfig.ConnectionString)
                .RunInParallelAsync().ConfigureAwait(false);

            var response = request.CreateResponse();
            response.StatusCode = CalculateStatusCode(results);
            response.Body = new MemoryStream(Encoding.UTF8.GetBytes(Print(results)));
            response.Headers.Add("Content-Type", "text/plain");
            return response;
        }

        private static HttpStatusCode CalculateStatusCode(IDictionary<string, bool> healthChecks)
        {
            return healthChecks.All(x => x.Value) ? HttpStatusCode.OK : HttpStatusCode.ServiceUnavailable;
        }

        private static string Print(IDictionary<string, bool> healthChecks)
        {
            const string OKMessage = "OK";
            const string unavailableMessage = "unavailable";

            var len = healthChecks.Any() ? healthChecks.Max(x => x.Key.Length + (x.Value ? OKMessage : unavailableMessage).Length + 2) : 0;
            var grouped = healthChecks.OrderBy(x => x.Key).GroupBy(x => x.Value).ToDictionary(x => x.Key, x => string.Join("\n", x.Select(x => $"{x.Key}: {(x.Value ? OKMessage : unavailableMessage)}")));
            var headerBar = new string('=', len);
            return
                $"{(grouped.TryGetValue(false, out var f) ? $"Failed checks:\n{headerBar}\n{f}\n\n" : string.Empty)}" +
                $"{(grouped.TryGetValue(true, out var t) ? $"Succeeded checks:\n{headerBar}\n{t}" : string.Empty)}";
        }
    }
}
