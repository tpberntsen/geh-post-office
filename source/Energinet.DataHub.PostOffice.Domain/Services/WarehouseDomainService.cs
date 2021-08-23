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

using System.Threading.Tasks;
using Energinet.DataHub.PostOffice.Domain.Model;
using Energinet.DataHub.PostOffice.Domain.Repositories;

namespace Energinet.DataHub.PostOffice.Domain.Services
{
    public class WarehouseDomainService : IWarehouseDomainService
    {
        private readonly IBundleRepository _bundleRepository;
        private readonly IDataAvailableNotificationRepository _dataAvailableRepository;

        public WarehouseDomainService(IBundleRepository bundleRepository, IDataAvailableNotificationRepository dataAvailableRepository)
        {
            _bundleRepository = bundleRepository;
            _dataAvailableRepository = dataAvailableRepository;
        }

        public async Task<IBundle?> PeekAsync(Recipient recipient)
        {
            var bundle = await _bundleRepository.PeekAsync(recipient).ConfigureAwait(false);

            if (bundle != null)
                return bundle;

            var dataAvailableNotification = await _dataAvailableRepository.PeekAsync(recipient).ConfigureAwait(false);

            if (dataAvailableNotification != null)
            {
                var dataAvailableNotifications = await _dataAvailableRepository.PeekAsync(recipient, dataAvailableNotification.MessageType).ConfigureAwait(false);
                return await _bundleRepository.CreateBundleAsync(dataAvailableNotifications, recipient).ConfigureAwait(false);
            }

            return null;
        }

        public async Task DequeueAsync(Recipient recipient)
        {
            var bundle = await _bundleRepository.PeekAsync(recipient).ConfigureAwait(false);

            if (bundle == null)
                return;

            await _dataAvailableRepository.DequeueAsync(bundle.NotificationsIds).ConfigureAwait(false);
            await _bundleRepository.DequeueAsync(bundle.Id).ConfigureAwait(false);
        }
    }
}
