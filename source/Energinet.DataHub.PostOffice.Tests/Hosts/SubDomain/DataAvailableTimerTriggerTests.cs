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
using System.Linq.Expressions;
using System.Threading.Tasks;
using Energinet.DataHub.MessageHub.Model.DataAvailable;
using Energinet.DataHub.MessageHub.Model.Model;
using Energinet.DataHub.MessageHub.Model.Protobuf;
using Energinet.DataHub.PostOffice.Application.Commands;
using Energinet.DataHub.PostOffice.EntryPoint.SubDomain.Functions;
using Energinet.DataHub.PostOffice.Tests.Common;
using Google.Protobuf;
using MediatR;
using Microsoft.Azure.ServiceBus;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using Xunit.Categories;
using DataAvailableNotificationDto = Energinet.DataHub.MessageHub.Model.Model.DataAvailableNotificationDto;

namespace Energinet.DataHub.PostOffice.Tests.Hosts.SubDomain
{
    [UnitTest]
    public sealed class DataAvailableTimerTriggerTests
    {
        [Fact]
        public async Task RunAsync_MessageNotParsable_IsDeadlettered()
        {
            // Arrange
            var mediator = new Mock<IMediator>();
            var messageReceiver = new Mock<IDataAvailableMessageReceiver>();

            var emptyMessage = new Message();

            messageReceiver
                .Setup(x => x.ReceiveAsync())
                .ReturnsAsync(new[] { emptyMessage });

            var target = new MockedDataAvailableTimerTrigger(
                mediator.Object,
                messageReceiver.Object,
                new DataAvailableNotificationParser());

            // Act
            await target.RunAsync(new MockedFunctionContext()).ConfigureAwait(false);

            // Assert
            messageReceiver.Verify(
                x => x.DeadLetterAsync(
                    It.Is<IEnumerable<Message>>(m => m.Single() == emptyMessage)),
                Times.Once);
        }

        [Fact]
        public async Task RunAsync_GoodMessage_IsCompleted()
        {
            // Arrange
            var goodDto = CreateDto();
            var goodMessage = CreateMessages(goodDto);

            var mediator = new Mock<IMediator>();
            mediator
                .Setup(m => m.Send(It.Is(ExpectedUuid(goodDto)), default))
                .ReturnsAsync(Unit.Value);

            var messageReceiver = new Mock<IDataAvailableMessageReceiver>();
            messageReceiver
                .Setup(x => x.ReceiveAsync())
                .ReturnsAsync(new[] { goodMessage });

            var target = new MockedDataAvailableTimerTrigger(
                mediator.Object,
                messageReceiver.Object,
                new DataAvailableNotificationParser());

            // Act
            await target.RunAsync(new MockedFunctionContext()).ConfigureAwait(false);

            // Assert
            messageReceiver.Verify(
                x => x.CompleteAsync(
                    It.Is<IEnumerable<Message>>(m => m.Single() == goodMessage)),
                Times.Once);
        }

        [Fact]
        public async Task RunAsync_GoodMessage_UpdatesSequenceNumber()
        {
            // Arrange
            var goodDto = CreateDto();
            var goodMessageA = CreateMessages(goodDto);
            var goodMessageB = CreateMessages(goodDto);

            var mediator = new Mock<IMediator>();
            mediator
                .Setup(m => m.Send(It.Is(ExpectedUuid(goodDto)), default))
                .ReturnsAsync(Unit.Value);

            var messageReceiver = new Mock<IDataAvailableMessageReceiver>();
            messageReceiver
                .Setup(x => x.ReceiveAsync())
                .ReturnsAsync(new[] { goodMessageA, goodMessageB });

            var target = new MockedDataAvailableTimerTrigger(
                mediator.Object,
                messageReceiver.Object,
                new DataAvailableNotificationParser());

            target.GetSequenceNumberCallback = m =>
            {
                if (m.Body == goodMessageA.Body)
                    return 1;

                if (m.Body == goodMessageB.Body)
                    return 2;

                return -1;
            };

            // Act
            await target.RunAsync(new MockedFunctionContext()).ConfigureAwait(false);

            // Assert
            mediator.Verify(
                x => x.Send(
                    It.Is<UpdateMaximumSequenceNumberCommand>(c => c.SequenceNumber == 2), default),
                Times.Once);
        }

