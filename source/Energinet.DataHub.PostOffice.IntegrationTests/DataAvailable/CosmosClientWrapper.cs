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
using Moq;

namespace Energinet.DataHub.PostOffice.IntegrationTests.DataAvailable
{
    public sealed class CosmosClientWrapper : CosmosClient
    {
        private readonly Mock<Container> _mockContainer;

        public CosmosClientWrapper(string connectionString,  Mock<Container> mockContainer)
            : base(connectionString)
        {
            _mockContainer = mockContainer;
            ClientOptions.AllowBulkExecution = false;
        }

        public override Container GetContainer(string databaseId, string containerId)
        {
            return _mockContainer.Object;
        }
    }
}
