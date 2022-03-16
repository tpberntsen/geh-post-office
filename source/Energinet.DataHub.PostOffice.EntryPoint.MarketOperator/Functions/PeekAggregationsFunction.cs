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
using System.Net.Mime;
using System.Threading.Tasks;
using Energinet.DataHub.PostOffice.Application.Commands;
using Energinet.DataHub.PostOffice.Common.Auth;
using Energinet.DataHub.PostOffice.Common.Extensions;
using Energinet.DataHub.PostOffice.Domain.Services;
using Energinet.DataHub.PostOffice.Utilities;
using MediatR;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;

namespace Energinet.DataHub.PostOffice.EntryPoint.MarketOperator.Functions
{
    public sealed class PeekAggregationsFunction
    {
        private readonly IMediator _mediator;
        private readonly IMarketOperatorIdentity _operatorIdentity;
        private readonly ICorrelationIdProvider _correlationIdProvider;
        private readonly IFeatureFlags _featureFlags;
        private readonly ExternalBundleIdProvider _bundleIdProvider;

        public PeekAggregationsFunction(
            IMediator mediator,
            IMarketOperatorIdentity operatorIdentity,
            ICorrelationIdProvider correlationIdProvider,
            IFeatureFlags featureFlags,
            ExternalBundleIdProvider bundleIdProvider)
        {
            _mediator = mediator;
            _operatorIdentity = operatorIdentity;
            _correlationIdProvider = correlationIdProvider;
            _featureFlags = featureFlags;
            _bundleIdProvider = bundleIdProvider;
        }

        [Function("PeekAggregations")]
        public Task<HttpResponseData> RunAsync(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "peek/aggregations")]
            HttpRequestData request)
        {
            return request.ProcessAsync(async () =>
            {
                var command = new PeekAggregationsCommand(_operatorIdentity.Gln, _bundleIdProvider.TryGetBundleId(request));
                var (hasContent, bundleId, stream, documentTypes) = await _mediator
                    .Send(command)
                    .ConfigureAwait(false);

                var response = hasContent
                    ? request.CreateResponse(stream, MediaTypeNames.Application.Xml)
                    : request.CreateResponse(HttpStatusCode.NoContent);

                response.Headers.Add(Constants.BundleIdHeaderName, bundleId);
                response.Headers.Add(Constants.CorrelationIdHeaderName, _correlationIdProvider.CorrelationId);

                if (_featureFlags.IsFeatureActive(Feature.SendMessageTypeHeader))
                {
                    response.Headers.Add(Constants.MessageTypeName, string.Join(",", documentTypes));
                }

                return response;
            });
        }
    }
}
