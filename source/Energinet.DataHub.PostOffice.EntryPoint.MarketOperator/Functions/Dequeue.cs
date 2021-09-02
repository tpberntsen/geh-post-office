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
using Energinet.DataHub.PostOffice.Application.Commands;
using Energinet.DataHub.PostOffice.Common.Extensions;
using MediatR;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;

namespace Energinet.DataHub.PostOffice.EntryPoint.MarketOperator.Functions
{
    public sealed class Dequeue
    {
        private readonly IMediator _mediator;

        public Dequeue(IMediator mediator)
        {
            _mediator = mediator;
        }

        [Function("Dequeue")]
        public async Task<HttpResponseData> RunAsync(
            [HttpTrigger(AuthorizationLevel.Function, "delete")]
            HttpRequestData request,
            FunctionContext context)
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request));

            var logger = context.GetLogger<Dequeue>();
            var command = request.Url.ParseQuery<DequeueCommand>();

            logger.LogInformation($"Processing Dequeue query: {command}.");

            var response = await _mediator.Send(command).ConfigureAwait(false);
            return response.IsDequeued
                ? request.CreateResponse(HttpStatusCode.OK)
                : request.CreateResponse(HttpStatusCode.NotFound);
        }
    }
}
