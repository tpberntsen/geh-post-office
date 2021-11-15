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

namespace Energinet.DataHub.PostOffice.Common.Auth
{
    /// <summary>
    /// Represents a market operator identity.
    /// </summary>
    public interface IMarketOperatorIdentity
    {
        /// <summary>
        /// Returns true if an identity has been assigned.
        /// </summary>
        bool HasIdentity { get; }

        /// <summary>
        /// The GLN of the market operator, or an exception if the identity is unassigned.
        /// </summary>
        string Gln { get; }

        /// <summary>
        /// Used by infrastructure to assign the identity.
        /// </summary>
        /// <param name="gln">The GLN to assign to the identity.</param>
        internal void AssignGln(string gln);
    }
}
