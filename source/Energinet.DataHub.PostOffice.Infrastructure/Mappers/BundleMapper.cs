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

using System;
using System.Linq;
using Energinet.DataHub.PostOffice.Domain.Model;
using Energinet.DataHub.PostOffice.Infrastructure.Documents;

namespace Energinet.DataHub.PostOffice.Infrastructure.Mappers
{
    internal static class BundleMapper
    {
        public static Bundle MapFromDocument(BundleDocument from)
        {
            return new Bundle(
                new Uuid(from.Id),
                from.NotificationIds.Select(x => new Uuid(x)));
        }

        public static BundleDocument MapToDocument(IBundle from, MarketOperator recipient, Uri? contentPath)
        {
            return new BundleDocument
            {
                Recipient = recipient.Gln.Value,
                Id = from.BundleId.ToString(),
                NotificationIds = from.NotificationIds.Select(x => x.ToString()).ToList(),
                Dequeued = false,
                ContentPath = contentPath?.ToString()!
            };
        }
    }
}
