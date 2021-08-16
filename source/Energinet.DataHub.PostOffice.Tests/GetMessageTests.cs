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
using Energinet.DataHub.PostOffice.Application.GetMessage.Handlers;
using Energinet.DataHub.PostOffice.Application.GetMessage.Interfaces;
using Energinet.DataHub.PostOffice.Application.GetMessage.Queries;
using Energinet.DataHub.PostOffice.Domain;
using Energinet.DataHub.PostOffice.Domain.Enums;
using Energinet.DataHub.PostOffice.Domain.Repositories;
using Energinet.DataHub.PostOffice.Infrastructure.ContentPath;
using Energinet.DataHub.PostOffice.Infrastructure.GetMessage;
using FluentAssertions;
using Moq;
using Xunit;

namespace Energinet.DataHub.PostOffice.Tests
{
    public class GetMessageTests
    {
        [Fact]
        public async Task GetMessageHandler_CallFromMarketOperator_ResultMustMatch_Failure()
        {
            // Arrange
            var dataAvailableRepositoryMock = new Mock<IDataAvailableRepository>();
            var dataAvailableRepository = dataAvailableRepositoryMock.Object;
            GetDocumentsAsync(dataAvailableRepositoryMock);

            var messageReplyRepository = new Mock<IMessageReplyRepository>();
            messageReplyRepository
                .Setup(e => e.GetMessageReplyAsync(It.IsAny<string>()))
                .ReturnsAsync(It.IsAny<string>());

            var messageReply = new MessageReply() { DataPath = "https://testpath.com", FailureReason = MessageReplyFailureReason.DatasetNotFound };
            var strategyFactory = new GetContentPathStrategyFactory(GetContentPathStrategies(messageReply));
            var dataAvailableController = new DataAvailableController(dataAvailableRepository, messageReplyRepository.Object, strategyFactory);

            var storageServiceMock = new Mock<IStorageService>();
            GetMarketOperatorDataFromStorageService(storageServiceMock);

            var query = new GetMessageQuery(It.IsAny<string>());
            var handler = new GetMessageHandler(
                dataAvailableController,
                storageServiceMock.Object);

            // Act
            Func<Task> act = async () => { await handler.Handle(query, System.Threading.CancellationToken.None).ConfigureAwait(false); };

            // Assert
            await act.Should().ThrowAsync<Domain.Exceptions.MessageReplyException>().ConfigureAwait(false);
        }

        [Fact]
        public async Task GetMessageHandler_CallFromMarketOperator_ResultMustMatch_Success()
        {
            // Arrange
            var dataAvailableRepositoryMock = new Mock<IDataAvailableRepository>();
            var dataAvailableRepository = dataAvailableRepositoryMock.Object;
            GetDocumentsAsync(dataAvailableRepositoryMock);

            var messageReplyRepository = new Mock<IMessageReplyRepository>();
            messageReplyRepository
                .Setup(e => e.GetMessageReplyAsync(It.IsAny<string>()))
                .ReturnsAsync(It.IsAny<string>());

            var messageReply = new MessageReply() { DataPath = "https://testpath.com" };
            var strategyFactory = new GetContentPathStrategyFactory(GetContentPathStrategies(messageReply));
            var dataAvailableController = new DataAvailableController(dataAvailableRepository, messageReplyRepository.Object, strategyFactory);

            var storageServiceMock = new Mock<IStorageService>();
            GetMarketOperatorDataFromStorageService(storageServiceMock);

            var query = new GetMessageQuery(It.IsAny<string>());
            var handler = new GetMessageHandler(
                dataAvailableController,
                storageServiceMock.Object);

            // Act
            var result = await handler.Handle(query, System.Threading.CancellationToken.None).ConfigureAwait(false);

            // Assert
            result.Should().Be(GetStorageContentAsyncSimulatedData());
        }

        private static IEnumerable<IGetContentPathStrategy> GetContentPathStrategies(MessageReply messageReply)
        {
            var getPathToDataFromServiceBus = new Mock<IGetPathToDataFromServiceBus>();
            getPathToDataFromServiceBus.Setup(path => path.GetPathAsync(It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(messageReply);
            var sendMessageToServiceBus = new Mock<ISendMessageToServiceBus>();

            return new List<IGetContentPathStrategy>() { new ContentPathFromSavedResponse(), new ContentPathFromSubDomain(sendMessageToServiceBus.Object, getPathToDataFromServiceBus.Object) };
        }

        private static void GetMarketOperatorDataFromStorageService(Mock<IStorageService> storageService)
        {
            storageService.Setup(
                ss => ss.GetStorageContentAsync(
                    It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(GetStorageContentAsyncSimulatedData());
        }

        private static void GetDocumentsAsync(Mock<IDataAvailableRepository> dataAvailableRepository)
        {
            dataAvailableRepository
                .Setup(repository => repository.GetDataAvailableUuidsAsync(It.IsAny<string>()))
                .ReturnsAsync(new RequestData { Uuids = CreateListOfDataAvailableObjects().Select(dataAvailable => dataAvailable.Uuid) });
        }

        private static string GetStorageContentAsyncSimulatedData()
        {
            return "test data";
        }

        private static IList<DataAvailable> CreateListOfDataAvailableObjects()
        {
            return new List<DataAvailable>()
            {
                new DataAvailable(
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<bool>(),
                    It.IsAny<int>(),
                    It.IsAny<decimal>()),
                new DataAvailable(
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<bool>(),
                    It.IsAny<int>(),
                    It.IsAny<decimal>()),
            };
        }
    }
}
