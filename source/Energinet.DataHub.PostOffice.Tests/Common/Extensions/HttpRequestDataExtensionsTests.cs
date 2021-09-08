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
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Energinet.DataHub.PostOffice.Common.Extensions;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.Azure.Functions.Worker.Http;
using Moq;
using Xunit;
using Xunit.Categories;

namespace Energinet.DataHub.PostOffice.Tests.Common.Extensions
{
    [UnitTest]
    public class HttpRequestDataExtensionsTests
    {
        [Fact]
        public void CreateResponse_ValidStream_ReturnsResponse()
        {
            // arrange
            using var stream = new MemoryStream();
            var request = new MockedHttpRequestData(new MockedFunctionContext());
            var response = request.HttpResponseDataMock;
            request.HttpRequestDataMock
                .Setup(x => x.CreateResponse())
                .Returns(response.Object);

            // act
            request.HttpRequestData.CreateResponse(stream);

            // assert
            // ReSharper disable once AccessToDisposedClosure
            response.VerifySet(x => x.Body = stream, Times.Once());
            response.VerifySet(x => x.StatusCode = HttpStatusCode.OK, Times.Once());
        }

        [Fact]
        public void CreateResponse_SourceIsNull_ThrowsException()
        {
            // arrange
            var request = (HttpRequestData)null!;

            // act, assert
            Assert.Throws<ArgumentNullException>(() => request.CreateResponse(new MemoryStream()));
        }

        [Fact]
        public async Task ProcessAsync_WorkerFinishesSuccessfully_ReturnsResponseFromWorker()
        {
            // arrange
            var request = new MockedHttpRequestData(new MockedFunctionContext()).HttpRequestData;
            const string responseData = "Some data";

            // act
            var actual = await request.ProcessAsync(() => Task.FromResult(request.CreateResponse(
                new MemoryStream(Encoding.UTF8.GetBytes(responseData))))).ConfigureAwait(false);
            var actualResponseMessage = Encoding.UTF8.GetString(((MemoryStream)actual.Body).ToArray());

            // assert
            Assert.Equal(HttpStatusCode.OK, actual.StatusCode);
            Assert.Equal(responseData, actualResponseMessage);
        }

        [Fact]
        public async Task ProcessAsync_WorkerThrows_ReturnsInternalServerError()
        {
            // arrange
            var request = new MockedHttpRequestData(new MockedFunctionContext()).HttpRequestData;
            const string internalServerErrorMessage = "Something's not right";

            // act
            var actual = await request.ProcessAsync(async () =>
            {
                await Task.FromException(new InvalidOperationException(internalServerErrorMessage)).ConfigureAwait(false);
                return request.CreateResponse(HttpStatusCode.OK);
            }).ConfigureAwait(false);
            var actualResponseMessage = Encoding.UTF8.GetString(((MemoryStream)actual.Body).ToArray());

            // assert
            Assert.Equal(HttpStatusCode.InternalServerError, actual.StatusCode);
            Assert.NotEmpty(actualResponseMessage);
            Assert.DoesNotContain(internalServerErrorMessage, actualResponseMessage, StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public async Task ProcessAsync_WorkerThrowsValidationException_ReturnsBadRequestWithValidationErrorMessage()
        {
            // arrange
            var request = new MockedHttpRequestData(new MockedFunctionContext()).HttpRequestData;
            const string validationErrorMessage = "Something is not right";

            // act
            var actual = await request.ProcessAsync(async () =>
            {
                await Task.FromException(new ValidationException(string.Empty, new[] { new ValidationFailure("propertyName", validationErrorMessage) })).ConfigureAwait(false);
                return request.CreateResponse(HttpStatusCode.OK);
            }).ConfigureAwait(false);
            var actualResponseMessage = Encoding.UTF8.GetString(((MemoryStream)actual.Body).ToArray());

            // assert
            Assert.Equal(HttpStatusCode.BadRequest, actual.StatusCode);
            Assert.Contains(validationErrorMessage, actualResponseMessage, StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public async Task ProcessAsync_SourceIsNull_Throws()
        {
            // arrange, act, assert
            await Assert.ThrowsAsync<ArgumentNullException>(() =>
                ((HttpRequestData)null!).ProcessAsync(() =>
                    Task.FromResult(new MockedHttpRequestData(new MockedFunctionContext()).HttpResponseData))).ConfigureAwait(false);
        }

        [Fact]
        public async Task ProcessAsync_WorkerIsNull_Throws()
        {
            // arrange
            var mockedHttpRequestData = new MockedHttpRequestData(new MockedFunctionContext());

            // act, assert
            await Assert.ThrowsAsync<ArgumentNullException>(() =>
                mockedHttpRequestData.HttpRequestData.ProcessAsync(null!)).ConfigureAwait(false);
        }
    }
}
