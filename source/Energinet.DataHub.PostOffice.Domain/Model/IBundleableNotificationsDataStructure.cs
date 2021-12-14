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

using System.Dynamic;
using System.IO;
using System.Threading.Tasks;

namespace Energinet.DataHub.PostOffice.Domain.Model
{
    /// <summary>
    /// Queue that contains similar data available-notifications
    /// </summary>
    public interface IBundleableNotificationsDataStructure
    {
        /// <summary>
        /// Is the queue ready to be peeked
        /// </summary>
        bool CanPeek { get; set; }

        /// <summary>
        /// Key to identify a partition for bundled data available-notifications
        /// </summary>
        BundleableNotificationsKey BundleableNotificationsKey { get; set; }

        /// <summary>
        /// Get the next bundle of unacknowledged data for a given market operator.
        /// Returns null when there is no new unacknowledged data to get.
        /// </summary>
        /// <param name="recipient">The market operator to receive data</param>
        /// <param name="origins">Domains from where the data is collected</param>
        /// <returns>A stream that contains data for a market operator</returns>
        Task<Stream> PeekAsync(MarketOperator recipient, DomainOrigin[] origins);

        /// <summary>
        /// Dequeues the current bundle from the queue
        /// </summary>
        /// <param name="recipient">The market operator to receive data</param>
        /// <param name="bundleId">Identifier which point to the current bundle</param>
        Task DequeueAsync(MarketOperator recipient, Uuid bundleId);
    }
}
