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
using Energinet.DataHub.PostOffice.Application.Commands;
using Energinet.DataHub.PostOffice.Domain.Model;
using Energinet.DataHub.PostOffice.Domain.Repositories;
using Energinet.DataHub.PostOffice.Utilities;
using MediatR;

namespace Energinet.DataHub.PostOffice.Application.Handlers
{
    public class DequeueCleanUpHandler : IRequestHandler<DequeueCleanUpCommand, OperationResponse>
    {
        private readonly IDataAvailableNotificationRepository _dataAvailableNotificationRepository;
        private readonly IBundleRepository _bundleRepository;

        public DequeueCleanUpHandler(
            IDataAvailableNotificationRepository dataAvailableNotificationRepository,
            IBundleRepository bundleRepository)
        {
            _dataAvailableNotificationRepository = dataAvailableNotificationRepository;
            _bundleRepository = bundleRepository;
        }

        public async Task<OperationResponse> Handle(DequeueCleanUpCommand request, CancellationToken cancellationToken)
        {
            Guard.ThrowIfNull(request, nameof(request));

            var bundleUuid = new Uuid(request.BundleId);
            var marketOperator = new MarketOperator(new GlobalLocationNumber(request.MarketOperator));

            var bundle = await _bundleRepository.GetBundleAsync(bundleUuid, marketOperator).ConfigureAwait(false);
            if (bundle is { NotificationsArchived: false })
            {
                var partitionKey = bundle.Recipient.Gln.Value + bundle.Origin + bundle.ContentType.Value;

                await _dataAvailableNotificationRepository
                    .WriteToArchiveAsync(bundle.NotificationIds, partitionKey).ConfigureAwait(false);

                await _dataAvailableNotificationRepository
                    .DeleteAsync(bundle.NotificationIds, partitionKey).ConfigureAwait(false);

                // TODO: We must use Patch here and not overwrite document, as then we can overwrite Dequeue state.
                // For now, I am disabling cleanup before dequeue.
                bundle.ArchiveNotifications();
                await _bundleRepository.SaveAsync(bundle).ConfigureAwait(false);
            }

            return new OperationResponse(true);
        }
    }
}
