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
    public sealed class MarketOperatorDataDomainService : IMarketOperatorDataDomainService
    {
        private readonly IBundleRepository _bundleRepository;
        private readonly IDataAvailableNotificationRepository _dataAvailableNotificationRepository;
        private readonly IRequestBundleDomainService _requestBundleDomainService;
        private readonly IWeightCalculatorDomainService _weightCalculatorDomainService;

        public MarketOperatorDataDomainService(
            IBundleRepository bundleRepository,
            IDataAvailableNotificationRepository dataAvailableRepository,
            IRequestBundleDomainService requestBundleDomainService,
            IWeightCalculatorDomainService weightCalculatorDomainService)
        {
            _bundleRepository = bundleRepository;
            _dataAvailableNotificationRepository = dataAvailableRepository;
            _requestBundleDomainService = requestBundleDomainService;
            _weightCalculatorDomainService = weightCalculatorDomainService;
        }

        public async Task<IBundle?> GetNextUnacknowledgedAsync(MarketOperator recipient)
        {
            var bundle = await _bundleRepository.GetNextUnacknowledgedAsync(recipient).ConfigureAwait(false);
            if (bundle != null)
                return bundle;

            var dataAvailableNotification = await _dataAvailableNotificationRepository.GetNextUnacknowledgedAsync(recipient).ConfigureAwait(false);
            if (dataAvailableNotification == null)
                return null;

            var dataAvailableNotifications = await _dataAvailableNotificationRepository
                .GetNextUnacknowledgedAsync(
                    recipient,
                    dataAvailableNotification.ContentType,
                    _weightCalculatorDomainService.CalculateMaxWeight(dataAvailableNotification.ContentType))
                .ConfigureAwait(false);

            var subDomain = dataAvailableNotification.Origin;
            var requestSession = await
                _requestBundleDomainService.RequestBundledDataFromSubDomainAsync(
                    dataAvailableNotifications,
                    subDomain)
                    .ConfigureAwait(false);

            var replyData = await _requestBundleDomainService
                .WaitForReplyFromSubDomainAsync(requestSession, subDomain)
                .ConfigureAwait(false);

            if (!replyData.Success)
                return null;

            return await _bundleRepository
                .CreateBundleAsync(dataAvailableNotifications, replyData.UriToContent)
                .ConfigureAwait(false);
        }

        public async Task<bool> TryAcknowledgeAsync(MarketOperator recipient, Uuid bundleId)
        {
            var bundle = await _bundleRepository.GetNextUnacknowledgedAsync(recipient).ConfigureAwait(false);
            if (bundle == null || bundle.BundleId != bundleId)
                return false;

            await _dataAvailableNotificationRepository.AcknowledgeAsync(bundle.NotificationIds).ConfigureAwait(false);
            await _bundleRepository.AcknowledgeAsync(bundle.BundleId).ConfigureAwait(false);
            return true;
        }
    }
}
