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
using System.Net.Mime;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Energinet.DataHub.PostOffice.Common.Extensions;
using Energinet.DataHub.PostOffice.Common.Model;
using Energinet.DataHub.PostOffice.Domain.Model;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.Extensions.Logging;
using Microsoft.Net.Http.Headers;
using Xunit;
using Xunit.Categories;

namespace Energinet.DataHub.PostOffice.Tests.Common.Extensions
{
    [UnitTest]
    public class ExceptionExtensionsTests
    {
        [Fact]
        public void Log_ExceptionIsNotValidationException_LogsError()
        {
            // arrange
            var logger = new FakeLogger();
            var exception = new InvalidOperationException("Something is not right");

            // act
            exception.Log(logger);

            // assert
            Assert.Equal(2, logger.LogCallCount);
        }

        [Fact]
        public void Log_ExceptionIsValidationException_DoesNotLogError()
        {
            // arrange
            var logger = new FakeLogger();
            var exception = new ValidationException("Something is not right");

            // act
            exception.Log(logger);

            // assert
            Assert.Equal(0, logger.LogCallCount);
        }

        [Fact]
        public void Log_SourceIsNull_ThrowsNullReferenceException()
        {
            // arrange, act, assert
            Assert.Throws<ArgumentNullException>(() => ((Exception)null!).Log(new FakeLogger()));
        }

        [Fact]
        public async Task AsHttpResponseData_ExceptionIsValidationException_ReturnsResponseWithAllErrorsAndStatusBadRequest()
        {
            // arrange
            var request = new MockedHttpRequestData(new MockedFunctionContext());
            var errors = new[]
            {
                new ValidationFailure("prop1", "err1") { ErrorCode = "code1" },
                new ValidationFailure("prop2", "err2") { ErrorCode = "code2" }
            };
            var exception = new ValidationException(errors);

            // act
            var actual = await exception.AsHttpResponseDataAsync(request.HttpRequestData).ConfigureAwait(false);
            var actualFunctionError = JsonSerializer.Deserialize<ErrorResponse>(
                ((MemoryStream)actual.Body).ToArray(),
                new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase })!;

            // assert
            var detailErrors = actualFunctionError.Error.Details!.ToArray();
            Assert.Equal(HttpStatusCode.BadRequest, actual.StatusCode);
            Assert.Equal(2, detailErrors.Length);
            for (var i = 0; i < detailErrors.Length; i++)
            {
                var expectedError = errors[i];
                var detail = detailErrors[i];
                Assert.Equal(expectedError.ErrorCode, detail.Code);
                Assert.Equal(expectedError.ErrorMessage, detail.Message);
                Assert.Equal(expectedError.PropertyName, detail.Target);
            }
        }

        [Fact]
        public async Task AsHttpResponseData_ExceptionIsAnUnexpectedException_ReturnsResponseWithGenericErrorAndStatusInternalServerError()
        {
            // arrange
            var request = new MockedHttpRequestData(new MockedFunctionContext());
            const string internalErrorMessage = "Something is not right";
            var exception = new ArgumentNullException(internalErrorMessage);

            // act
            var actual = await exception.AsHttpResponseDataAsync(request.HttpRequestData).ConfigureAwait(false);
            var actualFunctionError = JsonSerializer.Deserialize<ErrorResponse>(
                ((MemoryStream)actual.Body).ToArray(),
                new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase })!;
            var error = actualFunctionError.Error;

