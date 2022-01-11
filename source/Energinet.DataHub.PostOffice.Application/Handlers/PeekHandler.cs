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
using System.Threading;
using System.Threading.Tasks;
using Energinet.DataHub.PostOffice.Application.Commands;
using Energinet.DataHub.PostOffice.Domain.Model;
using Energinet.DataHub.PostOffice.Domain.Model.Logging;
using Energinet.DataHub.PostOffice.Domain.Repositories;
using Energinet.DataHub.PostOffice.Domain.Services;
using Energinet.DataHub.PostOffice.Utilities;
using MediatR;

namespace Energinet.DataHub.PostOffice.Application.Handlers
{
    public class PeekHandler :
        IRequestHandler<PeekCommand, PeekResponse>,
        IRequestHandler<PeekTimeSeriesCommand, PeekResponse>,
        IRequestHandler<PeekMasterDataCommand, PeekResponse>,
        IRequestHandler<PeekAggregationsCommand, PeekResponse>
    {
        private readonly IMarketOperatorDataDomainService _marketOperatorDataDomainService;
        private readonly ILogRepository _log;

        public PeekHandler(
            IMarketOperatorDataDomainService marketOperatorDataDomainService,
            ILogRepository log)
        {
            _marketOperatorDataDomainService = marketOperatorDataDomainService;
            _log = log;
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
            Func<MarketOperator, Uuid, Task<Bundle?>> requestHandler,
            Func<ProcessId, IBundleContent, PeekLog> logProvider)
        {
            Guard.ThrowIfNull(request, nameof(request));

            var marketOperator = new MarketOperator(new GlobalLocationNumber(request.MarketOperator));
            var uuid = new Uuid(request.BundleId);
            var bundle = await requestHandler(marketOperator, uuid).ConfigureAwait(false);

            if (bundle != null && bundle.TryGetContent(out var bundleContent))
            {
                var peekLog = logProvider(bundle.ProcessId, bundleContent);
                await _log.SavePeekLogOccurrenceAsync(peekLog).ConfigureAwait(false);

                return new PeekResponse(true, await bundleContent.OpenAsync().ConfigureAwait(false));
            }

            return new PeekResponse(false, Stream.Null);
        }
    }
}
