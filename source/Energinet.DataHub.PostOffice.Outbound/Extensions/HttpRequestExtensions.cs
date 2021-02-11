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
using Microsoft.AspNetCore.Http;

namespace Energinet.DataHub.PostOffice.Outbound.Extensions
{
    public static class HttpRequestExtensions
    {
        public static DocumentQuery GetDocumentQuery(this HttpRequest request)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));

            var type = request.Query.ContainsKey("type") ? request.Query["type"].ToString() : string.Empty;
            var documentQuery = new DocumentQuery(type);

            if (request.Query.ContainsKey("recipient"))
            {
                documentQuery.Recipient = request.Query["recipient"];
            }

            if (request.Query.ContainsKey("pageSize") && int.TryParse(request.Query["pageSize"], out var pageSize))
            {
                documentQuery.PageSize = pageSize;
            }

            return documentQuery;
        }
    }
}
