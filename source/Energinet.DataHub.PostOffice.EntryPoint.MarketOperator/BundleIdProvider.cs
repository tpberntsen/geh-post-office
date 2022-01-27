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
using Energinet.DataHub.PostOffice.Common.Extensions;
using Microsoft.Azure.Functions.Worker.Http;

namespace Energinet.DataHub.PostOffice.EntryPoint.MarketOperator
{
    public abstract class BundleIdProvider
    {
        /// <summary>
        /// Default instance of bundle id provider
        /// </summary>
        public static BundleIdProvider Default => new GuidBundleIdProvider();

        /// <summary>
        /// Get the bundle id from the request. If no id is found it is constructed by <see cref="CreateBundleId"/>
        /// </summary>
        /// <param name="request">Probe request for bundle id</param>
        /// <returns>Bundle id</returns>
        public string GetBundleId(HttpRequestData request)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));

            var maybeBundleId = request.Url.GetQueryValue(Constants.BundleIdQueryName);
            if (string.IsNullOrEmpty(maybeBundleId)) maybeBundleId = CreateBundleId();

            return maybeBundleId;
        }

        /// <summary>
        /// Create a bundle id
        /// </summary>
        /// <returns>returns a bundle id</returns>
        protected abstract string CreateBundleId();

        private class GuidBundleIdProvider : BundleIdProvider
        {
            protected override string CreateBundleId()
                => Guid.NewGuid().ToString("N");
        }
    }
}
