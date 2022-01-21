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
using System.Collections.Generic;
using System.Linq;
using Energinet.DataHub.PostOffice.Domain.Model;
using Energinet.DataHub.PostOffice.Infrastructure.Documents;
using Energinet.DataHub.PostOffice.Infrastructure.Model;

namespace Energinet.DataHub.PostOffice.Infrastructure.Mappers
{
    internal static class BundleMapper
    {
        public static Bundle Map(CosmosBundleDocument bundleDocument, IBundleContent? bundleContent = null)
        {
            var bundle = new Bundle(
                new Uuid(bundleDocument.Id),
                new MarketOperator(new GlobalLocationNumber(bundleDocument.Recipient)),
                Enum.Parse<DomainOrigin>(bundleDocument.Origin),
                new ContentType(bundleDocument.ContentType),
                bundleDocument.NotificationIds.Select(id => new Uuid(id)),
                bundleContent);

            if (bundleDocument.Dequeued)
                bundle.Dequeue();

            return bundle;
        }

        public static CosmosBundleDocument Map(Bundle source, IEnumerable<CosmosCabinetDrawerChanges> changes)
        {
            return new CosmosBundleDocument
            {
                Id = source.BundleId.ToString(),
                ProcessId = source.ProcessId.ToString(),
                Recipient = source.Recipient.Gln.Value,
                Origin = source.Origin.ToString(),
                ContentType = source.ContentType.Value,

                Dequeued = source.Dequeued,

                AffectedDrawers = changes.ToList(),
                NotificationIds = source.NotificationIds.Select(id => id.ToString()).ToList(),
                ContentPath = MapBundleContent(source)
            };
        }

        private static string MapBundleContent(Bundle bundle)
        {
            if (!bundle.TryGetContent(out var bundleContent))
                return string.Empty;

            switch (bundleContent)
            {
                case AzureBlobBundleContent azureBlobBundleContent:
                    return azureBlobBundleContent.ContentPath.ToString();
                default:
                    return string.Empty;
            }
        }
    }
}
