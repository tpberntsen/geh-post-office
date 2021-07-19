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

using System;
using Energinet.DataHub.PostOffice.Application;
using Energinet.DataHub.PostOffice.Contracts;
using NodaTime.Serialization.Protobuf;

namespace Energinet.DataHub.PostOffice.Infrastructure
{
    public sealed class DocumentMapper : IMapper<Contracts.Document, Domain.Document>
    {
        public Domain.Document Map(Document obj)
        {
            if (obj is null) throw new ArgumentNullException(nameof(obj));

            var document = new Domain.Document
            {
                Content = Newtonsoft.Json.JsonConvert.DeserializeObject<dynamic>(obj.Content),
                Type = obj.Type,
                Recipient = obj.Recipient,
                EffectuationDate = obj.EffectuationDate.ToInstant(),
                Version = obj.Version,
            };

            return document;
        }
    }
}
