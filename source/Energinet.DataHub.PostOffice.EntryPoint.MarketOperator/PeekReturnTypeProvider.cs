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
using Energinet.DataHub.PostOffice.Domain.Model;
using Energinet.DataHub.PostOffice.Utilities;
using Microsoft.Azure.Functions.Worker.Http;

namespace Energinet.DataHub.PostOffice.EntryPoint.MarketOperator
{
    public sealed class BundleReturnTypeProvider
    {
        /// <summary>
        /// Get the return type you want for this peek request id from the request, or returns <see cref="BundleReturnType.Xml"/> if no return type was provided.
        /// </summary>
        /// <param name="request">The request to probe for the return type.</param>
        /// <returns>The return type requested, or <see cref="BundleReturnType.Xml"/> if none was provided</returns>
#pragma warning disable CA1822 // Mark members as static
        public BundleReturnType GetReturnType(HttpRequestData request)
#pragma warning restore CA1822 // Mark members as static
        {
            Guard.ThrowIfNull(request, nameof(request));

            Enum.TryParse<BundleReturnType>(request.Url.GetQueryValue(Constants.ReturnTypeQueryName), true, out var returnType);
            return returnType;
        }
    }
}
