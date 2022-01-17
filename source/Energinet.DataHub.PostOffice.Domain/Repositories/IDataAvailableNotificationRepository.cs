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
        /// Saves the given notification as unacknowledged.
        /// </summary>
        /// <param name="dataAvailableNotification">The notification to save.</param>
        Task SaveAsync(DataAvailableNotification dataAvailableNotification);

        /// <summary>
        /// Gets the next unacknowledged notification for the given market operator and domains.
        /// Returns null if there are no unacknowledged notifications.
        /// </summary>
        /// <param name="recipient">The market operator to get the next notification for.</param>
        /// <param name="domains">The domains the retrieved notification must belong to.</param>
        /// <returns>The next unacknowledged notification; or null, if there are no unacknowledged notifications.</returns>
        Task<DataAvailableNotification?> GetNextUnacknowledgedAsync(MarketOperator recipient, params DomainOrigin[] domains);

        /// <summary>
        /// Gets the next ordered list of unacknowledged notifications of a specific type for the given market operator.
        /// The list is limited by maximum weight, based on the given content type.
        /// The list is empty if there are no unacknowledged notifications.
        /// </summary>
        /// <param name="recipient">The market operator to get the notifications for.</param>
        /// <param name="domainOrigin">The domain the retrieved notification must belong to.</param>
        /// <param name="contentType">The content type used to filter the notitications.</param>
        /// <param name="maxWeight">The max weight to fetch.</param>
        /// <returns>An ordered list of unacknowledged notifications for the given market operator and content type.</returns>
        Task<IEnumerable<DataAvailableNotification>> GetNextUnacknowledgedAsync(
            MarketOperator recipient,
            DomainOrigin domainOrigin,
            ContentType contentType,
            Weight maxWeight);

        /// <summary>
        /// Acknowledges the specified list of notifications, based on their ids.
        /// Acknowledged notifications are not returned from GetNextUnacknowledgedAsync.
        /// </summary>
        /// <param name="recipient">The market operator the next notifications belong to.</param>
        /// <param name="dataAvailableNotificationUuids">The list of notification ids to acknowledge.</param>
        Task AcknowledgeAsync(MarketOperator recipient, IEnumerable<Uuid> dataAvailableNotificationUuids);

        /// <summary>
        /// Acknowledges the specified bundle and its notifications.
        /// These acknowledged notifications will no longer be returned from <see cref="GetNextUnacknowledgedAsync(MarketOperator, DomainOrigin[])" />.
        /// </summary>
        /// <param name="bundle">The bundle to acknowledge.</param>
        Task AcknowledgeAsync(Bundle bundle);

        /// <summary>
        /// Writes copy of DataAvailableNotification to archive
        /// </summary>
        /// <param name="dataAvailableNotifications"></param>
        /// <param name="partitionKey"></param>
        Task WriteToArchiveAsync(IEnumerable<Uuid> dataAvailableNotifications, string partitionKey);

        /// <summary>
        /// Deletes a range of dataavailablenotification documents
        /// </summary>
        /// <param name="dataAvailableNotifications">ids to delete</param>
        /// <param name="partitionKey"></param>
        Task DeleteAsync(IEnumerable<Uuid> dataAvailableNotifications, string partitionKey);
    }
}
