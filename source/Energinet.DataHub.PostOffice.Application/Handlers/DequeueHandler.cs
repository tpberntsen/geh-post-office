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
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Azure.Messaging.ServiceBus;
using Energinet.DataHub.MessageHub.Client.Dequeue;
using Energinet.DataHub.MessageHub.Client.Model;
using Energinet.DataHub.PostOffice.Application.Commands;
using Energinet.DataHub.PostOffice.Domain.Model;
using Energinet.DataHub.PostOffice.Domain.Services;
using MediatR;
using DomainOrigin = Energinet.DataHub.MessageHub.Client.Model.DomainOrigin;

namespace Energinet.DataHub.PostOffice.Application.Handlers
{
    public class DequeueHandler : IRequestHandler<DequeueCommand, DequeueResponse>
    {
        private readonly IMarketOperatorDataDomainService _marketOperatorDataDomainService;
        private readonly IDequeueNotificationSender _dequeueNotificationSender;

        public DequeueHandler(
            IMarketOperatorDataDomainService marketOperatorDataDomainService,
            IDequeueNotificationSender dequeueNotificationSender)
        {
            _marketOperatorDataDomainService = marketOperatorDataDomainService;
            _dequeueNotificationSender = dequeueNotificationSender;
        }

        public async Task<DequeueResponse> Handle(DequeueCommand request, CancellationToken cancellationToken)
        {
            if (request is null)
                throw new ArgumentNullException(nameof(request));

            var (isDequeued, dequeuedBundle) = await _marketOperatorDataDomainService
                .TryAcknowledgeAsync(
                    new MarketOperator(new GlobalLocationNumber(request.MarketOperator)),
                    new Uuid(request.BundleId))
                .ConfigureAwait(false);

            // TODO: Should we capture an exception here, and in case one happens, what should we do?
            if (isDequeued && dequeuedBundle is not null)
            {
                try
                {
                    var dequeueNotificationDto = new DequeueNotificationDto(
                        dequeuedBundle.NotificationIds.Select(x => x.AsGuid()).ToList(),
                        new GlobalLocationNumberDto(request.MarketOperator));

                    await _dequeueNotificationSender
                        .SendAsync(dequeueNotificationDto, (DomainOrigin)dequeuedBundle.Origin)
                        .ConfigureAwait(false);
                }
                catch (ServiceBusException)
                {
                    // TODO: Currently ignored until we know what to do if this call fails.
                    // This ensures that Dequeue is working for now
                }
            }

            return new DequeueResponse(isDequeued);
        }
    }
}
