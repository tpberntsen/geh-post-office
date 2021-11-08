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
using Energinet.DataHub.MessageHub.Model.DataAvailable;
using Energinet.DataHub.MessageHub.Model.Model;
using Energinet.DataHub.PostOffice.Application.Commands;
using Energinet.DataHub.PostOffice.EntryPoint.SubDomain.Functions;
using Energinet.DataHub.PostOffice.Infrastructure.Mappers;
using Energinet.DataHub.PostOffice.Tests.Common;
using MediatR;
using Moq;
using Xunit;
using Xunit.Categories;

namespace Energinet.DataHub.PostOffice.Tests.Hosts.SubDomain
{
    [UnitTest]
    public sealed class DataAvailableAzureFunctionTests
    {
        [Fact]
        public async Task Run_ValidInput_CallsMediator()
        {
            // Arrange
            var mockedMediator = new Mock<IMediator>();
            var mockedParser = new Mock<IDataAvailableNotificationParser>();
            var mockedMapper = new DataAvailableMapper();
            var mockedFunctionContext = new MockedFunctionContext();

            var target = new DataAvailableInbox(mockedMediator.Object, mockedParser.Object, mockedMapper);

            var fakeProtobuf = Array.Empty<byte>();
            var fakeDto = new DataAvailableNotificationDto(
                Guid.NewGuid(),
                new GlobalLocationNumberDto("fake_value"),
                new MessageTypeDto("fake_value"),
                DomainOrigin.TimeSeries,
                false,
                0);

            mockedParser.Setup(x => x.Parse(fakeProtobuf)).Returns(fakeDto);

            // Act
            await target.RunAsync(fakeProtobuf, mockedFunctionContext).ConfigureAwait(false);

            // Assert
            mockedMediator.Verify(mediator => mediator.Send(It.IsAny<DataAvailableNotificationCommand>(), default));
        }

        [Fact]
        public async Task Run_InvalidMessage_IsHandled()
        {
            // Arrange
            var mockedMediator = new Mock<IMediator>();
            var mockedParser = new Mock<IDataAvailableNotificationParser>();
            var mockedMapper = new DataAvailableMapper();
            var mockedFunctionContext = new MockedFunctionContext();

            var target = new DataAvailableInbox(mockedMediator.Object, mockedParser.Object, mockedMapper);

            byte[] fakeProtobuf = null!;

            // Act Assert
            await Assert
                .ThrowsAsync<ArgumentNullException>(async () => await target.RunAsync(fakeProtobuf, mockedFunctionContext).ConfigureAwait(false))
                .ConfigureAwait(false);
        }

        [Fact]
        public async Task Run_HandlerException_ParseInvalid_IsHandled()
        {
            // Arrange
            var mockedMediator = new Mock<IMediator>();
            var mockedParser = new Mock<IDataAvailableNotificationParser>();
            var mockedMapper = new DataAvailableMapper();
            var mockedFunctionContext = new MockedFunctionContext();

            var target = new DataAvailableInbox(mockedMediator.Object, mockedParser.Object, mockedMapper);

            var fakeProtobuf = System.Text.Encoding.ASCII.GetBytes("invalid contract");

            await Assert
                .ThrowsAsync<ArgumentNullException>(async () => await target.RunAsync(fakeProtobuf, mockedFunctionContext).ConfigureAwait(false))
                .ConfigureAwait(false);
        }
    }
}
