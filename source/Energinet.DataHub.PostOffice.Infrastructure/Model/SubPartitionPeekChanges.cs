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

using Energinet.DataHub.PostOffice.Infrastructure.Documents;

namespace Energinet.DataHub.PostOffice.Infrastructure.Model
{
    internal sealed class SubPartitionPeekChanges
    {
        private readonly string _subPartitionLookupId;
        private readonly long _subPartitionInitialSequenceNumber;
        private readonly long _subPartitionNextSequenceNumber;
        private readonly int _subPartitionNextCursorPosition;

        private readonly string _contentTypeLookupId;

        public SubPartitionPeekChanges(
            string recipient,
            string origin,
            string contentType,
            string subPartitionLookupId,
            string subPartitionLookupExpectedETag,
            long subPartitionInitialSequenceNumber,
            long subPartitionNextSequenceNumber,
            int subPartitionNextCursorPosition,
            string contentTypeLookupId,
            long contentTypeLookupSequenceNumber)
        {
            Recipient = recipient;
            Origin = origin;
            ContentType = contentType;
            _subPartitionLookupId = subPartitionLookupId;
            SubPartitionLookupExpectedETag = subPartitionLookupExpectedETag;
            _subPartitionInitialSequenceNumber = subPartitionInitialSequenceNumber;
            _subPartitionNextSequenceNumber = subPartitionNextSequenceNumber;
            _subPartitionNextCursorPosition = subPartitionNextCursorPosition;
            _contentTypeLookupId = contentTypeLookupId;
            ContentTypeLookupSequenceNumber = contentTypeLookupSequenceNumber;
        }

        public string Recipient { get; }
        public string Origin { get; }
        public string ContentType { get; }

        public string SubPartitionLookupPartitionKey => string.Join('_', Recipient, Origin, ContentType);
        public string ContentTypeLookupPartitionKey => string.Join('_', Recipient, Origin);

        public string SubPartitionLookupExpectedETag { get; }

        public long ContentTypeLookupSequenceNumber { get; }

        public CosmosSubPartitionLookup GetNextSubPartitionLookup()
        {
            return new CosmosSubPartitionLookup
            {
                Id = _subPartitionLookupId,
                PartitionKey = SubPartitionLookupPartitionKey,
                InitialSequenceNumber = _subPartitionInitialSequenceNumber,
                CurrentCursor = _subPartitionNextCursorPosition
            };
        }

        public CosmosContentTypeLookup? GetNextContentTypeLookup()
        {
            if (_subPartitionNextSequenceNumber < 0)
                return null;

            return new CosmosContentTypeLookup
            {
                Id = _contentTypeLookupId,
                PartitionKey = ContentTypeLookupPartitionKey,
                ContentType = ContentType,
                NextSequenceNumber = _subPartitionNextSequenceNumber
            };
        }
    }
}
