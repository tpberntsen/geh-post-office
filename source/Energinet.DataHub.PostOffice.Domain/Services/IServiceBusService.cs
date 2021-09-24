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
    /// <summary>
    /// Handles communication with ServiceBus
    /// </summary>
    public interface IServiceBusService
    {
        /// <summary>
        /// Sends a request out on a specific SubDomain ServiceBus and returns the session to use to wait for a reply.
        /// </summary>
        /// <returns>A session <see cref="RequestDataSession"/> to use to wait for the reply</returns>
        public Task<RequestDataSession> RequestBundledDataFromSubDomainAsync(IEnumerable<Uuid> notificationIds, DomainOrigin domainOrigin);

        /// <summary>
        /// Waits for a given reply for a previous request, based on the session used, will wait 3 seconds for a reply
        /// </summary>
        /// <param name="bundleId"></param>
        /// <param name="session"></param>
        /// <param name="domainOrigin"></param>
        /// <returns><see cref="IBundleContent"/></returns>
        public Task<IBundleContent?> WaitForReplyFromSubDomainAsync(Uuid bundleId, RequestDataSession session, DomainOrigin domainOrigin);
    }
}
