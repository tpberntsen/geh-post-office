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

using Microsoft.Azure.Cosmos;

namespace Energinet.DataHub.PostOffice.Infrastructure.Repositories.Containers
{
    /// <summary>
    /// Provides access to the CosmosDB container to use with DataAvailableNotificationRepository.
    /// </summary>
    public interface IDataAvailableNotificationRepositoryContainer
    {
        /// <summary>
        /// The CosmosDB cabinet container to use with DataAvailableNotificationRepository.
        /// </summary>
        public Container Cabinet { get; }

        /// <summary>
        /// The CosmosDB catalog container to use with DataAvailableNotificationRepository.
        /// </summary>
        public Container Catalog { get; }

        /// <summary>
        /// The CosmosDB idempotency container to use with DataAvailableNotificationRepository.
        /// </summary>
        public Container Idempotency { get; }
    }
}
