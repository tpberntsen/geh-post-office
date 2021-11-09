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
using Energinet.DataHub.PostOffice.Application.Commands;
using Energinet.DataHub.PostOffice.Domain.Model;
using Energinet.DataHub.PostOffice.Domain.Repositories;
using MediatR;

namespace Energinet.DataHub.PostOffice.Application.Handlers
{
    public class CleanUpDataAvailableHandler : IRequestHandler<DataAvailableCleanUpCommand, OperationResponse>
    {
        private readonly IDataAvailableNotificationRepository _dataAvailableNotificationRepository;
        private readonly IBundleRepository _bundleRepository;

        public CleanUpDataAvailableHandler(
            IDataAvailableNotificationRepository dataAvailableNotificationRepository,
            IBundleRepository bundleRepository)
        {
            _dataAvailableNotificationRepository = dataAvailableNotificationRepository;
            _bundleRepository = bundleRepository;
        }

        public async Task<OperationResponse> Handle(DataAvailableCleanUpCommand request, CancellationToken cancellationToken)
        {
            if (request is null)
                throw new ArgumentNullException(nameof(request));

            var bundle = await _bundleRepository.GetBundleAsync(request.BundleId).ConfigureAwait(false);

            if (bundle != null)
            {
                var marketOperator = new MarketOperator(new GlobalLocationNumber(bundle.Recipient.ToString()));

                await _dataAvailableNotificationRepository
                    .WriteToArchiveAsync(bundle.NotificationIds.Select(n => n)).ConfigureAwait(false);

                await _dataAvailableNotificationRepository
                    .DeleteAsync(bundle.NotificationIds.Select(n => n), marketOperator).ConfigureAwait(false);

                bundle.NotificationsArchived = true;
                await _bundleRepository.SaveAsync(bundle).ConfigureAwait(false);
            }

            return new OperationResponse(true);
        }
    }
}
