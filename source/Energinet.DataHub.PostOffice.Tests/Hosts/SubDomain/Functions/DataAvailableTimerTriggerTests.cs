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
using System.Linq;
using System.Threading.Tasks;
using Energinet.DataHub.MessageHub.Model.DataAvailable;
using Energinet.DataHub.MessageHub.Model.Exceptions;
using Energinet.DataHub.MessageHub.Model.Model;
using Energinet.DataHub.PostOffice.Application;
using Energinet.DataHub.PostOffice.Application.Commands;
using Energinet.DataHub.PostOffice.EntryPoint.SubDomain.Functions;
using Energinet.DataHub.PostOffice.Tests.Common;
using FluentValidation;
using MediatR;
using Microsoft.Azure.ServiceBus;
using Moq;
using Xunit;
using Xunit.Categories;

namespace Energinet.DataHub.PostOffice.Tests.Hosts.SubDomain.Functions
{
    [UnitTest]
    public class DataAvailableTimerTriggerTests
    {
        [Fact]
        public async Task RunAsync_NoErrors_CallsCompleteOnReceiver()
        {
            // arrange
            var messages = new[]
            {
                MockedMessage.Create(Array.Empty<byte>(), Guid.NewGuid()),
                MockedMessage.Create(Array.Empty<byte>(), Guid.NewGuid())
            };

            var (target, receiverMock, _, parserMock, mapperMock) = Setup(messages);

            parserMock.Setup(x => x.Parse(It.IsAny<byte[]>()))
                .Returns(CreateDto(1));

            mapperMock.Setup(x => x.Map(It.IsAny<DataAvailableNotificationDto>()))
                .Returns(CreateCommand(1));

            var context = new MockedFunctionContext();

            // act
            await target.RunAsync(context).ConfigureAwait(false);

            // assert
            receiverMock.Verify(x => x.CompleteAsync(It.Is<IEnumerable<Message>>(y => y.Count() == 2)));
        }

        [Fact]
        public async Task RunAsync_UnexpectedException_BubblesUp()
        {
            // arrange
            var messages = new[] { MockedMessage.Create(Array.Empty<byte>(), Guid.NewGuid()) };

            var (target, _, mediatorMock, _, _) = Setup(messages);
            mediatorMock.Setup(x => x.Send(It.IsAny<DataAvailableNotificationCommand>(), default)).Throws(new ArgumentNullException());
            var context = new MockedFunctionContext();

            // act, assert
            await Assert.ThrowsAsync<ArgumentNullException>(() => target.RunAsync(context)).ConfigureAwait(false);
        }

        [Fact]
        public async Task RunAsync_ThrowsMessageHubException_DeadLetters()
        {
            // arrange
            var messages = new[] { MockedMessage.Create(new byte[] { 1 }, Guid.NewGuid()), MockedMessage.Create(new byte[] { 2 }, Guid.NewGuid()) };

            var (target, receiverMock, mediatorMock, parserMock, mapperMock) = Setup(messages);

            parserMock.Setup(x => x.Parse(It.Is<byte[]>(y => y[0] == 1)))
                .Returns(CreateDto(1));

            parserMock.Setup(x => x.Parse(It.Is<byte[]>(y => y[0] == 2)))
                .Returns(CreateDto(2));

            mapperMock.Setup(x => x.Map(It.Is<DataAvailableNotificationDto>(y => y.RelativeWeight == 1)))
                .Returns(CreateCommand(1));

            mapperMock.Setup(x => x.Map(It.Is<DataAvailableNotificationDto>(y => y.RelativeWeight == 2)))
                .Returns(CreateCommand(2));

            mediatorMock.Setup(
                    x => x.Send(It.Is<DataAvailableNotificationCommand>(y => y != null && y.Weight == 1), default))
                .Throws(new MessageHubException());

            var context = new MockedFunctionContext();

            // act
            await target.RunAsync(context).ConfigureAwait(false);

            // assert
            receiverMock.Verify(x => x.CompleteAsync(It.Is<IEnumerable<Message>>(y => y.Count() == 1)));
            receiverMock.Verify(x => x.DeadLetterAsync(It.Is<IEnumerable<Message>>(y => y.Count() == 1)));
        }

