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
using System.Threading.Tasks;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;

namespace Energinet.DataHub.PostOffice.ServiceContracts
{
    public static class GetContractFunction
    {
        [Function("GetContract")]
        public static async Task<HttpResponseData> RunAsync(
            [Microsoft.Azure.Functions.Worker.HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "contracts/v{version:int}/{contractName}")] HttpRequestData request,
            string version,
            string contractName,
            ExecutionContext executionContext,
            FunctionContext context)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));
            if (context == null) throw new ArgumentNullException(nameof(context));

            var logger = context.GetLogger(nameof(GetContractFunction));
            logger.LogInformation("GetContract {version}/{contractName}", version, contractName);

            var protoContractService = ProtoContractServiceFactory.Create(executionContext);
            var protoFile = protoContractService.Get(version, contractName);

            if (protoFile != null)
            {
                var response = request.CreateResponse(HttpStatusCode.OK);
                response.Headers.Add("Content-Type", "application/text");
                response.WriteBytes(protoFile);
                return response;
            }
            else
            {
                var response = request.CreateResponse(HttpStatusCode.NotFound);
                await response.WriteStringAsync($"No contracts found for {version}/{contractName}").ConfigureAwait(false);
                return response;
            }
        }
    }
}
