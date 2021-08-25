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
    public class DequeueOrig
    {
        private readonly IDocumentStore<Domain.Document> _documentStore;

        public DequeueOrig(
            IDocumentStore<Domain.Document> documentStore)
        {
            _documentStore = documentStore;
        }

        [Function("DequeueOrig")]
        public async Task<HttpResponseData> RunAsync(
            [HttpTrigger(AuthorizationLevel.Anonymous, "delete", Route = null)] HttpRequestData request,
            FunctionContext context)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));

            var dequeueCommand = request.GetDequeueOrigCommand();
            if (string.IsNullOrEmpty(dequeueCommand.Recipient)) return GetHttpResponse(request, HttpStatusCode.BadRequest, "Request body is missing 'recipient'");
            if (string.IsNullOrEmpty(dequeueCommand.Bundle)) return GetHttpResponse(request, HttpStatusCode.BadRequest, "Request body is missing 'type'");

            var logger = context.GetLogger(nameof(DequeueOrig));
            logger.LogInformation($"processing dequeue command: {dequeueCommand}", dequeueCommand);

            var didDeleteDocuments = await _documentStore
                .DeleteDocumentsAsync(dequeueCommand)
                .ConfigureAwait(false);

            return didDeleteDocuments
                ? GetHttpResponse(request, HttpStatusCode.OK, string.Empty)
                : GetHttpResponse(request, HttpStatusCode.NotFound, string.Empty);
        }

        private static HttpResponseData GetHttpResponse(HttpRequestData request, HttpStatusCode httpStatusCode, string body)
        {
            var response = request.CreateResponse(httpStatusCode);
            response.WriteString(body);
            return response;
        }
    }
}
