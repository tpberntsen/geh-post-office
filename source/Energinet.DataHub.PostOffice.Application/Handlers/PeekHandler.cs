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
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Energinet.DataHub.PostOffice.Application.Commands;
using Energinet.DataHub.PostOffice.Domain.Model;
using Energinet.DataHub.PostOffice.Domain.Model.Logging;
using Energinet.DataHub.PostOffice.Domain.Repositories;
using Energinet.DataHub.PostOffice.Domain.Services;
using Energinet.DataHub.PostOffice.Utilities;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Energinet.DataHub.PostOffice.Application.Handlers
{
    public sealed class PeekHandler :
        IRequestHandler<PeekCommand, PeekResponse>,
        IRequestHandler<PeekTimeSeriesCommand, PeekResponse>,
        IRequestHandler<PeekMasterDataCommand, PeekResponse>,
        IRequestHandler<PeekAggregationsCommand, PeekResponse>
    {
        private readonly IMarketOperatorDataDomainService _marketOperatorDataDomainService;
        private readonly ILogRepository _log;
        private readonly ILogger _logger;
        private readonly ICorrelationIdProvider _correlationIdProvider;

        public PeekHandler(
            IMarketOperatorDataDomainService marketOperatorDataDomainService,
            ILogRepository log,
            ILogger logger,
            ICorrelationIdProvider correlationIdProvider)
        {
            _marketOperatorDataDomainService = marketOperatorDataDomainService;
            _log = log;
            _logger = logger;
            _correlationIdProvider = correlationIdProvider;
        }

        public Task<PeekResponse> Handle(PeekCommand request, CancellationToken cancellationToken)
        {
            return HandleAsync(
                request,
                _marketOperatorDataDomainService.GetNextUnacknowledgedAsync,
                (processId, bundleContent) => new PeekLog(processId, bundleContent));
        }

        public Task<PeekResponse> Handle(PeekTimeSeriesCommand request, CancellationToken cancellationToken)
        {
            return HandleAsync(
                request,
                _marketOperatorDataDomainService.GetNextUnacknowledgedTimeSeriesAsync,
                (processId, bundleContent) => new PeekTimeseriesLog(processId, bundleContent));
        }

        public Task<PeekResponse> Handle(PeekMasterDataCommand request, CancellationToken cancellationToken)
        {
            return HandleAsync(
                request,
                _marketOperatorDataDomainService.GetNextUnacknowledgedMasterDataAsync,
                (processId, bundleContent) => new PeekMasterDataLog(processId, bundleContent));
        }

        public Task<PeekResponse> Handle(PeekAggregationsCommand request, CancellationToken cancellationToken)
        {
            return HandleAsync(
                request,
                _marketOperatorDataDomainService.GetNextUnacknowledgedAggregationsAsync,
                (processId, bundleContent) => new PeekAggregationsLog(processId, bundleContent));
        }

        private async Task<PeekResponse> HandleAsync(
            PeekCommandBase request,
            Func<MarketOperator, Uuid?, Task<Bundle?>> requestHandler,
            Func<ProcessId, IBundleContent, PeekLog> logProvider)
        {
            Guard.ThrowIfNull(request, nameof(request));

            _logger.LogProcess("Peek", _correlationIdProvider.CorrelationId, request.MarketOperator);

            var marketOperator = new MarketOperator(new GlobalLocationNumber(request.MarketOperator));

            var suggestedBundleId = request.BundleId != null
                ? new Uuid(request.BundleId)
                : null;

            var bundle = await requestHandler(marketOperator, suggestedBundleId).ConfigureAwait(false);

            if (bundle != null)
            {
                if (bundle.TryGetContent(out var bundleContent))
                {
                    var peekLog = logProvider(bundle.ProcessId, bundleContent);
                    await _log.SavePeekLogOccurrenceAsync(peekLog).ConfigureAwait(false);

                    _logger.LogProcess("Peek", "HasContent", _correlationIdProvider.CorrelationId, request.MarketOperator, bundle.BundleId.ToString(), bundle.NotificationIds.Select(x => x.ToString()));

                    return new PeekResponse(
                        true,
                        bundle.BundleId.ToString(),
                        await bundleContent.OpenAsync().ConfigureAwait(false),
                        bundle.DocumentTypes);
                }

                _logger.LogProcess("Peek", "TimeoutOrError", _correlationIdProvider.CorrelationId, request.MarketOperator, bundle.BundleId.ToString(), bundle.NotificationIds.Select(x => x.ToString()));
            }

            _logger.LogProcess("Peek", "NoContent", _correlationIdProvider.CorrelationId, request.MarketOperator, string.Empty);

            return new PeekResponse(
                false,
                string.Empty,
                Stream.Null,
                Enumerable.Empty<string>());
        }
    }
}
