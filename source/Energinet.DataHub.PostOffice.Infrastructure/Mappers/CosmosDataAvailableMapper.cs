﻿// Copyright 2020 Energinet DataHub A/S
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
using Energinet.DataHub.PostOffice.Domain.Model;
using Energinet.DataHub.PostOffice.Infrastructure.Documents;

namespace Energinet.DataHub.PostOffice.Infrastructure.Mappers
{
    internal static class CosmosDataAvailableMapper
    {
        public static DataAvailableNotification Map(CosmosDataAvailable document)
        {
            return new DataAvailableNotification(
                new Uuid(document.Id),
                new MarketOperator(new GlobalLocationNumber(document.Recipient)),
                new ContentType(document.ContentType),
                Enum.Parse<DomainOrigin>(document.Origin),
                new SupportsBundling(document.SupportsBundling),
                new Weight(document.RelativeWeight),
                new SequenceNumber(document.SequenceNumber),
                new DocumentType(document.DocumentType));
        }

        public static CosmosDataAvailable Map(DataAvailableNotification notification, string partitionKey)
        {
            return new CosmosDataAvailable
            {
                Id = notification.NotificationId.ToString(),
                Recipient = notification.Recipient.Gln.Value,
                ContentType = notification.ContentType.Value,
                Origin = notification.Origin.ToString(),
                SupportsBundling = notification.SupportsBundling.Value,
                RelativeWeight = notification.Weight.Value,
                SequenceNumber = notification.SequenceNumber.Value,
                PartitionKey = partitionKey,
                DocumentType = notification.DocumentType.Value
            };
        }
    }
}
