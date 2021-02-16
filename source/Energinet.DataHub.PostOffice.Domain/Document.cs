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

using System.Text.Json.Serialization;
using GreenEnergyHub.Messaging.MessageTypes.Common;
using GreenEnergyHub.Messaging.Transport;
using NodaTime;

namespace Energinet.DataHub.PostOffice.Domain
{
    // TODO: I don't know if this should depend on IInboundMessage (or anything really).
    public class Document : IInboundMessage
    {
        public string? Recipient { get; set; }

        public string? Type { get; set; }

        public Instant? EffectuationDate { get; set; }

        public dynamic? Content { get; set; }

        // TODO: This comes from IHubMessage (via IInboundMessage), but we don't want that.
        public Transaction Transaction { get => null!; set => _ = value; }
    }
}
