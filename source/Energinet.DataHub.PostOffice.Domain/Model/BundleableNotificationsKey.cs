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
using System.Diagnostics.CodeAnalysis;

namespace Energinet.DataHub.PostOffice.Domain.Model
{
    public sealed class BundleableNotificationsKey
    {
        public BundleableNotificationsKey(
            MarketOperator recipient,
            DomainOrigin origin,
            ContentType messageType)
        {
            Recipient = recipient;
            Origin = origin;
            MessageType = messageType;
        }

        public BundleableNotificationsKey(DataAvailableNotification dataAvailableNotification)
        {
            if (dataAvailableNotification is null)
                throw new ArgumentNullException(nameof(dataAvailableNotification));

            Recipient = dataAvailableNotification.Recipient;
            Origin = dataAvailableNotification.Origin;
            MessageType = dataAvailableNotification.ContentType;
        }

        public MarketOperator Recipient { get; set; }
        public DomainOrigin Origin { get; set; }
        public ContentType MessageType { get; set; }
    }
}
