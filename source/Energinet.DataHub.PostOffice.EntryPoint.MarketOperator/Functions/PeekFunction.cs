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
using System.Threading.Tasks;
using Energinet.DataHub.PostOffice.Application.Commands;
using Energinet.DataHub.PostOffice.Common.Auth;
using Energinet.DataHub.PostOffice.Common.Extensions;
using Energinet.DataHub.PostOffice.Infrastructure.Correlation;
using MediatR;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;

namespace Energinet.DataHub.PostOffice.EntryPoint.MarketOperator.Functions
{
    public sealed class PeekFunction
    {
        private const string BundleIdQueryName = "bundleId";

        private readonly IMediator _mediator;
        private readonly IMarketOperatorIdentity _operatorIdentity;
        private readonly ILogCallback _logCallback;

        public PeekFunction(IMediator mediator, IMarketOperatorIdentity operatorIdentity, ILogCallback logCallback)
        {
            _mediator = mediator;
            _operatorIdentity = operatorIdentity;
            _logCallback = logCallback;
        }

        [Function("Peek")]
        public Task<HttpResponseData> RunAsync(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get")]
            HttpRequestData request)
        {
            var logger = request.FunctionContext.GetLogger("PeekFunctionPerfTest");
            _logCallback.SetCallback(x => logger.LogWarning(x));

            return request.ProcessAsync(async () =>
            {
                var command = new PeekCommand(_operatorIdentity.Gln, request.Url.GetQueryValue(BundleIdQueryName));
                var (hasContent, stream) = await _mediator.Send(command).ConfigureAwait(false);
                return hasContent
                    ? request.CreateResponse(stream)
                    : request.CreateResponse(HttpStatusCode.NoContent);
            });
        }
    }
}
