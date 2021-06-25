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
using Energinet.DataHub.PostOffice.Application;
using Energinet.DataHub.PostOffice.Outbound.Extensions;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;

namespace Energinet.DataHub.PostOffice.Outbound.Functions
{
    public class Peek
    {
        private readonly IDocumentStore _documentStore;

        public Peek(
            IDocumentStore documentStore)
        {
            _documentStore = documentStore;
        }

        [Function("Peek")]
        public async Task<HttpResponseData> RunAsync(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = null)] HttpRequestData request,
            FunctionContext context)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));

            var documentQuery = request.GetDocumentQuery();
            if (string.IsNullOrEmpty(documentQuery.Recipient)) return GetHttpResponse(request, HttpStatusCode.BadRequest, "Query parameter is missing 'recipient'");
            if (string.IsNullOrEmpty(documentQuery.ContainerName)) return GetHttpResponse(request, HttpStatusCode.BadRequest, "Query parameter is missing 'group'");

            var logger = context.GetLogger(nameof(Peek));
            logger.LogInformation("processing document query: {documentQuery}", documentQuery);

            var documents = await _documentStore
                .GetDocumentBundleAsync(documentQuery)
                .ConfigureAwait(false);

            if (documents.Count == 0) return request.CreateResponse(HttpStatusCode.NoContent);

            var response = request.CreateResponse(HttpStatusCode.OK);

            await response.WriteAsJsonAsync(documents).ConfigureAwait(false);

            return response;
        }

        private static HttpResponseData GetHttpResponse(HttpRequestData request, HttpStatusCode httpStatusCode, string body)
        {
            var response = request.CreateResponse(httpStatusCode);
            response.WriteString(body);
            return response;
        }
    }
}
