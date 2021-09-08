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
using System.Linq;
using System.Net;
using System.Text.Json;
using Energinet.DataHub.PostOffice.Common.Model;
using FluentValidation;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;

namespace Energinet.DataHub.PostOffice.Common.Extensions
{
    public static class ExceptionExtensions
    {
        public static void Log(this Exception source, ILogger logger)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));

            if (source is not ValidationException)
                logger.LogError(source, "An error occurred while processing request");
        }

        public static HttpResponseData AsHttpResponseData(this Exception source, HttpRequestData request)
        {
            static HttpResponseData CreateHttpResponseData(HttpRequestData request, HttpStatusCode httpStatusCode, IEnumerable<ErrorDescriptor> errors)
            {
                var bytes = JsonSerializer.SerializeToUtf8Bytes(
                    new FunctionError(errors.ToArray()),
                    new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
                var stream = new MemoryStream(bytes);
                return request.CreateResponse(stream, httpStatusCode);
            }

            if (source == null)
                throw new ArgumentNullException(nameof(source));

            return source switch
            {
                ValidationException ve => CreateHttpResponseData(request, HttpStatusCode.BadRequest, ve.Errors.Select(x => new ErrorDescriptor(x.ErrorCode, x.ErrorMessage, x.PropertyName))),
                _ => CreateHttpResponseData(request, HttpStatusCode.InternalServerError, new[] { new ErrorDescriptor("INTERNAL_ERROR", "An error occured while processing the request", null) })
            };
        }
    }
}
