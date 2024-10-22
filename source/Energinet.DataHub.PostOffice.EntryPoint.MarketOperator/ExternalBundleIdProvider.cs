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

using Energinet.DataHub.PostOffice.Common.Extensions;
using Energinet.DataHub.PostOffice.Utilities;
using Microsoft.Azure.Functions.Worker.Http;

namespace Energinet.DataHub.PostOffice.EntryPoint.MarketOperator
{
    public sealed class ExternalBundleIdProvider
    {
        /// <summary>
        /// Get the bundle id from the request, or returns null if no bundle id was provided.
        /// </summary>
        /// <param name="request">The request to probe for the bundle id.</param>
        /// <returns>The bundle id, or null.</returns>
#pragma warning disable CA1822 // Mark members as static
        public string? TryGetBundleId(HttpRequestData request)
#pragma warning restore CA1822 // Mark members as static
        {
            Guard.ThrowIfNull(request, nameof(request));

            var maybeBundleId = request.Url.GetQueryValue(Constants.BundleIdQueryName);
            return !string.IsNullOrWhiteSpace(maybeBundleId) ? maybeBundleId : null;
        }
    }
}
