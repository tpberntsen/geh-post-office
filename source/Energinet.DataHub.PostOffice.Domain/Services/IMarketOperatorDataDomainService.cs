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

namespace Energinet.DataHub.PostOffice.Domain.Services
{
    /// <summary>
    /// Provides market operators with access to their data. The data is grouped and returned as a bundle.
    /// </summary>
    public interface IMarketOperatorDataDomainService
    {
        /// <summary>
        /// Get the next bundle of unacknowledged data for a given market operator.
        /// Returns null when there is no new unacknowledged data to get.
        /// </summary>
        /// <param name="recipient">The market operator to get the next unacknowledged bundle for.</param>
        /// <param name="bundleId">The bundleId used for the next bundle</param>
        /// <returns>The next unacknowledged bundle; or null, if there is no new data.</returns>
        Task<Bundle?> GetNextUnacknowledgedAsync(MarketOperator recipient, Uuid bundleId);

        /// <summary>
        /// Get the next bundle of unacknowledged time series data for a given market operator.
        /// Returns null when there is no new unacknowledged data to get.
        /// </summary>
        /// <param name="recipient">The market operator to get the next unacknowledged bundle for.</param>
        /// <param name="bundleId">The bundleId used for the next bundle</param>
        /// <returns>The next unacknowledged bundle; or null, if there is no new data.</returns>
        Task<Bundle?> GetNextUnacknowledgedTimeSeriesAsync(MarketOperator recipient, Uuid bundleId);

        /// <summary>
        /// Get the next bundle of unacknowledged master data for a given market operator.
        /// Returns null when there is no new unacknowledged data to get.
        /// </summary>
        /// <param name="recipient">The market operator to get the next unacknowledged bundle for.</param>
        /// <param name="bundleId">The bundleId used for the next bundle</param>
        /// <returns>The next unacknowledged bundle; or null, if there is no new data.</returns>
        Task<Bundle?> GetNextUnacknowledgedMasterDataAsync(MarketOperator recipient, Uuid bundleId);

        /// <summary>
        /// Get the next bundle of unacknowledged aggregations data for a given market operator.
        /// Returns null when there is no new unacknowledged data to get.
        /// </summary>
        /// <param name="recipient">The market operator to get the next unacknowledged bundle for.</param>
        /// <param name="bundleId">id for data response.</param>
        /// <returns>The next unacknowledged bundle; or null, if there is no new data.</returns>
        Task<Bundle?> GetNextUnacknowledgedAggregationsAsync(MarketOperator recipient, Uuid bundleId);

        /// <summary>
        /// Checks if the current bundle can be acknowledged.
        /// If there is nothing to acknowledge or the id does not match the bundle, the method returns false.
        /// </summary>
        /// <param name="recipient">The market operator that is the recipient of the bundle.</param>
        /// <param name="bundleId">The id of the bundle that is being acknowledged.</param>
        /// <returns>true is the bundle can be acknowledged; false if the id is incorrect or there is nothing to acknowledge.</returns>
        Task<(bool CanAcknowledge, Bundle? Bundle)> CanAcknowledgeAsync(MarketOperator recipient, Uuid bundleId);

        /// <summary>
        /// Acknowledges the current bundle, as returned by GetNextUnacknowledgedAsync.
        /// </summary>
        /// <param name="bundle">The the bundle that is being acknowledged.</param>
        Task AcknowledgeAsync(Bundle bundle);
    }
}
