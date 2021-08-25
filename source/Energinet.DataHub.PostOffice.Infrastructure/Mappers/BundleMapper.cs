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
using Energinet.DataHub.PostOffice.Infrastructure.Entities;

namespace Energinet.DataHub.PostOffice.Infrastructure.Mappers
{
    public static class BundleMapper
    {
        public static Bundle MapFromDocument(BundleDocument from)
        {
            if (@from is null) throw new ArgumentNullException(nameof(@from));
            return new Bundle(
                new Uuid(from.Id),
                from.NotificationsIds.Select(x => new Uuid(x)));
        }

        public static BundleDocument MapToDocument(IBundle from, Recipient recipient)
        {
            if (@from is null)
                throw new ArgumentNullException(nameof(@from));
            if (recipient is null)
                throw new ArgumentNullException(nameof(recipient));

            return new BundleDocument(recipient, from.Id, from.NotificationsIds, false);
        }
    }
}
