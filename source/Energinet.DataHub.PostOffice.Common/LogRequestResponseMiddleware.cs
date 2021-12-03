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
using System.Diagnostics;
using System.IO;
using System.Linq;
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

            var sw1 = new Stopwatch();
            sw1.Start();
            // Log request to blob
            await using var request = BuildRequest(context);
            await _logResourceService.LogRequestAsync(request, new Dictionary<string, string>()).ConfigureAwait(false);

            sw1.Stop();
            Console.WriteLine("MAGIC: " + sw1.Elapsed.Milliseconds);

            await next(context).ConfigureAwait(false);

            var (stream, metaData) = BuildResponse(context);
            await _logResourceService.LogResponseAsync(stream, metaData).ConfigureAwait(false);
        }

        private static (Stream Stream, Dictionary<string, string> MetaData) BuildResponse(FunctionContext context)
        {
            // Log response to blob
            var functionBindingsFeature = context.Features.SingleOrDefault(f => f.Key.Name == "IFunctionBindingsFeature").Value;
            if (functionBindingsFeature == null)
            {
                throw new MessageHubException("Cannot get function bindings feature");
            }

            var type = functionBindingsFeature.GetType();
            var result = type.GetProperties().Single(p => p.Name == "InvocationResult");

            if (result.GetValue(functionBindingsFeature) is HttpResponseData responseData)
            {
                return new(responseData.Body, new Dictionary<string, string>() { { "StatusCode", responseData.StatusCode.ToString() } });
            }

            return new(Stream.Null, new Dictionary<string, string>());
        }

        private static Stream BuildRequest(FunctionContext context)
        {
            var requestBody = new { Request = context.BindingContext.BindingData };
            return new MemoryStream(Encoding.UTF8.GetBytes(System.Text.Json.JsonSerializer.Serialize(requestBody)));
        }
    }
}
