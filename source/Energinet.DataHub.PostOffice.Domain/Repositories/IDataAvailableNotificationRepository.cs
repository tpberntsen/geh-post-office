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
    /// Repository for DataAvailableNotifications
    /// </summary>
    public interface IDataAvailableNotificationRepository
    {
        /// <summary>
        /// Create new DataAvailableNotification
        /// </summary>
        /// <param name="dataAvailableNotification"></param>
        Task CreateAsync(DataAvailableNotification dataAvailableNotification);

        /// <summary>
        /// Peek notifications of a specific type for the recipient
        /// </summary>
        /// <param name="recipient"></param>
        /// <param name="messageType"></param>
        /// <returns>IEnumerable of DataAvailNotification</returns>
        Task<IEnumerable<DataAvailableNotification>> PeekAsync(Recipient recipient, MessageType messageType);

        /// <summary>
        /// Peek top DataAvailableNotification for recipient
        /// </summary>
        /// <param name="recipient"></param>
        /// <returns>Notification</returns>
        Task<DataAvailableNotification?> PeekAsync(Recipient recipient);

        /// <summary>
        /// Dequeue notifications
        /// </summary>
        /// <param name="dataAvailableNotificationUuids"></param>
        Task DequeueAsync(IEnumerable<Uuid> dataAvailableNotificationUuids);
    }
}
