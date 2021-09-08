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
using System.Text.Json;
using Energinet.DataHub.PostOffice.Common.Extensions;
using Energinet.DataHub.PostOffice.Common.Model;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.Extensions.Logging;
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
            Assert.Equal(1, logger.LogCallCount);
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
        public void AsHttpResponseData_ExceptionIsValidationException_ReturnsResponseWithAllErrorsAndStatusBadRequest()
        {
            var request = new MockedHttpRequestData(new MockedFunctionContext());
            var errors = new[]
            {
                new ValidationFailure("prop1", "err1") { ErrorCode = "code1" },
                new ValidationFailure("prop2", "err2") { ErrorCode = "code2" }
            };
            var exception = new ValidationException(errors);

            // act
            var actual = exception.AsHttpResponseData(request.HttpRequestData);
            var actualFunctionError = JsonSerializer.Deserialize<FunctionError>(
                ((MemoryStream)actual.Body).ToArray(),
                new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase })!;

            // assert
            Assert.Equal(HttpStatusCode.BadRequest, actual.StatusCode);
            Assert.Equal(2, actualFunctionError.Errors.Length);
            for (var i = 0; i < actualFunctionError.Errors.Length; i++)
            {
                var expectedError = errors[i];
                var (actualCode, actualMessage, actualTarget) = actualFunctionError.Errors[i];
                Assert.Equal(expectedError.ErrorCode, actualCode);
                Assert.Equal(expectedError.ErrorMessage, actualMessage);
                Assert.Equal(expectedError.PropertyName, actualTarget);
            }
        }

        [Fact]
        public void AsHttpResponseData_ExceptionIsAnUnexpectedException_ReturnsResponseWithGenericErrorAndStatusInternalServerError()
        {
            var request = new MockedHttpRequestData(new MockedFunctionContext());
            const string internalErrorMessage = "Something is not right";
            var exception = new ArgumentNullException(internalErrorMessage);

            // act
            var actual = exception.AsHttpResponseData(request.HttpRequestData);
            var actualFunctionError = JsonSerializer.Deserialize<FunctionError>(
                ((MemoryStream)actual.Body).ToArray(),
                new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase })!;
            var (actualCode, actualMessage, actualTarget) = actualFunctionError.Errors.Single();

            // assert
            Assert.Equal(HttpStatusCode.InternalServerError, actual.StatusCode);
            Assert.Equal("INTERNAL_ERROR", actualCode);
            Assert.Equal("An error occured while processing the request", actualMessage);
            Assert.Null(actualTarget);
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
