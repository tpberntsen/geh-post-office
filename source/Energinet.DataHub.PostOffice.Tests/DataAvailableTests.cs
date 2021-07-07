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
using Energinet.DataHub.PostOffice.Application.DataAvailable;
using Energinet.DataHub.PostOffice.Domain;
using Energinet.DataHub.PostOffice.Inbound.Parsing;
using Energinet.DataHub.PostOffice.Infrastructure.Mappers;
using FluentAssertions;
using Google.Protobuf;
using Moq;
using Xunit;

namespace Energinet.DataHub.PostOffice.Tests
{
    public class DataAvailableTests
    {
        public DataAvailableTests()
        {
        }

        [Fact]
        public async Task Validate_DataAvailable_Handler()
        {
            // Arrange
            var documentStore = new Mock<IDocumentStore<Domain.DataAvailable>>();

            documentStore.Setup(e => e.SaveDocumentAsync(It.IsAny<DataAvailable>())).ReturnsAsync(true);

            DataAvailableCommand command = new DataAvailableCommand(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<int>());
            DataAvailableHandler handler = new DataAvailableHandler(documentStore.Object);

            // Act
            var result = await handler.Handle(command, System.Threading.CancellationToken.None).ConfigureAwait(false);

            // Assert
            result.Should().Be(true);
        }

        [Fact]
        public void Validate_DataAvailable_Protobuf_Contract_Parser()
        {
            // Arrange
            IMapper<Contracts.DataAvailable, DataAvailableCommand> mapper = new DataAvailableMapper();
            DataAvailableContractParser dataAvailableContractParser = new DataAvailableContractParser(mapper);
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
