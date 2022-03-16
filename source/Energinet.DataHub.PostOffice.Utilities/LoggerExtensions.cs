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

using System.Collections.Generic;
using Microsoft.Extensions.Logging;

namespace Energinet.DataHub.PostOffice.Utilities
{
    public static class LoggerExtensions
    {
        public static void LogProcess(this ILogger source, string entryPoint, string correlationId, string gln)
        {
            source.LogInformation("EntryPoint={0};CorrelationId={1};Gln={2}", entryPoint, correlationId, gln);
        }

        public static void LogProcess(this ILogger source, string entryPoint, string status, string correlationId, string gln, string bundleId)
        {
            source.LogInformation("EntryPoint={0};Status={1};CorrelationId={2};Gln={3};BundleId={4}", entryPoint, status, correlationId, gln, bundleId);
        }

        public static void LogProcess(this ILogger source, string entryPoint, string status, string correlationId, string gln, string bundleId, string domain)
        {
            source.LogInformation("EntryPoint={0};Status={1};CorrelationId={2};Gln={3};BundleId={4};Domain={5}", entryPoint, status, correlationId, gln, bundleId, domain);
        }

        public static void LogProcess(this ILogger source, string entryPoint, string status, string correlationId, string gln, string bundleId, IEnumerable<string> dataAvailables)
        {
            source.LogInformation("EntryPoint={0};Status={1};CorrelationId={2};Gln={3};BundleId={4};DataAvailables={5}", entryPoint, status, correlationId, gln, bundleId, string.Join(",", dataAvailables));
        }
    }
}
