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

namespace Energinet.DataHub.PostOffice.EntryPoint.MarketOperator
{
    public static class Constants
    {
        /// <summary>
        /// Name of query parameter
        /// </summary>
        public const string BundleIdQueryName = "bundleId";

        /// <summary>
        /// Name of HttpHeader that contains the response bundle-id
        /// </summary>
        public const string BundleIdHeaderName = "MessageId";

        /// <summary>
        /// Name of HttpHeader that contains the message types found in a given bundle
        /// </summary>
        public const string MessageTypeName = "MessageType";

        /// <summary>
        /// Name of query parameter
        /// </summary>
        public const string ReturnTypeQueryName = "returnType";
    }
}
