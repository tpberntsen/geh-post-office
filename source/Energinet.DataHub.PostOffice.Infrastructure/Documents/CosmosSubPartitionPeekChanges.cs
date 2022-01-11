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

namespace Energinet.DataHub.PostOffice.Infrastructure.Documents
{
    internal sealed record CosmosSubPartitionPeekChanges
    {
        public CosmosSubPartitionPeekChanges()
        {
            ContentTypeLookupId = null!;
            SubPartitionLookupId = null!;
            SubPartitionLookupExpectedETag = null!;
        }

        public string ContentTypeLookupId { get; init; }
        public string SubPartitionLookupId { get; init; }
        public long SubPartitionInitialSequenceNumber { get; init; }
        public long SubPartitionNextSequenceNumber { get; init; }
        public int SubPartitionNextCursorPosition { get; init; }
        public string SubPartitionLookupExpectedETag { get; init; }
        public long ContentTypeLookupSequenceNumber { get; init; }
    }
}
