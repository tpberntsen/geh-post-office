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

using System.Threading.Tasks;
using Energinet.DataHub.PostOffice.Domain.Model;

namespace Energinet.DataHub.PostOffice.Domain.Repositories
{
    /// <summary>
    /// Facilitates access to a cabinet filled with bundleable unacknowledged notifications.
    /// The read items are returned in the order they were written.
    /// </summary>
    public interface ICabinetReader
    {
        /// <summary>
        /// The key of the cabinet.
        /// </summary>
        CabinetKey Key { get; }

        /// <summary>
        /// Returns true if there are items left in the cabinet; otherwise, false.
        /// </summary>
        bool CanPeek { get; }

        /// <summary>
        /// Returns the next unacknowledged notification from the cabinet.
        /// Throws InvalidOperationException if there are no more items.
        /// </summary>
        /// <returns>The next unacknowledged notification.</returns>
        DataAvailableNotification Peek();

        /// <summary>
        /// Removes and returns the next unacknowledged notification from the cabinet.
        /// </summary>
        /// <returns>The removed unacknowledged notification.</returns>
        Task<DataAvailableNotification> TakeAsync();
    }
}
