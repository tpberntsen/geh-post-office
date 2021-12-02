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
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Energinet.DataHub.MessageHub.Model.Exceptions;
using Energinet.DataHub.PostOffice.Infrastructure.Services;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Azure.Functions.Worker.Middleware;

namespace Energinet.DataHub.PostOffice.Common
{
    public class LogRequestResponseMiddleware : IFunctionsWorkerMiddleware
    {
        private readonly LogResourceService _logResourceService;

        public LogRequestResponseMiddleware(LogResourceService logResourceService)
        {
            _logResourceService = logResourceService;
        }

        public async Task Invoke(FunctionContext context, FunctionExecutionDelegate next)
        {
            if (context == null) throw new ArgumentNullException(nameof(context));
            if (next == null) throw new ArgumentNullException(nameof(next));

            // Log request to blob
            var request = BuildRequest(context);
            await _logResourceService.LogRequestAsync(request).ConfigureAwait(false);

            await next(context).ConfigureAwait(false);

            var response = await BuildResponseAsync(context).ConfigureAwait(false);
            await _logResourceService.LogResponseAsync(response).ConfigureAwait(false);
        }

        private static async Task<string> BuildResponseAsync(FunctionContext context)
        {
            // Log response to blob
            var feature = context.Features.FirstOrDefault(f => f.Key.Name == "IFunctionBindingsFeature").Value;
            if (feature == null)
            {
                throw new MessageHubException("Cannot get function bindings feature");
            }

            var keyValuePair = context.Features.SingleOrDefault(f => f.Key.Name == "IFunctionBindingsFeature");
            var functionBindingsFeature = keyValuePair.Value;
            var type = functionBindingsFeature.GetType();
            var result = type.GetProperties().Single(p => p.Name == "InvocationResult");

            var response = string.Empty;
            if (result.GetValue(functionBindingsFeature) is HttpResponseData responseData)
            {
                using var reader = new StreamReader(responseData.Body);
                string body = await reader.ReadToEndAsync().ConfigureAwait(false);
                var responseObj = new
                {
                    Body = body,
                    responseData.StatusCode,
                };
                response = System.Text.Json.JsonSerializer.Serialize(new { Response = responseObj });
            }

            return response;
        }

        private static string BuildRequest(FunctionContext context)
        {
            var requestBody = new { Request = context.BindingContext.BindingData };
            return System.Text.Json.JsonSerializer.Serialize(requestBody);
        }
    }
}
