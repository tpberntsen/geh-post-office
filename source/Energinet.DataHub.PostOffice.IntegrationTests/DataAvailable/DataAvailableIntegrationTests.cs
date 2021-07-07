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
using System.Threading;
using System.Threading.Tasks;
using Energinet.DataHub.PostOffice.Application.DataAvailable;
using FluentAssertions;
using MediatR;
using Xunit;

namespace Energinet.DataHub.PostOffice.IntegrationTests.DataAvailable
{
    public class DataAvailableIntegrationTests : IntegrationTestHost
    {
        private readonly IMediator _mediator;

        public DataAvailableIntegrationTests()
        {
            _mediator = GetService<IMediator>();
        }

        [Fact]
        public async Task Test_DataAvailable_Integration()
        {
            // Arrange
            var dataAvailableCommand = GetDataAvailableCommand();

            // Act
            var result = await _mediator.Send(dataAvailableCommand, CancellationToken.None).ConfigureAwait(false);

            // Assert
            result.Should().BeTrue();
        }

        private static DataAvailableCommand GetDataAvailableCommand()
        {
            return new DataAvailableCommand(
                Guid.NewGuid().ToString(),
                Guid.NewGuid().ToString(),
                "MessageType",
                "Origin",
                false,
                1);
        }
    }
}
