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
    /// Handles communication with sub-domains
    /// </summary>
    public interface IRequestBundleDomainService
    {
        /// <summary>
        /// Requests data from a sub-domain
        /// </summary>
        /// <param name="notifications"></param>
        /// <param name="origin"></param>
        /// <returns>A <see cref="Task"/> with a session id for this request.</returns>
        Task<RequestDataSession> RequestBundledDataFromSubDomainAsync(IEnumerable<DataAvailableNotification> notifications, DomainOrigin origin);

        /// <summary>
        /// Awaits a reply for the given session and Subdomain
        /// </summary>
        /// <param name="session">The session to wait for a reply from</param>
        /// <param name="origin">The subdomain which queue you want to wait for</param>
        /// <returns>A reply <see cref="SubDomainReply"/> indicating a success or failure, includes a path to the data if successful</returns>
        Task<SubDomainReply> WaitForReplyFromSubDomainAsync(RequestDataSession session, DomainOrigin origin);
    }
}
