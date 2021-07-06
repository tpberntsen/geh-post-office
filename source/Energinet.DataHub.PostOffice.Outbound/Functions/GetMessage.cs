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
using Energinet.DataHub.PostOffice.Outbound.Extensions;
using MediatR;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;

namespace Energinet.DataHub.PostOffice.Outbound.Functions
{
    public class GetMessage
    {
        private readonly IMediator _mediator;

        public GetMessage(IMediator mediator)
        {
            _mediator = mediator;
        }

        [Function("GetMessage")]
        public async Task<HttpResponseData> RunAsync(
            [HttpTrigger(AuthorizationLevel.Function, "get")] HttpRequestData request,
            FunctionContext context)
        {
            try
            {
                var getMessageQuery = request.GetDocumentQuery();

                if (string.IsNullOrEmpty(getMessageQuery.Recipient))
                {
                    return GetHttpResponse(request, HttpStatusCode.BadRequest, "Query parameter is missing 'recipient'");
                }

                var logger = context.GetLogger(nameof(GetMessage));
                logger.LogInformation($"Processing GetMessage query: {getMessageQuery}.");

                var data = await _mediator.Send(getMessageQuery).ConfigureAwait(false);

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
