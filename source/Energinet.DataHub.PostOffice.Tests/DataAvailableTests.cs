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
using Energinet.DataHub.PostOffice.Application;
using Energinet.DataHub.PostOffice.Application.Commands;
using Energinet.DataHub.PostOffice.Application.DataAvailable;
using Energinet.DataHub.PostOffice.Application.Handlers;
using Energinet.DataHub.PostOffice.Domain.Model;
using Energinet.DataHub.PostOffice.Domain.Repositories;
using Energinet.DataHub.PostOffice.Inbound.Parsing;
using Energinet.DataHub.PostOffice.Infrastructure.Mappers;
using FluentAssertions;
using Google.Protobuf;
using Moq;
using Xunit;
using Xunit.Categories;

namespace Energinet.DataHub.PostOffice.Tests
{
    [UnitTest]
    public class DataAvailableTests
    {
        [Fact]
        public async Task Validate_DataAvailable_Handler()
        {
            // Arrange
            var dataAvailableRepositoryMock = new Mock<IDataAvailableNotificationRepository>();

            dataAvailableRepositoryMock.Setup(e => e.CreateAsync(It.IsAny<DataAvailableNotification>())).Returns(Task.CompletedTask);

            var command = new DataAvailableCommand(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), Origin.Charges.ToString(), It.IsAny<bool>(), It.IsAny<int>());
            var handler = new DataAvailableNotificationHandler(dataAvailableRepositoryMock.Object);

            // Act
            var result = await handler.Handle(command, System.Threading.CancellationToken.None).ConfigureAwait(false);

            // Assert
            result.Should().BeOfType(typeof(DataAvailableNotificationResponse));
        }

        [Fact]
        public void Validate_DataAvailable_Protobuf_Contract_Parser()
        {
            // Arrange
            IMapper<Contracts.DataAvailable, DataAvailableCommand> mapper = new DataAvailableMapper();
            var dataAvailableContractParser = new DataAvailableContractParser(mapper);
            var dataContract = GetDataAvailableProtobufContract();
            var inputContractBytes = dataContract.ToByteArray();

            // Act
            var parseResult = dataAvailableContractParser.Parse(inputContractBytes);

            // Assert
            parseResult.UUID.Should().Be(dataContract.UUID);
        }

        private static Contracts.DataAvailable GetDataAvailableProtobufContract()
        {
            var contract =
                new Contracts.DataAvailable
                {
                    UUID = Guid.NewGuid().ToString(),
                    Recipient = Guid.NewGuid().ToString(),
                    Origin = "Origin",
                    MessageType = "Type",
                    RelativeWeight = 1,
                    SupportsBundling = false,
                };
            return contract;
        }
    }
}
