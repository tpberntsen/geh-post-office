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

namespace Energinet.DataHub.PostOffice.Application
{
    /// <summary>
    /// Map from one object to another.
    /// </summary>
    /// <typeparam name="TFromType">Type to map from.</typeparam>
    /// <typeparam name="TToType">Type to map to.</typeparam>
    public interface IMapper<in TFromType, out TToType>
    {
        /// <summary>
        /// Map from one object to another.
        /// </summary>
        /// <param name="obj">object to map from.</param>
        /// <returns>An mapped object.</returns>
        TToType Map(TFromType obj);
    }
}
