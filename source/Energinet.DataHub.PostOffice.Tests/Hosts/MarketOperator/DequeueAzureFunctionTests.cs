﻿// Copyright 2020 Energinet DataHub A/S
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
using Energinet.DataHub.PostOffice.Domain.Services;
using Energinet.DataHub.PostOffice.EntryPoint.MarketOperator.Functions;
using Energinet.DataHub.PostOffice.Tests.Common.Auth;
using FluentValidation;
using MediatR;
using Microsoft.Azure.Functions.Isolated.TestDoubles;
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
            var mockedRequestData = MockHelpers.CreateHttpRequestData(url: _functionRoute);

            var mockedMediator = new Mock<IMediator>();
            var mockedIdentity = new MockedMarketOperatorIdentity("fake_value");

            mockedMediator
                .Setup(x => x.Send(It.IsAny<DequeueCommand>(), default))
                .ReturnsAsync(new DequeueResponse(true));

            var target = new DequeueFunction(mockedMediator.Object, mockedIdentity, new Mock<ICorrelationIdProvider>().Object);

            // Act
            var response = await target.RunAsync(mockedRequestData).ConfigureAwait(false);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task Run_DidNotDequeue_ReturnsStatusNotFound()
        {
            // Arrange
            var mockedRequestData = MockHelpers.CreateHttpRequestData(url: _functionRoute);

            var mockedMediator = new Mock<IMediator>();
            var mockedIdentity = new MockedMarketOperatorIdentity("fake_value");

            mockedMediator
                .Setup(x => x.Send(It.IsAny<DequeueCommand>(), default))
                .ReturnsAsync(new DequeueResponse(false));

            var target = new DequeueFunction(mockedMediator.Object, mockedIdentity, new Mock<ICorrelationIdProvider>().Object);

            // Act
            var response = await target.RunAsync(mockedRequestData).ConfigureAwait(false);

            // Assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task Run_InvalidInput_IsHandled()
        {
            // Arrange
            var mockedRequestData = MockHelpers.CreateHttpRequestData(url: _functionRoute);

            var mockedMediator = new Mock<IMediator>();
            var mockedIdentity = new MockedMarketOperatorIdentity("fake_value");

            mockedMediator
                .Setup(x => x.Send(It.IsAny<DequeueCommand>(), default))
                .ThrowsAsync(new ValidationException("test"));

            var target = new DequeueFunction(mockedMediator.Object, mockedIdentity, new Mock<ICorrelationIdProvider>().Object);

            // Act
            var response = await target.RunAsync(mockedRequestData).ConfigureAwait(false);

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task Run_HandlerException_IsHandled()
        {
            // Arrange
            var mockedRequestData = MockHelpers.CreateHttpRequestData(url: _functionRoute);

            var mockedMediator = new Mock<IMediator>();
            var mockedIdentity = new MockedMarketOperatorIdentity("fake_value");

            mockedMediator
                .Setup(x => x.Send(It.IsAny<DequeueCommand>(), default))
                .ThrowsAsync(new InvalidOperationException("test"));

            var target = new DequeueFunction(mockedMediator.Object, mockedIdentity, new Mock<ICorrelationIdProvider>().Object);

            // Act
            var response = await target.RunAsync(mockedRequestData).ConfigureAwait(false);

            // Assert
            Assert.Equal(HttpStatusCode.InternalServerError, response.StatusCode);
        }
    }
}
