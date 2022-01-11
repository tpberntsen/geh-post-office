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

using System.Threading;
using System.Threading.Tasks;
using Energinet.DataHub.MessageHub.Core.Dequeue;
using Energinet.DataHub.MessageHub.Model.Model;
using Energinet.DataHub.PostOffice.Application.Commands;
using Energinet.DataHub.PostOffice.Domain.Model;
using Energinet.DataHub.PostOffice.Domain.Model.Logging;
using Energinet.DataHub.PostOffice.Domain.Repositories;
using Energinet.DataHub.PostOffice.Domain.Services;
using Energinet.DataHub.PostOffice.Utilities;
using MediatR;
using DomainOrigin = Energinet.DataHub.MessageHub.Model.Model.DomainOrigin;

namespace Energinet.DataHub.PostOffice.Application.Handlers
{
    public class DequeueHandler : IRequestHandler<DequeueCommand, DequeueResponse>
    {
        private readonly IMarketOperatorDataDomainService _marketOperatorDataDomainService;
        private readonly IDequeueNotificationSender _dequeueNotificationSender;
        private readonly ILogRepository _log;

        public DequeueHandler(
            IMarketOperatorDataDomainService marketOperatorDataDomainService,
            IDequeueNotificationSender dequeueNotificationSender,
            ILogRepository log)
        {
            _marketOperatorDataDomainService = marketOperatorDataDomainService;
            _dequeueNotificationSender = dequeueNotificationSender;
            _log = log;
        }

        public async Task<DequeueResponse> Handle(DequeueCommand request, CancellationToken cancellationToken)
        {
            Guard.ThrowIfNull(request, nameof(request));

            var recipient = new MarketOperator(new GlobalLocationNumber(request.MarketOperator));
            var bundleId = new Uuid(request.BundleId);

            var (canAcknowledge, bundle) = await _marketOperatorDataDomainService
                .CanAcknowledgeAsync(recipient, bundleId)
                .ConfigureAwait(false);

            if (!canAcknowledge)
                return new DequeueResponse(false);

            var dequeueNotification = new DequeueNotificationDto(
                bundle!.ProcessId.ToString(),
                new GlobalLocationNumberDto(request.MarketOperator));

            await _dequeueNotificationSender
                .SendAsync(bundle.ProcessId.ToString(), dequeueNotification, (DomainOrigin)bundle.Origin)
                .ConfigureAwait(false);

            await _log
                .SaveDequeueLogOccurrenceAsync(new DequeueLog(bundle.ProcessId))
                .ConfigureAwait(false);

            await _marketOperatorDataDomainService
                .AcknowledgeAsync(bundle)
                .ConfigureAwait(false);

            return new DequeueResponse(true);
        }
    }
}
