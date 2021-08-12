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
using Energinet.DataHub.PostOffice.Domain.Enums;
using Energinet.DataHub.PostOffice.Domain.Exceptions;
using Energinet.DataHub.PostOffice.Outbound.Extensions;
using FluentValidation;
using MediatR;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;

namespace Energinet.DataHub.PostOffice.Outbound.Functions
{
    public class GetMessage
    {
        private readonly IMediator _mediator;
        private readonly ILogger _logger;

        public GetMessage(
            IMediator mediator,
            ILogger<GetMessage> logger)
        {
            _mediator = mediator;
            _logger = logger;
        }

        [Function("GetMessage")]
        public async Task<HttpResponseData> RunAsync(
            [HttpTrigger(AuthorizationLevel.Function, "get")] HttpRequestData request,
            FunctionContext context)
        {
            try
            {
                var getMessageQuery = request.GetMessageQuery();

                var logger = context.GetLogger(nameof(GetMessage));
                logger.LogInformation($"Processing GetMessage query: {getMessageQuery}.");

                var data = await _mediator.Send(getMessageQuery).ConfigureAwait(false);

                var response = await CreateHttpResponseAsync(request, HttpStatusCode.OK, string.IsNullOrWhiteSpace(data) ? null : data).ConfigureAwait(false);

                return response;
            }
            catch (ValidationException e)
            {
                var errorResponse = await request.CreateErrorHttpResponseAsync(HttpStatusCode.BadRequest, e.Message).ConfigureAwait(false);
                return errorResponse;
            }
            catch (MessageReplyException e)
            {
                _logger.LogError(e.Message, e);
                var httpStatusCode = ConvertMessageReplyFailureToHttpStatusCode(e.FailureReason);
                return await request.CreateErrorHttpResponseAsync(httpStatusCode, e.Message).ConfigureAwait(false);
            }
            catch (Exception e)
            {
                _logger.LogError(e.Message, e);
                throw new Exception("Exception in GetMessage.", e);
            }
        }

        private static async Task<HttpResponseData> CreateHttpResponseAsync(HttpRequestData request, HttpStatusCode httpStatusCode, string body)
        {
            var response = request.CreateResponse(httpStatusCode);
            await response.WriteAsJsonAsync(body).ConfigureAwait(false);
            return response;
        }

        private static HttpStatusCode ConvertMessageReplyFailureToHttpStatusCode(MessageReplyFailureReason? failureReason)
        {
            return failureReason switch
            {
                MessageReplyFailureReason.InternalError => HttpStatusCode.InternalServerError,
                MessageReplyFailureReason.DatasetNotAvailable => HttpStatusCode.Accepted,
                _ => HttpStatusCode.NotFound
            };
        }
    }
}
