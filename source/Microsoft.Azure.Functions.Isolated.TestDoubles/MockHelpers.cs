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

using System.IO;
using System.Text;
using Azure.Core.Serialization;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.Azure.Functions.Isolated.TestDoubles
{
    public static class MockHelpers
    {
        public static HttpRequestData CreateHttpRequestData(
            string? payload = null,
            string? token = null,
            string method = "GET",
            string? url = null)
        {
            var input = payload ?? string.Empty;
            var functionContext = CreateContext(new NewtonsoftJsonObjectSerializer());
            var request = new MockHttpRequestData(
                functionContext,
                method: method,
                url: url,
                body: new MemoryStream(Encoding.UTF8.GetBytes(input)));
            request.Headers.Add("Content-Type", "application/json");
            if (token != null)
            {
                request.Headers.Add("Authorization", $"Bearer {token}");
            }

            return request;
        }

        private static FunctionContext CreateContext(ObjectSerializer? serializer = null)
        {
            var context = new MockFunctionContext();

            var services = new ServiceCollection();
            services.AddOptions();
            services.AddLogging();
            services.AddFunctionsWorkerCore();

            services.Configure<WorkerOptions>(c =>
            {
                c.Serializer = serializer;
            });

            context.InstanceServices = services.BuildServiceProvider();

            return context;
        }
    }
}
