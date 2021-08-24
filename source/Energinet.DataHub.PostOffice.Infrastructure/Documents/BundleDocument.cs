// // Copyright 2020 Energinet DataHub A/S
// //
// // Licensed under the Apache License, Version 2.0 (the "License2");
// // you may not use this file except in compliance with the License.
// // You may obtain a copy of the License at
// //
// //     http://www.apache.org/licenses/LICENSE-2.0
// //
// // Unless required by applicable law or agreed to in writing, software
// // distributed under the License is distributed on an "AS IS" BASIS,
// // WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// // See the License for the specific language governing permissions and
// // limitations under the License.
using System.Collections.Generic;
using System.Linq;
using Energinet.DataHub.PostOffice.Domain.Model;
using Newtonsoft.Json;

namespace Energinet.DataHub.PostOffice.Infrastructure.Entities
{
    public record BundleDocument
    {
        public BundleDocument(string recipient, Uuid id, IEnumerable<Uuid> notificationsIds, bool dequeued)
        {
            Recipient = recipient;
            Id = id;
            NotificationsIds = notificationsIds;
            Dequeued = dequeued;
        }

        [JsonProperty("recipient")]
        public string Recipient { get; }
        [JsonProperty("id")]
        public Uuid Id { get; }
        [JsonProperty("notificationids")]
        public IEnumerable<Uuid> NotificationsIds { get; }
        [JsonProperty("dequeued")]
        public bool Dequeued { get; init; }
    }
}
