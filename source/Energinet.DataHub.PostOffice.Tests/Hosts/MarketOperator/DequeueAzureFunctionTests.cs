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
using System.Net;
using System.Threading.Tasks;
using Energinet.DataHub.PostOffice.Application.Commands;
using Energinet.DataHub.PostOffice.EntryPoint.MarketOperator.Functions;
using Energinet.DataHub.PostOffice.Tests.Common;
using MediatR;
using Moq;
using Xunit;
using Xunit.Categories;

namespace Energinet.DataHub.PostOffice.Tests.Hosts.MarketOperator
{
    [UnitTest]
    public sealed class DequeueAzureFunctionTests
    {
        private readonly Uri _functionRoute = new("https://localhost?recipient=0101010101010&bundleUuid=61835F24-3839-4E4B-B66D-E089042BB98A");

        [Fact]
        public async Task Run_DidDequeue_ReturnsStatusOk()
        {
            // Arrange
            var mockedRequestData = new MockedHttpRequestData(new MockedFunctionContext());
            mockedRequestData.HttpRequestDataMock
                .Setup(x => x.Url)
                .Returns(_functionRoute);

            var mockedMediator = new Mock<IMediator>();

            mockedMediator
                .Setup(x => x.Send(It.IsAny<DequeueCommand>(), default))
                .ReturnsAsync(new DequeueResponse(true));

            var target = new Dequeue(mockedMediator.Object);

            // Act
            var response = await target.RunAsync(mockedRequestData).ConfigureAwait(false);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task Run_DidNotDequeue_ReturnsStatusNotFound()
        {
            // Arrange
            var mockedRequestData = new MockedHttpRequestData(new MockedFunctionContext());
            mockedRequestData.HttpRequestDataMock
                .Setup(x => x.Url)
                .Returns(_functionRoute);

            var mockedMediator = new Mock<IMediator>();

            mockedMediator
                .Setup(x => x.Send(It.IsAny<DequeueCommand>(), default))
                .ReturnsAsync(new DequeueResponse(false));

            var target = new Dequeue(mockedMediator.Object);

            // Act
            var response = await target.RunAsync(mockedRequestData).ConfigureAwait(false);

            // Assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        //[Fact]
        //public void Run_InvalidInput_IsHandled()
        //{
        //    // TODO: Error handling not defined.
        //}

        //[Fact]
        //public void Run_HandlerException_IsHandled()
        //{
        //    // TODO: Error handling not defined.
        //}
    }
}
