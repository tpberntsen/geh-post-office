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
using System.ComponentModel.DataAnnotations;
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

        public Task<Bundle?> GetNextUnacknowledgedAsync(MarketOperator recipient, Uuid bundleId)
        {
            return GetNextUnacknowledgedForDomainsAsync(recipient, bundleId);
        }

        public Task<Bundle?> GetNextUnacknowledgedTimeSeriesAsync(MarketOperator recipient, Uuid bundleId)
        {
            return GetNextUnacknowledgedForDomainsAsync(recipient, bundleId, DomainOrigin.TimeSeries);
        }

        public Task<Bundle?> GetNextUnacknowledgedAggregationsAsync(MarketOperator recipient, Uuid bundleId)
        {
            return GetNextUnacknowledgedForDomainsAsync(recipient, bundleId, DomainOrigin.Aggregations);
        }

        public Task<Bundle?> GetNextUnacknowledgedMasterDataAsync(MarketOperator recipient, Uuid bundleId)
        {
            return GetNextUnacknowledgedForDomainsAsync(
                recipient,
                bundleId,
                DomainOrigin.MarketRoles,
                DomainOrigin.MeteringPoints,
                DomainOrigin.Charges);
        }

        public async Task<(bool CanAcknowledge, Bundle? Bundle)> CanAcknowledgeAsync(MarketOperator recipient, Uuid bundleId)
        {
            var bundle = await _bundleRepository.GetNextUnacknowledgedAsync(recipient).ConfigureAwait(false);
            return bundle != null && bundle.BundleId == bundleId
                ? (true, bundle)
                : (false, null);
        }

        public async Task AcknowledgeAsync(Bundle bundle)
        {
            if (bundle == null)
                throw new ArgumentNullException(nameof(bundle));

            await _dataAvailableNotificationRepository
                .AcknowledgeAsync(bundle.Recipient, bundle.NotificationIds)
                .ConfigureAwait(false);

            await _bundleRepository
                .AcknowledgeAsync(bundle.Recipient, bundle.BundleId)
                .ConfigureAwait(false);
        }

        private async Task<Bundle?> GetNextUnacknowledgedForDomainsAsync(MarketOperator recipient, Uuid bundleId, params DomainOrigin[] domains)
        {
            var existingBundle = await _bundleRepository.GetNextUnacknowledgedAsync(recipient, domains).ConfigureAwait(false);
            if (existingBundle != null)
            {
                if (existingBundle.BundleId != bundleId)
                    throw new ValidationException($"The provided bundleId does not match current scoped bundleId: {existingBundle.BundleId}");

                return await AskSubDomainForContentAsync(existingBundle).ConfigureAwait(false);
            }

            var dataAvailableNotification = await _dataAvailableNotificationRepository.GetNextUnacknowledgedAsync(recipient, domains).ConfigureAwait(false);
            if (dataAvailableNotification == null)
                return null;

            var newBundle = await CreateNextBundleAsync(bundleId, dataAvailableNotification)
                .ConfigureAwait(false);

            var bundleCreatedResponse = await _bundleRepository
                .TryAddNextUnacknowledgedAsync(newBundle)
                .ConfigureAwait(false);

            return bundleCreatedResponse switch
            {
                BundleCreatedResponse.Success => await AskSubDomainForContentAsync(newBundle).ConfigureAwait(false),
                BundleCreatedResponse.AnotherBundleExists => null,
                BundleCreatedResponse.BundleIdAlreadyInUse => throw new ValidationException(nameof(BundleCreatedResponse.BundleIdAlreadyInUse)),
                _ => throw new InvalidOperationException($"bundleCreatedResponse was {bundleCreatedResponse}")
            };
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

        private async Task<Bundle> CreateNextBundleAsync(Uuid bundleUuid, DataAvailableNotification source)
        {
            var recipient = source.Recipient;
            var domainOrigin = source.Origin;
            var contentType = source.ContentType;

            var maxWeight = _weightCalculatorDomainService.CalculateMaxWeight(domainOrigin);

            if (!source.SupportsBundling.Value || source.Weight >= maxWeight)
            {
                return new Bundle(
                    bundleUuid,
                    domainOrigin,
                    recipient,
                    new[] { source.NotificationId });
            }

            var dataAvailableNotifications = await _dataAvailableNotificationRepository
                .GetNextUnacknowledgedAsync(recipient, domainOrigin, contentType, maxWeight)
                .ConfigureAwait(false);

            var notificationIds = dataAvailableNotifications
                .Select(x => x.NotificationId)
                .ToList();

            return new Bundle(
                bundleUuid,
                domainOrigin,
                recipient,
                notificationIds);
        }
    }
}
