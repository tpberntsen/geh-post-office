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
using Newtonsoft.Json;

namespace Energinet.DataHub.PostOffice.Infrastructure.Documents
{
    internal sealed record CosmosBundleDocument
    {
        public CosmosBundleDocument()
        {
            Id = string.Empty;
            ProcessId = string.Empty;

            Recipient = string.Empty;
            Origin = string.Empty;
            MessageType = string.Empty;

            NotificationIds = new List<string>();
            ContentPath = string.Empty;
            Timestamp = null!;
        }

        public string Id { get; init; }
        public string ProcessId { get; init; }
        public bool Dequeued { get; init; }

        public string Recipient { get; init; }
        public string Origin { get; init; }
        public string MessageType { get; init; }

        public ICollection<string> NotificationIds { get; init; }
        public string ContentPath { get; init; }

        [JsonProperty(PropertyName = "_ts")]
        public string Timestamp { get; init; }
    }
}