        [Fact]
        public async Task RunAsync_ThrowsFluentValidationException_DeadLetters()
        {
            // arrange
            var messages = new[] { MockedMessage.Create(new byte[] { 1 }, Guid.NewGuid()), MockedMessage.Create(new byte[] { 2 }, Guid.NewGuid()) };

            var (target, receiverMock, mediatorMock, parserMock, mapperMock) = Setup(messages);

            parserMock.Setup(x => x.Parse(It.Is<byte[]>(y => y[0] == 1)))
                .Returns(CreateDto(1));

            parserMock.Setup(x => x.Parse(It.Is<byte[]>(y => y[0] == 2)))
                .Returns(CreateDto(2));

            mapperMock.Setup(x => x.Map(It.Is<DataAvailableNotificationDto>(y => y.RelativeWeight == 1)))
                .Returns(CreateCommand(1));

            mapperMock.Setup(x => x.Map(It.Is<DataAvailableNotificationDto>(y => y.RelativeWeight == 2)))
                .Returns(CreateCommand(2));

            mediatorMock.Setup(
                    x => x.Send(It.Is<DataAvailableNotificationCommand>(y => y != null && y.Weight == 1), default))
                .Throws(new ValidationException(string.Empty));

            var context = new MockedFunctionContext();

            // act
            await target.RunAsync(context).ConfigureAwait(false);

            // assert
            receiverMock.Verify(x => x.CompleteAsync(It.Is<IEnumerable<Message>>(y => y.Count() == 1)));
            receiverMock.Verify(x => x.DeadLetterAsync(It.Is<IEnumerable<Message>>(y => y.Count() == 1)));
        }

        [Fact]
        public async Task RunAsync_ThrowsValidationException_DeadLetters()
        {
            // arrange
            var messages = new[] { MockedMessage.Create(new byte[] { 1 }, Guid.NewGuid()), MockedMessage.Create(new byte[] { 2 }, Guid.NewGuid()) };

            var (target, receiverMock, mediatorMock, parserMock, mapperMock) = Setup(messages);

            parserMock.Setup(x => x.Parse(It.Is<byte[]>(y => y[0] == 1)))
                .Returns(CreateDto(1));

            parserMock.Setup(x => x.Parse(It.Is<byte[]>(y => y[0] == 2)))
                .Returns(CreateDto(2));

            mapperMock.Setup(x => x.Map(It.Is<DataAvailableNotificationDto>(y => y.RelativeWeight == 1)))
                .Returns(CreateCommand(1));

            mapperMock.Setup(x => x.Map(It.Is<DataAvailableNotificationDto>(y => y.RelativeWeight == 2)))
                .Returns(CreateCommand(2));

            mediatorMock.Setup(
                    x => x.Send(It.Is<DataAvailableNotificationCommand>(y => y != null && y.Weight == 1), default))
                .Throws(new System.ComponentModel.DataAnnotations.ValidationException());

            var context = new MockedFunctionContext();

            // act
            await target.RunAsync(context).ConfigureAwait(false);

            // assert
            receiverMock.Verify(x => x.CompleteAsync(It.Is<IEnumerable<Message>>(y => y.Count() == 1)));
            receiverMock.Verify(x => x.DeadLetterAsync(It.Is<IEnumerable<Message>>(y => y.Count() == 1)));
        }

        private static DataAvailableNotificationDto CreateDto(int weight)
        {
            return new DataAvailableNotificationDto(
                Guid.NewGuid(),
                new GlobalLocationNumberDto("gln"),
                new MessageTypeDto("type"),
                DomainOrigin.Aggregations,
                true,
                weight);
        }

        private static DataAvailableNotificationCommand CreateCommand(int weight)
        {
            return new DataAvailableNotificationCommand(
                string.Empty,
                string.Empty,
                string.Empty,
                string.Empty,
                true,
                weight,
                1);
        }

        private static (DataAvailableTimerTrigger Target,
            Mock<IDataAvailableMessageReceiver> ReceiverMock,
            Mock<IMediator> MediatorMock,
            Mock<IDataAvailableNotificationParser> ParserMock,
            Mock<IMapper<DataAvailableNotificationDto, DataAvailableNotificationCommand>> MapperMock)
            Setup(IReadOnlyList<Message> messages)
        {
            var receiverMock = new Mock<IDataAvailableMessageReceiver>();
            receiverMock.Setup(x => x.ReceiveAsync()).Returns(Task.FromResult(messages));

            var mediatorMock = new Mock<IMediator>();
            var parserMock = new Mock<IDataAvailableNotificationParser>();
            var mapperMock = new Mock<IMapper<DataAvailableNotificationDto, DataAvailableNotificationCommand>>();

            var target = new DataAvailableTimerTrigger(
                mediatorMock.Object,
                receiverMock.Object,
                parserMock.Object,
                mapperMock.Object);

            return (target, receiverMock, mediatorMock, parserMock, mapperMock);
        }
    }
}
