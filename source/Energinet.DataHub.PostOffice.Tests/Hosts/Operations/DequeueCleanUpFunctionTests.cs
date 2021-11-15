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
using System.Threading.Tasks;
using Energinet.DataHub.PostOffice.Application.Commands;
using Energinet.DataHub.PostOffice.EntryPoint.Operations.Functions;
using Energinet.DataHub.PostOffice.Tests.Common;
using FluentValidation;
using MediatR;
using Moq;
using Xunit;
using Xunit.Categories;

namespace Energinet.DataHub.PostOffice.Tests.Hosts.Operations
{
    [UnitTest]
    public class DequeueCleanUpFunctionTests
    {
        [Fact]
        public async Task Run_ValidInput_CallsMediator()
        {
            // Arrange
            var mockedMediator = new Mock<IMediator>();
            var mockedFunctionContext = new MockedFunctionContext();

            mockedMediator
                .Setup(x => x.Send(It.IsAny<DequeueCleanUpCommand>(), default))
                .ReturnsAsync(new OperationResponse(true));

            var target = new DequeueCleanUpFunction(mockedMediator.Object);

            var fakeMessageBundleId = Guid.NewGuid().ToString();

            // Act
            await target.RunAsync(fakeMessageBundleId, mockedFunctionContext).ConfigureAwait(false);

            // Assert
            mockedMediator
                .Verify(mediator => mediator
                    .Send(It.IsAny<DequeueCleanUpCommand>(), default));
        }

        [Fact]
        public async Task Run_InValidInput_MessageIsNull()
        {
            // Arrange
            var mockedMediator = new Mock<IMediator>();
            var mockedFunctionContext = new MockedFunctionContext();

            mockedMediator
                .Setup(x => x.Send(It.IsAny<DequeueCleanUpCommand>(), default))
                .ReturnsAsync(new OperationResponse(true));

            var target = new DequeueCleanUpFunction(mockedMediator.Object);

            // Act
            var task = target.RunAsync(null!, mockedFunctionContext).ConfigureAwait(false);

            // Assert
            await Assert.ThrowsAsync<ArgumentNullException>(async () => await task).ConfigureAwait(false);
        }

        [Fact]
        public async Task Run_InValidInput_MediatorReturnsException()
        {
            // Arrange
            var mockedMediator = new Mock<IMediator>();
            var mockedFunctionContext = new MockedFunctionContext();

            mockedMediator
                .Setup(x => x.Send(It.IsAny<DequeueCleanUpCommand>(), default))
                .Throws(new ValidationException("Test message"));

            var target = new DequeueCleanUpFunction(mockedMediator.Object);

            var fakeMessageBundleId = Guid.NewGuid().ToString();

            // Act
            var task = target.RunAsync(fakeMessageBundleId, mockedFunctionContext).ConfigureAwait(false);

            // Assert
            await Assert.ThrowsAsync<ValidationException>(async () => await task).ConfigureAwait(false);
        }
    }
}
