﻿// Copyright 2020 Energinet DataHub A/S
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
        /// Gets the next ordered list of unacknowledged notifications of a specific type for the given market operator.
        /// The list is limited by maximum weight, based on the given content type.
        /// The list is empty if there are no unacknowledged notifications.
        /// </summary>
        /// <param name="recipient">The market operator to get the notifications for.</param>
        /// <param name="contentType">The content type used to filter the notitications.</param>
        /// <returns>An ordered list of unacknowledged notifications for the given market operator and content type.</returns>
        Task<IEnumerable<DataAvailableNotification>> GetNextUnacknowledgedAsync(MarketOperator recipient, ContentType contentType);

        /// <summary>
        /// Gets the next unacknowledged notification for the given market operator.
        /// Returns null, if there are no unacknowledged notifications.
        /// </summary>
        /// <param name="recipient">The market operator to get the next notification for.</param>
        /// <returns>The next unacknowledged notification; or null, if there are no unacknowledged notifications.</returns>
        Task<DataAvailableNotification?> GetNextUnacknowledgedAsync(MarketOperator recipient);

        /// <summary>
        /// Acknowledges the specified list of notifications, based on their ids.
        /// Acknowledged notifications are not returned from GetNextUnacknowledgedAsync.
        /// </summary>
        /// <param name="dataAvailableNotificationUuids">The list of notification ids to acknowledge.</param>
        Task AcknowledgeAsync(IEnumerable<Uuid> dataAvailableNotificationUuids);
    }
}
