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

using System.Collections.Generic;
using Energinet.DataHub.PostOffice.Domain;
using Energinet.DataHub.PostOffice.Infrastructure;
using Moq;

namespace Energinet.DataHub.PostOffice.Tests.Helpers
{
    public static class TestData
    {
        public static IEnumerable<DataAvailable> GetRandomValidDataAvailables(int number)
        {
            var list = new List<DataAvailable>();
            for (var i = 0; i < number; i++)
            {
                list.Add(new DataAvailable(
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<bool>(),
                    It.IsAny<int>(),
                    It.IsAny<int>()));
            }

            return list;
        }

        public static IEnumerable<CosmosDataAvailable> GetRandomValidCosmosDataAvailables(int number)
        {
            var list = new List<CosmosDataAvailable>();
            for (var i = 0; i < number; i++)
            {
                list.Add(new CosmosDataAvailable()
                {
                    id = It.IsAny<string>(),
                    recipient = It.IsAny<string>(),
                    messageType = It.IsAny<string>(),
                    origin = It.IsAny<string>(),
                    supportsBundling = It.IsAny<bool>(),
                    relativeWeight = It.IsAny<int>(),
                    priority = It.IsAny<int>(),
                });
            }

            return list;
        }
    }
}
