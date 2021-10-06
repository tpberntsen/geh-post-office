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

        public async Task<Bundle?> GetNextUnacknowledgedAsync(MarketOperator recipient)
        {
            var existingBundle = await _bundleRepository.GetNextUnacknowledgedAsync(recipient).ConfigureAwait(false);
            if (existingBundle != null)
                return await AskSubDomainForContentAsync(existingBundle).ConfigureAwait(false);

            var dataAvailableNotification = await _dataAvailableNotificationRepository.GetNextUnacknowledgedAsync(recipient).ConfigureAwait(false);
            if (dataAvailableNotification == null)
                return null; // No new data.

            var newBundle = await CreateNextBundleAsync(
                    dataAvailableNotification.Recipient,
                    dataAvailableNotification.Origin,
                    dataAvailableNotification.ContentType).ConfigureAwait(false);

            if (await _bundleRepository.TryAddNextUnacknowledgedAsync(newBundle).ConfigureAwait(false))
                return await AskSubDomainForContentAsync(newBundle).ConfigureAwait(false);

            // Concurrent peek in progress; response is "no new data".
            return null;
        }

        public async Task<Bundle?> GetNextUnacknowledgedAggregationsOrTimeSeriesAsync(MarketOperator recipient)
        {
            var domains = new[] { DomainOrigin.Aggregations, DomainOrigin.TimeSeries };

            foreach (var domainOrigin in domains)
            {
                var (hasData, bundle) = await GetNextUnacknowledgedAsync(recipient, domainOrigin).ConfigureAwait(false);
                if (hasData)
                {
                    return bundle;
                }
            }

            return null;
        }

        public async Task<(bool IsAcknowledged, Bundle? AcknowledgedBundle)> TryAcknowledgeAsync(MarketOperator recipient, Uuid bundleId)
        {
            var bundle = await _bundleRepository.GetNextUnacknowledgedAsync(recipient).ConfigureAwait(false);
            if (bundle == null || bundle.BundleId != bundleId)
                return (false, null);

            await _dataAvailableNotificationRepository.AcknowledgeAsync(bundle.NotificationIds).ConfigureAwait(false);
            await _bundleRepository.AcknowledgeAsync(bundle.BundleId).ConfigureAwait(false);
            return (true, bundle);
        }

        private async Task<(bool HasData, Bundle? Bundle)> GetNextUnacknowledgedAsync(MarketOperator recipient, DomainOrigin domainOrigin)
        {
            var existingBundle = await _bundleRepository.GetNextUnacknowledgedForDomainAsync(recipient, domainOrigin).ConfigureAwait(false);
            if (existingBundle != null)
                return (true, await AskSubDomainForContentAsync(existingBundle).ConfigureAwait(false));

            var dataAvailableNotification = await _dataAvailableNotificationRepository.GetNextUnacknowledgedForDomainAsync(recipient, domainOrigin).ConfigureAwait(false);
            if (dataAvailableNotification == null)
                return (false, null); // No new data.

            var newBundle = await CreateNextBundleAsync(
                dataAvailableNotification.Recipient,
                dataAvailableNotification.Origin,
                dataAvailableNotification.ContentType).ConfigureAwait(false);

            if (await _bundleRepository.TryAddNextUnacknowledgedAsync(newBundle).ConfigureAwait(false))
                return (true, await AskSubDomainForContentAsync(newBundle).ConfigureAwait(false));

            // Concurrent peek in progress; response is "no new data".
            return (true, null);
        }

        private async Task<Bundle?> AskSubDomainForContentAsync(Bundle bundle)
        {
            if (bundle.TryGetContent(out _))
                return bundle;

            var bundleContent = await _requestBundleDomainService
                .WaitForBundleContentFromSubDomainAsync(bundle)
                .ConfigureAwait(false);

            if (bundleContent == null)
                return null; // Timeout or error. Currently returned as "no new data".

            bundle.AssignContent(bundleContent);
            await _bundleRepository.SaveAsync(bundle).ConfigureAwait(false);
            return bundle;
        }

        private async Task<Bundle> CreateNextBundleAsync(
            MarketOperator recipient,
            DomainOrigin domainOrigin,
            ContentType contentType)
        {
            var maxWeight = _weightCalculatorDomainService.CalculateMaxWeight(domainOrigin);

            var dataAvailableNotifications = await _dataAvailableNotificationRepository
                .GetNextUnacknowledgedAsync(recipient, contentType, maxWeight)
                .ConfigureAwait(false);

            var notificationIds = dataAvailableNotifications
                .Select(x => x.NotificationId)
                .ToList();

            var bundleId = new Uuid(Guid.NewGuid());
            return new Bundle(
                bundleId,
                domainOrigin,
                recipient,
                notificationIds);
        }
    }
}
