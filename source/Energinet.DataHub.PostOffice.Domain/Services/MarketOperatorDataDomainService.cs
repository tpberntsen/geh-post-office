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
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using Energinet.DataHub.PostOffice.Domain.Model;
using Energinet.DataHub.PostOffice.Domain.Repositories;
using Energinet.DataHub.PostOffice.Utilities;

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
            Guard.ThrowIfNull(bundle, nameof(bundle));

            await _dataAvailableNotificationRepository
                .AcknowledgeAsync(bundle)
                .ConfigureAwait(false);

            await _bundleRepository
                .AcknowledgeAsync(bundle.Recipient, bundle.BundleId)
                .ConfigureAwait(false);
        }

        private async Task<Bundle?> GetNextUnacknowledgedForDomainsAsync(MarketOperator recipient, Uuid bundleId, params DomainOrigin[] domains)
        {
            var existingBundle = await _bundleRepository
                .GetNextUnacknowledgedAsync(recipient, domains)
                .ConfigureAwait(false);

            if (existingBundle != null)
            {
                if (existingBundle.BundleId != bundleId)
                {
                    throw new ValidationException(
                        $"The specified bundle id was rejected, as the current bundle {existingBundle.BundleId} is yet to be acknowledged.");
                }

                return await AskSubDomainForContentAsync(existingBundle).ConfigureAwait(false);
            }

            var cabinetReader = await _dataAvailableNotificationRepository
                .GetNextUnacknowledgedAsync(recipient, domains)
                .ConfigureAwait(false);

            // Nothing to return.
            if (cabinetReader == null)
                return null;

            var newBundle = await CreateNextBundleAsync(bundleId, cabinetReader).ConfigureAwait(false);

            var bundleCreatedResponse = await _bundleRepository
                .TryAddNextUnacknowledgedAsync(newBundle, cabinetReader)
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

        private async Task<Bundle> CreateNextBundleAsync(Uuid bundleId, ICabinetReader cabinetReader)
        {
            var cabinetKey = cabinetReader.Key;

            var weight = new Weight(0);
            var maxWeight = _weightCalculatorDomainService.CalculateMaxWeight(cabinetKey.Origin);

            var notificationIds = new List<Uuid>();
            var documentTypes = new HashSet<string>();

            while (cabinetReader.CanPeek)
            {
                if (notificationIds.Count == 0)
                {
                    // Initial notification is always taken.
                    // If the weight is too high, a bundle is created anyway, with just this notification.
                    var notification = await cabinetReader.TakeAsync().ConfigureAwait(false);

                    weight += notification.Weight;
                    notificationIds.Add(notification.NotificationId);
                    documentTypes.Add(notification.DocumentType.Value);

                    if (!notification.SupportsBundling.Value)
                        break;
                }
                else
                {
                    var notification = cabinetReader.Peek();
                    if (notification.SupportsBundling.Value && weight + notification.Weight <= maxWeight)
                    {
                        var dequeued = await cabinetReader
                            .TakeAsync()
                            .ConfigureAwait(false);

                        weight += dequeued.Weight;
                        notificationIds.Add(dequeued.NotificationId);
                        documentTypes.Add(dequeued.DocumentType.Value);
                    }
                    else
                    {
                        break;
                    }
                }
            }

            return new Bundle(
                bundleId,
                cabinetKey.Recipient,
                cabinetKey.Origin,
                cabinetKey.ContentType,
                notificationIds,
                documentTypes);
        }
    }
}