            // assert
            Assert.Equal(HttpStatusCode.InternalServerError, actual.StatusCode);
            Assert.Equal("INTERNAL_ERROR", error.Code);
            Assert.Equal("An error occured while processing the request.", error.Message);
            Assert.Null(error.Target);
            Assert.Null(error.Details);
        }

        [Fact]
        public async Task AsHttpResponseData_ExceptionIsDataAnnotationException_ReturnsResponseWithGenericErrorAndStatusValidationError()
        {
            // arrange
            var request = new MockedHttpRequestData(new MockedFunctionContext());
            const string validationErrorMessage = nameof(BundleCreatedResponse.BundleIdAlreadyInUse);
            var exception = new System.ComponentModel.DataAnnotations.ValidationException(validationErrorMessage);

            // act
            var actual = await exception.AsHttpResponseDataAsync(request.HttpRequestData).ConfigureAwait(false);
            var actualFunctionError = JsonSerializer.Deserialize<ErrorResponse>(
                ((MemoryStream)actual.Body).ToArray(),
                new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase })!;
            var error = actualFunctionError.Error;

            // assert
            Assert.Equal(HttpStatusCode.BadRequest, actual.StatusCode);
            Assert.Equal("VALIDATION_EXCEPTION", error.Code);
            Assert.Equal(nameof(BundleCreatedResponse.BundleIdAlreadyInUse), error.Message);
            Assert.Null(error.Target);
            Assert.Null(error.Details);
        }

        [Fact]
        public async Task AsHttpResponseData_ContentTypeIsXml_ReturnsResponseSerializedAsXml()
        {
            // arrange
            const string expectedXml = "<?xml version=\"1.0\" encoding=\"utf-8\"?><Error><Code>VALIDATION_EXCEPTION</Code><Message>BundleIdAlreadyInUse</Message></Error>";
            var request = new MockedHttpRequestData(new MockedFunctionContext());
            request.HttpRequestDataMock.Setup(x => x.Headers).Returns(new Microsoft.Azure.Functions.Worker.Http.HttpHeadersCollection(
                new[] { new KeyValuePair<string, IEnumerable<string>>(HeaderNames.ContentType, new[] { MediaTypeNames.Application.Xml }) }));
            const string validationErrorMessage = nameof(BundleCreatedResponse.BundleIdAlreadyInUse);
            var exception = new System.ComponentModel.DataAnnotations.ValidationException(validationErrorMessage);

            // act
            var actual = await exception.AsHttpResponseDataAsync(request.HttpRequestData).ConfigureAwait(false);
            var content = Encoding.UTF8.GetString(((MemoryStream)actual.Body).ToArray());

            // assert
            Assert.Equal(expectedXml, content);
        }

        [Fact]
        public async Task AsHttpResponseData_ContentTypeIsJson_ReturnsResponseSerializedAsJson()
        {
            // arrange
            const string expectedXml = "{\"error\":{\"code\":\"VALIDATION_EXCEPTION\",\"message\":\"BundleIdAlreadyInUse\"}}";
            var request = new MockedHttpRequestData(new MockedFunctionContext());
            request.HttpRequestDataMock.Setup(x => x.Headers).Returns(new Microsoft.Azure.Functions.Worker.Http.HttpHeadersCollection(
                new[] { new KeyValuePair<string, IEnumerable<string>>(HeaderNames.ContentType, new[] { MediaTypeNames.Application.Json }) }));
            const string validationErrorMessage = nameof(BundleCreatedResponse.BundleIdAlreadyInUse);
            var exception = new System.ComponentModel.DataAnnotations.ValidationException(validationErrorMessage);

            // act
            var actual = await exception.AsHttpResponseDataAsync(request.HttpRequestData).ConfigureAwait(false);
            var content = Encoding.UTF8.GetString(((MemoryStream)actual.Body).ToArray());

            // assert
            Assert.Equal(expectedXml, content);
        }

        private sealed class FakeLogger : ILogger
        {
            public int LogCallCount { get; private set; }

            public void Log<TState>(
                LogLevel logLevel,
                EventId eventId,
                TState state,
                Exception exception,
                Func<TState, Exception, string> formatter)
            {
                ++LogCallCount;
            }

            public bool IsEnabled(LogLevel logLevel)
            {
                return true;
            }

            public IDisposable BeginScope<TState>(TState state)
            {
                throw new NotSupportedException();
            }
        }
    }
}
