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
using Energinet.DataHub.PostOffice.Application.Commands;
using Energinet.DataHub.PostOffice.Application.Handlers;
using Energinet.DataHub.PostOffice.Domain.Model;
using Energinet.DataHub.PostOffice.Domain.Repositories;
using Moq;
using Xunit;
using Xunit.Categories;

namespace Energinet.DataHub.PostOffice.Tests.Handlers
{
    [UnitTest]
    public sealed class MaximumSequenceNumberCommandHandlerTests
    {
        [Fact]
        public async Task GetHandle_SequenceNumber_IsReturned()
        {
            // Arrange
            const long expected = 170989;

            var repository = new Mock<ISequenceNumberRepository>();
            repository.Setup(x => x.GetMaximumSequenceNumberAsync())
                .ReturnsAsync(new SequenceNumber(expected));

            var target = new MaximumSequenceNumberCommandHandler(repository.Object);

            // Act
            var command = new GetMaximumSequenceNumberCommand();
            var actual = await target.Handle(command, CancellationToken.None).ConfigureAwait(false);

            // Assert
            Assert.Equal(expected, actual);
        }

        [Fact]
        public async Task UpdateHandle_NullArgument_ThrowsException()
        {
            // Arrange
            var repository = new Mock<ISequenceNumberRepository>();
            var target = new MaximumSequenceNumberCommandHandler(repository.Object);
            var updateMaximumSequenceNumberCommand = (UpdateMaximumSequenceNumberCommand)null!;

            // Act + Assert
            await Assert
                .ThrowsAsync<ArgumentNullException>(() => target.Handle(updateMaximumSequenceNumberCommand, CancellationToken.None))
                .ConfigureAwait(false);
        }

        [Fact]
        public async Task UpdateHandle_SequenceNumber_IsSaved()
        {
            // Arrange
            const long sequenceNumber = 100;

            var repository = new Mock<ISequenceNumberRepository>();
            var target = new MaximumSequenceNumberCommandHandler(repository.Object);

            // Act
            var command = new UpdateMaximumSequenceNumberCommand(sequenceNumber);
            await target.Handle(command, CancellationToken.None).ConfigureAwait(false);

            // Assert
            repository.Verify(x => x.AdvanceSequenceNumberAsync(It.Is<SequenceNumber>(s => s.Value == sequenceNumber)));
        }
    }
}
