using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using Azure.Messaging.ServiceBus;
using Energinet.DataHub.PostOffice.Application;
using Energinet.DataHub.PostOffice.Application.GetMessage;
using Energinet.DataHub.PostOffice.Contracts;
using Energinet.DataHub.PostOffice.Outbound.Extensions;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using Document = Energinet.DataHub.PostOffice.Domain.Document; // using Microsoft.AspNetCore.Mvc;

namespace Energinet.DataHub.PostOffice.Outbound.Functions
{
    public class GetMessage
    {
        private ICosmosService _cosmosService;
        private ISendMessageToServiceBus _sendMessageToServiceBus;
        private IGetPathToDataFromServiceBus _getPathToDataFromServiceBus;
        private IBlobStorageService _blobStorageService;
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
