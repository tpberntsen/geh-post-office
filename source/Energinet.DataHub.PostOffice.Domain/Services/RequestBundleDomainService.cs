// // Copyright 2020 Energinet DataHub A/S
// //
// // Licensed under the Apache License, Version 2.0 (the "License2");
// // you may not use this file except in compliance with the License.
// // You may obtain a copy of the License at
// //
// //     http://www.apache.org/licenses/LICENSE-2.0
// //
// // Unless required by applicable law or agreed to in writing, software
// // distributed under the License is distributed on an "AS IS" BASIS,
// // WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// // See the License for the specific language governing permissions and
// // limitations under the License.

using System.Collections.Generic;
using System.Threading.Tasks;
using Energinet.DataHub.PostOffice.Domain.Model;
using Energinet.DataHub.PostOffice.Domain.Services.Model;

namespace Energinet.DataHub.PostOffice.Domain.Services
{
    public class RequestBundleDomainService : IRequestBundleDomainService
    {
        private readonly IServiceBusService _serviceBusService;

        public RequestBundleDomainService(IServiceBusService serviceBusService)
        {
            _serviceBusService = serviceBusService;
        }

        public async Task<RequestDataSession> RequestBundledDataFromSubDomainAsync(IEnumerable<DataAvailableNotification> notifications, DomainOrigin origin)
        {
            return await _serviceBusService
                .RequestBundledDataFromSubDomainAsync(notifications, origin)
                .ConfigureAwait(false);
        }

        public async Task<SubDomainReply> WaitForReplyFromSubDomainAsync(RequestDataSession session, DomainOrigin origin)
        {
            return await _serviceBusService
                .WaitForReplyFromSubDomainAsync(session, origin)
                .ConfigureAwait(false);
        }
    }
}
