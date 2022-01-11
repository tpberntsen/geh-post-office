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
    internal sealed record CosmosSubPartitionLookup
    {
        public CosmosSubPartitionLookup()
        {
            Id = null!;
            PartitionKey = null!;
            InitialSequenceNumber = -1;
            CurrentCursor = 0;
            ETag = null!;
        }

        public string Id { get; init; } // The partition key (a random guid) of the sub-partition.
        public string PartitionKey { get; init; } // Used to find all sub-partitions; format is <recipient>_<origin>_<contentType>.
        public long InitialSequenceNumber { get; init; } // The sequence number of the first item in sub-partition. Used to order by.
        public int CurrentCursor { get; init; } // A 0-zero index pointing to the next item in sub-partition.

        [JsonProperty(PropertyName = "_etag")]
        public string ETag { get; init; } // Prevents conflicting overwrites during Dequeue.
    }
}
