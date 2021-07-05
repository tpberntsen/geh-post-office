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
using Energinet.DataHub.PostOffice.Application.GetMessage;
using Energinet.DataHub.PostOffice.Outbound.Extensions;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;

namespace Energinet.DataHub.PostOffice.Outbound.Functions
{
    public class GetMessage
    {
        private readonly ICosmosService _cosmosService;
        private readonly ISendMessageToServiceBus _sendMessageToServiceBus;
        private readonly IGetPathToDataFromServiceBus _getPathToDataFromServiceBus;
        private readonly IBlobStorageService _blobStorageService;
        private Guid _sessionId;

        public GetMessage(
            ICosmosService cosmosService,
            ISendMessageToServiceBus sendMessageToServiceBus,
            IGetPathToDataFromServiceBus getPathToDataFromServiceBus,
            IBlobStorageService blobStorageService)
        {
            _cosmosService = cosmosService;
            _sendMessageToServiceBus = sendMessageToServiceBus;
            _getPathToDataFromServiceBus = getPathToDataFromServiceBus;
            _blobStorageService = blobStorageService;
            _sessionId = Guid.NewGuid();
        }

        [Function("GetMessage")]
        public async Task<HttpResponseData> RunAsync(
            [HttpTrigger(AuthorizationLevel.Function, "get")] HttpRequestData request,
            FunctionContext context)
        {
            try
            {
                var documentQuery = request.GetDocumentQuery();

                if (string.IsNullOrEmpty(documentQuery.Recipient))
                {
                    return GetHttpResponse(request, HttpStatusCode.BadRequest, "Query parameter is missing 'recipient'");
                }

                if (string.IsNullOrEmpty(documentQuery.ContainerName))
                {
                    return GetHttpResponse(request, HttpStatusCode.BadRequest, "Query parameter is missing 'group'");
                }

                var logger = context.GetLogger(nameof(GetMessage));
                logger.LogInformation($"Processing document query: {documentQuery}.");

                var uuids = _cosmosService.GetUuidsFromCosmosDatabaseAsync(documentQuery.Recipient, documentQuery.ContainerName);

                await _sendMessageToServiceBus.SendMessageAsync(
                    await uuids.ConfigureAwait(false),
                    documentQuery.ContainerName,
                    _sessionId.ToString()).ConfigureAwait(false);

                var path = _getPathToDataFromServiceBus.GetPathAsync(
                    documentQuery.ContainerName,
                    _sessionId.ToString());

                var data =
                    await _blobStorageService.GetBlobAsync(
                        "test-blobstorage",
                        await path.ConfigureAwait(false)).ConfigureAwait(false);

                var response = request.CreateResponse(HttpStatusCode.OK);

                await response.WriteAsJsonAsync(data).ConfigureAwait(false);

                return response;
            }
            catch (Exception e)
            {
                throw new InvalidOperationException("Something went wrong... Baaah!", e);
            }
        }

        private static HttpResponseData GetHttpResponse(HttpRequestData request, HttpStatusCode httpStatusCode, string body)
        {
            var response = request.CreateResponse(httpStatusCode);
            response.WriteString(body);
            return response;
        }
    }
}
