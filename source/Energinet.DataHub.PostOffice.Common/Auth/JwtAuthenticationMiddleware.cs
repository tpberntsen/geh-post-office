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
using System.IdentityModel.Tokens.Jwt;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Middleware;

namespace Energinet.DataHub.PostOffice.Common.Auth
{
    public sealed class JwtAuthenticationMiddleware : IFunctionsWorkerMiddleware
    {
        private static readonly JwtSecurityTokenHandler _tokenHandler = new();
        private readonly IMarketOperatorIdentity _identity;

        public JwtAuthenticationMiddleware(IMarketOperatorIdentity identity)
        {
            _identity = identity;
        }

        public Task Invoke(FunctionContext context, FunctionExecutionDelegate next)
        {
            if (context == null)
                throw new ArgumentNullException(nameof(context));

            if (next == null)
                throw new ArgumentNullException(nameof(next));

            if (!_identity.HasIdentity)
            {
                try
                {
                    // This abomination is temporary, while MS is working on something nicer.
                    // https://github.com/Azure/azure-functions-dotnet-worker/issues/414
                    if (context.BindingContext.BindingData["headers"] is string headers)
                    {
                        var headerMatch = Regex.Match(headers, "\"[aA]uthorization\"\\s*:\\s*\"Bearer (.*?)\"");
                        if (headerMatch.Success && headerMatch.Groups.Count == 2)
                        {
                            var token = headerMatch.Groups[1].Value;
                            var parsed = _tokenHandler.ReadJwtToken(token);
                            _identity.AssignGln(parsed.Subject);
                        }
                    }
                }
                catch (Exception ex) when (ex is ArgumentException or FormatException or InvalidOperationException)
                {
                    // If token not parsable, do not auth.
                }
            }

            return next(context);
        }
    }
}
