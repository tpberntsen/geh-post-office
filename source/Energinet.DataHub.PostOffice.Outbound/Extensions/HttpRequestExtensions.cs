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
using Energinet.DataHub.PostOffice.Application;
using Microsoft.Azure.Functions.Worker.Http;

namespace Energinet.DataHub.PostOffice.Outbound.Extensions
{
    public static class HttpRequestExtensions
    {
        public static DocumentQuery GetDocumentQuery(this HttpRequestData request)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));

            var queryDictionary = Microsoft.AspNetCore.WebUtilities.QueryHelpers.ParseQuery(request.Url.Query);
            var group = queryDictionary.ContainsKey("group") ? queryDictionary["group"].ToString() : null;
            var recipient = queryDictionary.ContainsKey("recipient") ? queryDictionary["recipient"].ToString() : null;
            if (group == null || recipient == null)
            {
                throw new InvalidOperationException("Request must include group and recipient.");
            }

            var documentQuery = new DocumentQuery(recipient!, group!);

            if (queryDictionary.ContainsKey("pageSize") && int.TryParse(queryDictionary["pageSize"], out var pageSize))
            {
                documentQuery.PageSize = pageSize;
            }

            return documentQuery;
        }

        public static DequeueCommand GetDequeueCommand(this HttpRequestData request)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));

            var queryDictionary = Microsoft.AspNetCore.WebUtilities.QueryHelpers.ParseQuery(request.Url.Query);
            var bundle = queryDictionary.ContainsKey("bundle") ? queryDictionary["bundle"].ToString() : null;
            var recipient = queryDictionary.ContainsKey("recipient") ? queryDictionary["recipient"].ToString() : null;
            if (bundle == null || recipient == null)
            {
                throw new InvalidOperationException("Request must include bundle and recipient.");
            }

            var dequeueCommand = new DequeueCommand(recipient!, bundle!);

            return dequeueCommand;
        }
    }
}
