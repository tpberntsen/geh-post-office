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
using Energinet.DataHub.PostOffice.Common.Extensions;
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
    }
}
