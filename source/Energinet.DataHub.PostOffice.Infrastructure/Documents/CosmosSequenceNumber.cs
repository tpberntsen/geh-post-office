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
    // JsonProperty is required for all properties, as the object is
    // used in CosmosDbFixture where naming rules are not yet configured,
    internal sealed record CosmosSequenceNumber
    {
        public const string CosmosSequenceNumberIdentifier = "CosmosSequenceNumberIdentifier";
        public const string CosmosSequenceNumberPartitionKey = "SequenceNumber";

        public CosmosSequenceNumber(long sequenceNumber)
        {
            Id = CosmosSequenceNumberIdentifier;
            PartitionKey = CosmosSequenceNumberPartitionKey;
            SequenceNumber = sequenceNumber;
        }

        [JsonProperty(PropertyName = "id")]
        public string Id { get; init; }
        [JsonProperty(PropertyName = "partitionKey")]
        public string PartitionKey { get; init; }
        [JsonProperty(PropertyName = "sequenceNumber")]
        public long SequenceNumber { get; init; }
    }
}
