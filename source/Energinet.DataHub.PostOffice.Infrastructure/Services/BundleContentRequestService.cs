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
using System.Threading.Tasks;
using Energinet.DataHub.MessageHub.Core.Peek;
using Energinet.DataHub.MessageHub.Model.Model;
using Energinet.DataHub.PostOffice.Domain.Model;
using Energinet.DataHub.PostOffice.Domain.Services;
using Energinet.DataHub.PostOffice.Infrastructure.Model;
using Energinet.DataHub.PostOffice.Utilities;
using Microsoft.Extensions.Logging;
using DomainOrigin = Energinet.DataHub.MessageHub.Model.Model.DomainOrigin;

namespace Energinet.DataHub.PostOffice.Infrastructure.Services
{
    public sealed class BundleContentRequestService : IBundleContentRequestService
    {
        private readonly ILogger _logger;
        private readonly IMarketOperatorDataStorageService _marketOperatorDataStorageService;
        private readonly IDataBundleRequestSender _dataBundleRequestSender;
        private readonly ICorrelationIdProvider _correlationIdProvider;

        public BundleContentRequestService(
            ILogger logger,
            IMarketOperatorDataStorageService marketOperatorDataStorageService,
            IDataBundleRequestSender dataBundleRequestSender,
            ICorrelationIdProvider correlationIdProvider)
        {
            _logger = logger;
            _marketOperatorDataStorageService = marketOperatorDataStorageService;
            _dataBundleRequestSender = dataBundleRequestSender;
            _correlationIdProvider = correlationIdProvider;
        }

        public async Task<IBundleContent?> WaitForBundleContentFromSubDomainAsync(Bundle bundle)
        {
            Guard.ThrowIfNull(bundle, nameof(bundle));

            var request = new DataBundleRequestDto(
                Guid.NewGuid(),
                bundle.ProcessId.ToString(),
                bundle.ProcessId.ToString(),
                bundle.ContentType.Value);

            _logger.LogProcess("Peek", "WaitForContent", _correlationIdProvider.CorrelationId, bundle.Recipient.ToString(), bundle.BundleId.ToString(), bundle.Origin.ToString());

            var response = await _dataBundleRequestSender.SendAsync(request, (DomainOrigin)bundle.Origin).ConfigureAwait(false);
            if (response == null)
            {
                _logger.LogProcess("Peek", "NoDomainResponse", _correlationIdProvider.CorrelationId, bundle.Recipient.ToString(), bundle.BundleId.ToString(), bundle.Origin.ToString());
                return null;
            }

            if (response.IsErrorResponse)
            {
                _logger.LogProcess("Peek", "DomainErrorResponse", _correlationIdProvider.CorrelationId, bundle.Recipient.ToString(), bundle.BundleId.ToString(), bundle.Origin.ToString());
                _logger.LogError(
                    "Domain returned an error {0}. Correlation ID: {1}.\nDescription: {2}",
                    response.ResponseError.Reason,
                    _correlationIdProvider.CorrelationId,
                    response.ResponseError.FailureDescription);
                return null;
            }

            _logger.LogProcess("Peek", "DomainResponse", _correlationIdProvider.CorrelationId, bundle.Recipient.ToString(), bundle.BundleId.ToString(), bundle.Origin.ToString());

            return new AzureBlobBundleContent(_marketOperatorDataStorageService, response.ContentUri);
        }
    }
}
