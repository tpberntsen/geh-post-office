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

namespace Energinet.DataHub.PostOffice.Application.GetMessage
{
    /// <summary>
    /// Service to connect and retrieve data from Cosmos database
    /// </summary>
    public interface ICosmosService
    {
        /// <summary>
        /// Get UUIDs from Cosmos database
        /// </summary>
        /// <param name="recipient"></param>
        /// <returns>A collection with all UUIDs for the specified recipient</returns>
        public Task<IList<string>> GetUuidsFromCosmosDatabaseAsync(string recipient);
    }
}
