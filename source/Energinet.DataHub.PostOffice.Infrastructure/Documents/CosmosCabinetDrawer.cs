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

using Newtonsoft.Json;

namespace Energinet.DataHub.PostOffice.Infrastructure.Documents
{
    internal sealed record CosmosCabinetDrawer
    {
        public CosmosCabinetDrawer()
        {
            Id = null!;
            PartitionKey = null!;
            Position = 0;
            OrderBy = -1;
            ETag = null!;
        }

        public string Id { get; init; }
        public string PartitionKey { get; init; }
        public int Position { get; init; }
        public long OrderBy { get; init; }

        [JsonProperty(PropertyName = "_etag")]
        public string ETag { get; init; }
    }
}
