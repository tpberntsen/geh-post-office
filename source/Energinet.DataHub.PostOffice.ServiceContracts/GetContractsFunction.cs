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
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;

namespace Energinet.DataHub.PostOffice.ServiceContracts
{
    public static class GetContractsFunction
    {
        [FunctionName("GetContracts")]
        public static async Task<IActionResult> RunAsync(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "contracts")] HttpRequest request,
            ExecutionContext context,
            ILogger logger)
        {
            if (context == null) throw new ArgumentNullException(nameof(context));

            logger.LogInformation("GetContracts {request}", request);

            var contracts = await ProtoContractServiceFactory.Create(context).GetAllAsync().ConfigureAwait(false);
            return await Task.FromResult(new OkObjectResult(contracts)).ConfigureAwait(false);
        }
    }
}
