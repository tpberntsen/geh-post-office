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

using System.Collections.Generic;
using System.Threading.Tasks;
using Energinet.DataHub.PostOffice.Domain.Model;

namespace Energinet.DataHub.PostOffice.Domain.Repositories
{
    /// <summary>
    /// BundleRepository
    /// </summary>
    public interface IBundleRepository
    {
        /// <summary>
        /// Peek a Bundle for a given recipient
        /// </summary>
        /// <param name="recipient"></param>
        /// <returns>Bundle</returns>
        Task<IBundle?> PeekAsync(Recipient recipient);

        /// <summary>
        /// Create a new bundle containing supplied dataAvailableNotifications
        /// </summary>
        /// <param name="dataAvailableNotifications"></param>
        /// <param name="recipient"></param>
        /// <returns>Bundle</returns>
        Task<IBundle> CreateBundleAsync(IEnumerable<DataAvailableNotification> dataAvailableNotifications, Recipient recipient);

        /// <summary>
        /// Dequeue next bundle for recipient
        /// </summary>
        /// <param name="id"></param>
        /// <returns>Bundle</returns>
        Task DequeueAsync(Uuid id);
    }
}
