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
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mime;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using System.Xml;
using Energinet.DataHub.PostOffice.Common.Model;
using Energinet.DataHub.PostOffice.Utilities;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Net.Http.Headers;
using DataAnnotationException = System.ComponentModel.DataAnnotations.ValidationException;
using FluentValidationException = FluentValidation.ValidationException;

namespace Energinet.DataHub.PostOffice.Common.Extensions
{
    public static class ExceptionExtensions
    {
        public static void Log(this Exception source, ILogger logger)
        {
            Guard.ThrowIfNull(source, nameof(source));

            if (source is not FluentValidationException)
            {
                logger.LogError(source, "An error occurred while processing request");

                // Observed that LogError does not always write the exception.
                logger.LogError(source.ToString());
            }
        }

        public static async Task<HttpResponseData> AsHttpResponseDataAsync(this Exception source, HttpRequestData request)
        {
            static async Task<HttpResponseData> CreateHttpResponseData(HttpRequestData request, HttpStatusCode httpStatusCode, ErrorDescriptor error)
            {
                static async Task<MemoryStream> XmlSerializeAsync(ErrorDescriptor error)
                {
                    var stream = new MemoryStream();
                    await using var writer = XmlWriter.Create(
                        stream,
                        new XmlWriterSettings
                        {
                            Async = true,
                            Encoding = new UTF8Encoding(false)
                        });
                    await new ErrorResponse(error).WriteXmlContentsAsync(writer).ConfigureAwait(false);
                    return stream;
                }

                static MemoryStream JsonSerialize(ErrorDescriptor error)
                {
                    var bytes = JsonSerializer.SerializeToUtf8Bytes(
                        new ErrorResponse(error),
                        new JsonSerializerOptions
                        {
                            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
                        });
                    return new MemoryStream(bytes);
                }

                var returnXml =
                    request.Headers != null &&
                    request.Headers.TryGetValues(HeaderNames.ContentType, out var values) &&
                    values.Contains(MediaTypeNames.Application.Xml, StringComparer.OrdinalIgnoreCase);

                var stream = returnXml
                    ? await XmlSerializeAsync(error).ConfigureAwait(false)
                    : JsonSerialize(error);

                return request.CreateResponse(
                    stream,
                    returnXml ? MediaTypeNames.Application.Xml : MediaTypeNames.Application.Json,
                    httpStatusCode);
            }

            Guard.ThrowIfNull(source, nameof(source));
            Guard.ThrowIfNull(request, nameof(request));

            return source switch
            {
                FluentValidationException ve =>
                    await CreateHttpResponseData(
                        request,
                        HttpStatusCode.BadRequest,
                        new ErrorDescriptor(
                            "VALIDATION_EXCEPTION",
                            "See details",
                            details: ve.Errors.Select(x =>
                                new ErrorDescriptor(
                                    x.ErrorCode,
                                    x.ErrorMessage,
                                    x.PropertyName)))).ConfigureAwait(false),

                DataAnnotationException ve =>
                    await CreateHttpResponseData(
                        request,
                        HttpStatusCode.BadRequest,
                        new ErrorDescriptor(
                            "VALIDATION_EXCEPTION",
                            ve.Message)).ConfigureAwait(false),

                CosmosException { StatusCode: HttpStatusCode.TooManyRequests } =>
                    await CreateHttpResponseData(
                        request,
                        HttpStatusCode.InternalServerError,
                        new ErrorDescriptor(
                            "INTERNAL_ERROR",
                            "TMR")).ConfigureAwait(false),

                _ =>
                    await CreateHttpResponseData(
                        request,
                        HttpStatusCode.InternalServerError,
                        new ErrorDescriptor(
                            "INTERNAL_ERROR",
                            "An error occured while processing the request.")).ConfigureAwait(false)
            };
        }
    }
}
