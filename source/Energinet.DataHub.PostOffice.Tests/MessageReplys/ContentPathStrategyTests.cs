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
using System.Threading;
using System.Threading.Tasks;
using Azure.Messaging.ServiceBus;
using Energinet.DataHub.PostOffice.Domain;
using Energinet.DataHub.PostOffice.Infrastructure.ContentPath;
using Energinet.DataHub.PostOffice.Infrastructure.GetMessage;
using FluentAssertions;
using FluentAssertions.Common;
using Google.Protobuf;
using Moq;
using Xunit;
using Xunit.Categories;

namespace Energinet.DataHub.PostOffice.Tests.MessageReplys
{
    [UnitTest]
    public class ContentPathStrategyTests
    {
        [Fact]
        public async Task Test_ContentPathFromSubDomainStrategy_Failure()
        {
            // Arrange
            var requestData = new RequestData() { Origin = "charges", Uuids = new List<string>() { Guid.NewGuid().ToString() } };
            var responseContract = new Contracts.DatasetReply()
            {
                Failure = new Contracts.DatasetReply.Types.RequestFailure()
                {
                    UUID = { requestData.Uuids },
                    Reason = Contracts.DatasetReply.Types.RequestFailure.Types.Reason.DatasetNotFound,
                    FailureDescription = "Failure",
                },
            };

            var contentPathFromSubDomain = SetUp_ContentPathStrategyTest(responseContract);

            // Act
            var messageReply = await contentPathFromSubDomain
                .GetContentPathAsync(requestData)
                .ConfigureAwait(false);

            // Assert
            Assert.Equal(Domain.Enums.MessageReplyFailureReason.DatasetNotFound, messageReply.FailureReason);
        }

        [Fact]
        public async Task Test_ContentPathFromSubDomainStrategy_Success()
        {
            // Arrange
            var requestData = new RequestData() { Origin = "charges", Uuids = new List<string>() { Guid.NewGuid().ToString() } };
            var responseContract = new Contracts.DatasetReply()
            {
                Success = new Contracts.DatasetReply.Types.FileResource()
                {
                    Uri = "https://testme.me", UUID = { requestData.Uuids },
                },
            };

            var contentPathFromSubDomain = SetUp_ContentPathStrategyTest(responseContract);

            // Act
            var messageReply = await contentPathFromSubDomain
                .GetContentPathAsync(requestData)
                .ConfigureAwait(false);

            // Assert
            messageReply.FailureReason.Should().Be(null);
        }

        private static ContentPathFromSubDomain SetUp_ContentPathStrategyTest(Contracts.DatasetReply response)
        {
            Environment.SetEnvironmentVariable("MessageReplyTopic", "queueName");

            var contractInBytes = new ReadOnlyMemory<byte>(response.ToByteArray());
            var serviceBusReceivedMessage = new Mock<ServiceBusReceivedMessage>(contractInBytes);

            var mockServiceBusSessionReceiver = new Mock<ServiceBusSessionReceiver>();
            mockServiceBusSessionReceiver
                .Setup(e => e.ReceiveMessageAsync(It.IsAny<TimeSpan>(), default(CancellationToken)))
                .ReturnsAsync(serviceBusReceivedMessage.Object);

            var mockServiceBusSender = new Mock<ServiceBusSender>();
            var mockServiceBusClient = new Mock<ServiceBusClient>();

            mockServiceBusClient
                .Setup(e => e.AcceptSessionAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), default(ServiceBusSessionReceiverOptions), default))
                .ReturnsAsync(mockServiceBusSessionReceiver.Object);
            mockServiceBusClient
                .Setup(e => e.CreateSender(It.IsAny<string>()))
                .Returns(mockServiceBusSender.Object);

            mockServiceBusSender.Setup(e => e
                .SendMessageAsync(It.IsAny<ServiceBusMessage>(), default(CancellationToken)))
                .Returns(Task.CompletedTask);

            var mockSendMessageToServiceBus = new Mock<SendMessageToServiceBus>(mockServiceBusClient.Object);
            var mockGetPathToDataFromServiceBus = new GetPathToDataFromServiceBus(mockServiceBusClient.Object);

            var contentPathFromSubDomain = new ContentPathFromSubDomain(mockSendMessageToServiceBus.Object, mockGetPathToDataFromServiceBus);
            return contentPathFromSubDomain;
        }
    }
}
