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
using System.IO;
using System.Net;
using System.Security.Claims;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;

namespace Microsoft.Azure.Functions.Isolated.TestDoubles
{
    public class MockHttpRequestData
        : HttpRequestData
    {
        public MockHttpRequestData(
            FunctionContext functionContext,
            Stream? body = null,
            string method = "GET",
            string? url = null)
            : base(functionContext)
        {
            Body = body ?? new MemoryStream();
            Headers = new HttpHeadersCollection();
            Cookies = new List<IHttpCookie>();
            Url = new Uri(url ?? "https://localhost");
            Identities = new List<ClaimsIdentity>();
            Method = method;
        }

        public override Stream Body { get; }

        public override HttpHeadersCollection Headers { get; }

        public override IReadOnlyCollection<IHttpCookie> Cookies { get; }

        public override Uri Url { get; }

        public override IEnumerable<ClaimsIdentity> Identities { get; }

        public override string Method { get; }

        public override HttpResponseData CreateResponse()
        {
            return new MockHttpResponseData(FunctionContext, HttpStatusCode.OK);
        }
    }
}
