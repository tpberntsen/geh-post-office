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

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Energinet.DataHub.PostOffice.Domain.Model;

namespace Energinet.DataHub.PostOffice.Domain.Repositories
{
    /// <summary>
    /// Provides access to the bundles.
    /// </summary>
    public interface IBundleRepository
    {
        /// <summary>
        /// Gets the next bundle from the recipient that has yet to be acknowledged.
        /// </summary>
        /// <param name="recipient">The market operator to retrieve the next bundle for.</param>
        /// <returns>The next unacknowledged bundle; or null, if none is available.</returns>
        Task<IBundle?> GetNextUnacknowledgedAsync(MarketOperator recipient);

        /// <summary>
        /// Create a new bundle containing supplied dataAvailableNotifications
        /// </summary>
        /// <param name="dataAvailableNotifications">The notifications included in the bundle</param>
        /// <param name="contentPath">The path to the content in blob storage</param>
        /// <returns>Bundle</returns>
        Task<IBundle> CreateBundleAsync(IEnumerable<DataAvailableNotification> dataAvailableNotifications, Uri contentPath);

        /// <summary>
        /// Acknowledges the bundle with the specified bundle id.
        /// </summary>
        /// <param name="bundleId">The bundle id to acknowledge.</param>
        Task AcknowledgeAsync(Uuid bundleId);
    }
}
