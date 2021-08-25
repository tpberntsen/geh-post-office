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

using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Energinet.DataHub.PostOffice.Outbound.Extensions;
using MediatR;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;

namespace Energinet.DataHub.PostOffice.Outbound.Functions
{
    public sealed class Dequeue
    {
        private readonly IMediator _mediator;

        public Dequeue(IMediator mediator)
        {
            _mediator = mediator;
        }

        [Function("Dequeue")]
        public async Task<HttpResponseMessage> RunAsync(
            [HttpTrigger(AuthorizationLevel.Function, "delete")]
            HttpRequestData request,
            FunctionContext context)
        {
            var logger = context.GetLogger<Dequeue>();
            var command = request.GetDequeueCommand();

            logger.LogInformation($"Processing Dequeue query: {command}.");

            var response = await _mediator.Send(command).ConfigureAwait(false);
            return response.IsDequeued
                ? new HttpResponseMessage(HttpStatusCode.OK)
                : new HttpResponseMessage(HttpStatusCode.NotFound);
        }
    }
}
