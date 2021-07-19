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

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Energinet.DataHub.PostOffice.Application;
using Energinet.DataHub.PostOffice.Application.GetMessage.Interfaces;
using Energinet.DataHub.PostOffice.Application.GetMessage.Queries;
using Energinet.DataHub.PostOffice.Domain;
using Energinet.DataHub.PostOffice.Infrastructure;
using Energinet.DataHub.PostOffice.Infrastructure.GetMessage;
using FluentAssertions;
using Microsoft.Azure.Cosmos;
using Moq;
using Moq.Protected;
using NSubstitute;
using Xunit;
using Xunit.Categories;

namespace Energinet.DataHub.PostOffice.Tests.GetMessage
{
    [UnitTest]
    public class CurrentDataAvailableRequestSetTests
    {
        [Fact]
        public async Task GetCurrentDataAvailableRequestSet_Test()
        {
            // Arrange
            var documentStore = new Mock<IDocumentStore<DataAvailable>>();
            documentStore
                .Setup(c => c.GetDocumentsAsync(It.IsAny<string>(), It.IsAny<List<KeyValuePair<string, string>>>()))
                .ReturnsAsync(Helpers.TestData.GetRandomValidDataAvailables(2));

            var dataAvailableStorageService = new DataAvailableStorageService(documentStore.Object);
            var messageResponseStorage = new Mock<IMessageReplyStorage>();
            var contentPathStrategyFactory = new Mock<IGetContentPathStrategyFactory>();

            var dataAvailableController = new DataAvailableController(dataAvailableStorageService, messageResponseStorage.Object, contentPathStrategyFactory.Object);

            // Act
            var result = await dataAvailableController
                .GetCurrentDataAvailableRequestSetAsync(new GetMessageQuery("recipient")).ConfigureAwait(false);

            // Assert
            result.Should().NotBeNull();
        }
    }
}
