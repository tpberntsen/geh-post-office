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

namespace Energinet.DataHub.PostOffice.Domain.Repositories
{
    /// <summary>
    /// Repository for DataAvailable domain value objects.
    /// </summary>
    public interface IDataAvailableRepository
    {
        /// <summary>
        /// Get UUIDs from Cosmos database
        /// </summary>
        /// <param name="recipient"></param>
        /// <returns>A collection with all UUIDs for the specified recipient</returns>
        public Task<RequestData> GetDataAvailableUuidsAsync(string recipient);

        /// <summary>
        /// Save a document.
        /// </summary>
        /// <param name="document">The document to save.</param>
        Task<bool> SaveDocumentAsync(DataAvailable document);
    }
}
