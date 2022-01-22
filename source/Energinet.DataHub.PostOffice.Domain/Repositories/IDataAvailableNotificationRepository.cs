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
    /// Provides access to DataAvailableNotifications.
    /// </summary>
    public interface IDataAvailableNotificationRepository
    {
        /// <summary>
        /// Gets a reader that can be used to obtain unacknowledged notifications.
        /// </summary>
        /// <param name="recipient">The market operator to get the next notification for.</param>
        /// <param name="domains">The domains the retrieved notification must belong to.</param>
        /// <returns>A reader that can be used to obtain unacknowledged notifications; otherwise null.</returns>
        Task<ICabinetReader?> GetNextUnacknowledgedAsync(MarketOperator recipient, params DomainOrigin[] domains);

        /// <summary>
        /// Saves the given notification as unacknowledged.
        /// </summary>
        /// <param name="key">The key to the cabinet where the notifications will be saved.</param>
        /// <param name="notifications">The notifications to save.</param>
        Task SaveAsync(CabinetKey key, IReadOnlyList<DataAvailableNotification> notifications);

        /// <summary>
        /// Acknowledges the specified bundle and its notifications.
        /// These acknowledged notifications will no longer be returned from <see cref="ICabinetReader" />.
        /// </summary>
        /// <param name="bundle">The bundle to acknowledge.</param>
        Task AcknowledgeAsync(Bundle bundle);
    }
}
