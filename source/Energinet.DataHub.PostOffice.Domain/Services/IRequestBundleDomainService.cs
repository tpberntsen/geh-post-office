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
using System.Threading.Tasks;
using Energinet.DataHub.PostOffice.Domain.Model;

namespace Energinet.DataHub.PostOffice.Domain.Services
{
    /// <summary>
    /// Handles communication with sub-domains through servicebus
    /// </summary>
    public interface IRequestBundleDomainService
    {
        /// <summary>
        /// Requests data from a sub-domain through the servicebus and waits for the answer.
        /// </summary>
        /// <param name="notifications"></param>
        /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
        Task RequestDataFromSubDomainAsync(DataAvailableNotification notifications);
    }
}
