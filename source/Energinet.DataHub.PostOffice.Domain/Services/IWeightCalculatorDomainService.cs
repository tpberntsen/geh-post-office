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

using Energinet.DataHub.PostOffice.Domain.Model;

namespace Energinet.DataHub.PostOffice.Domain.Services
{
    /// <summary>
    /// Performs calculation and transformation of <see cref="Weight"/>.
    /// </summary>
    public interface IWeightCalculatorDomainService
    {
        /// <summary>
        /// Maps a given <see cref="DomainOrigin"/> to its maximum <see cref="Weight"/>.
        /// </summary>
        /// <param name="domainOrigin">The <see cref="DomainOrigin"/> for which a maximum <see cref="Weight"/> is found and returned.</param>
        /// <param name="returnType">The <see cref="BundleReturnType"/> for which a maximum <see cref="Weight"/> is found and returned.</param>
        /// <returns>The maximum <see cref="Weight"/>.</returns>
        Weight CalculateMaxWeight(DomainOrigin domainOrigin, BundleReturnType returnType);
    }
}