        [Fact]
        public async Task RunAsync_BadMessage_IsDeadlettered()
        {
            // Arrange
            var badDto = CreateDto();
            var badMessage = CreateMessages(badDto);

            var mediator = new Mock<IMediator>();
            mediator
                .Setup(m => m.Send(It.Is(ExpectedUuid(badDto)), default))
                .ThrowsAsync(new InvalidOperationException());

            var messageReceiver = new Mock<IDataAvailableMessageReceiver>();
            messageReceiver
                .Setup(x => x.ReceiveAsync())
                .ReturnsAsync(new[] { badMessage });

            var target = new MockedDataAvailableTimerTrigger(
                mediator.Object,
                messageReceiver.Object,
                new DataAvailableNotificationParser());

            // Act
            await target.RunAsync(new MockedFunctionContext()).ConfigureAwait(false);

            // Assert
            messageReceiver.Verify(
                x => x.DeadLetterAsync(
                    It.Is<IEnumerable<Message>>(m => m.Single() == badMessage)),
                Times.Once);
        }

        [Fact]
        public async Task RunAsync_GoodAndBadMessage_IsHandled()
        {
            // Arrange
            var goodDto = CreateDto();
            var badDto = CreateDto();

            var goodMessage = CreateMessages(goodDto);
            var badMessage = CreateMessages(badDto);

            var mediator = new Mock<IMediator>();
            mediator
                .Setup(m => m.Send(It.Is(ExpectedUuid(badDto)), default))
                .ThrowsAsync(new InvalidOperationException());

            mediator
                .Setup(m => m.Send(It.Is(ExpectedUuid(goodDto)), default))
                .ReturnsAsync(Unit.Value);

            var messageReceiver = new Mock<IDataAvailableMessageReceiver>();
            messageReceiver
                .Setup(x => x.ReceiveAsync())
                .ReturnsAsync(new[] { badMessage, goodMessage });

            var target = new MockedDataAvailableTimerTrigger(
                mediator.Object,
                messageReceiver.Object,
                new DataAvailableNotificationParser());

            // Act
            await target.RunAsync(new MockedFunctionContext()).ConfigureAwait(false);

            // Assert
            messageReceiver.Verify(
                x => x.DeadLetterAsync(
                    It.Is<IEnumerable<Message>>(m => m.Single() == badMessage)),
                Times.Once);

            messageReceiver.Verify(
                x => x.CompleteAsync(
                    It.Is<IEnumerable<Message>>(m => m.Single() == goodMessage)),
                Times.Once);
        }

        private static DataAvailableNotificationDto CreateDto()
        {
            return new DataAvailableNotificationDto(
                Guid.NewGuid(),
                new GlobalLocationNumberDto(Guid.NewGuid().ToString()),
                new MessageTypeDto("fake_value"),
                DomainOrigin.Charges,
                false,
                10);
        }

        private static Expression<Func<InsertDataAvailableNotificationsCommand, bool>> ExpectedUuid(DataAvailableNotificationDto badDto)
        {
            return command => command.Notifications.Single().Uuid == badDto.Uuid.ToString();
        }

        private static Message CreateMessages(DataAvailableNotificationDto dto)
        {
            var protobuf = new DataAvailableNotificationContract
            {
                UUID = dto.Uuid.ToString(),
                Recipient = dto.Recipient.Value,
                Origin = dto.Origin.ToString(),
                MessageType = dto.MessageType.Value,
                RelativeWeight = dto.RelativeWeight,
                SupportsBundling = dto.SupportsBundling
            };

            return new Message(protobuf.ToByteArray());
        }

        private sealed class MockedDataAvailableTimerTrigger : DataAvailableTimerTrigger
        {
            public MockedDataAvailableTimerTrigger(
                IMediator mediator,
                IDataAvailableMessageReceiver messageReceiver,
                IDataAvailableNotificationParser dataAvailableNotificationParser)
                : base(
                    new Mock<ILogger<DataAvailableTimerTrigger>>().Object,
                    mediator,
                    messageReceiver,
                    dataAvailableNotificationParser)
            {
            }

            public Func<Message, long>? GetSequenceNumberCallback { get; set; }

            protected override long GetSequenceNumber(Message message)
            {
                return GetSequenceNumberCallback?.Invoke(message) ?? 0;
            }
        }
    }
}
