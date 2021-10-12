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
using Energinet.DataHub.MessageHub.Client.Model;
using Energinet.DataHub.MessageHub.Client.Peek;
using Energinet.DataHub.PostOffice.Domain.Model;
using Energinet.DataHub.PostOffice.Domain.Services;
using Energinet.DataHub.PostOffice.Infrastructure.Model;
using DomainOrigin = Energinet.DataHub.MessageHub.Client.Model.DomainOrigin;

namespace Energinet.DataHub.PostOffice.Infrastructure.Services
{
    public sealed class BundleContentRequestService : IBundleContentRequestService
    {
        private readonly IMarketOperatorDataStorageService _marketOperatorDataStorageService;
        private readonly IDataBundleRequestSender _dataBundleRequestSender;

        public BundleContentRequestService(
            IMarketOperatorDataStorageService marketOperatorDataStorageService,
            IDataBundleRequestSender dataBundleRequestSender)
        {
            _marketOperatorDataStorageService = marketOperatorDataStorageService;
            _dataBundleRequestSender = dataBundleRequestSender;
        }

        public async Task<IBundleContent?> WaitForBundleContentFromSubDomainAsync(Bundle bundle)
        {
            if (bundle == null)
                throw new ArgumentNullException(nameof(bundle));

            var request = new DataBundleRequestDto(
                bundle.ProcessId.ToString(),
                bundle.NotificationIds.Select(x => x.AsGuid()));

            var response = await _dataBundleRequestSender.SendAsync(request, (DomainOrigin)bundle.Origin).ConfigureAwait(false);
            if (response == null || response.IsErrorResponse)
                return null;

            return new AzureBlobBundleContent(_marketOperatorDataStorageService, response.ContentUri);
        }
    }
}
