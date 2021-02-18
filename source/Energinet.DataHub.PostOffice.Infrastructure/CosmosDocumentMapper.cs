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
using NodaTime.Extensions;

namespace Energinet.DataHub.PostOffice.Infrastructure
{
    public sealed class CosmosDocumentMapper
    {
        public static CosmosDocument Map(Domain.Document obj)
        {
            if (obj == null) throw new ArgumentNullException(nameof(obj));

            return new CosmosDocument
            {
                Content = obj.Content,
                Type = obj.Type,
                Recipient = obj.Recipient,

                // TODO: fix epoch nullable hack
                EffectuationDate = obj.EffectuationDate?.ToDateTimeOffset() ?? DateTimeOffset.UnixEpoch,
            };
        }

        public static Domain.Document Map(CosmosDocument obj)
        {
            if (obj == null) throw new ArgumentNullException(nameof(obj));

            return new Domain.Document
            {
                Content = obj.Content,
                EffectuationDate = obj.EffectuationDate.ToInstant(),
                Recipient = obj.Recipient,
                Type = obj.Type,
                Bundle = obj.Bundle,
            };
        }
    }
}
