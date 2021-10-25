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
using System.IO;
using System.Threading.Tasks;
using Energinet.DataHub.MessageHub.Core.Storage;
using Energinet.DataHub.PostOffice.Infrastructure.Services;
using Moq;
using Xunit;
using Xunit.Categories;

namespace Energinet.DataHub.PostOffice.Tests.Infrastructure.Services
{
    [UnitTest]
    public class MarketOperatorDataStorageServiceTests
    {
        [Fact]
        public async Task GetData_UriIsNull_Throws()
        {
            // arrange
            var target = new MarketOperatorDataStorageService(new Mock<IStorageHandler>().Object);

            // act, assert
            await Assert.ThrowsAsync<ArgumentNullException>(() => target.GetMarketOperatorDataAsync(null!)).ConfigureAwait(false);
        }

        [Fact]
        public async Task GetData_UriIsValid_ReturnsContent()
        {
            // arrange
            await using var expectedStream = new MemoryStream();

            var path = new Uri("http://localhost");

            var storageHandlerMock = new Mock<IStorageHandler>();
            storageHandlerMock.Setup(x => x.GetStreamFromStorageAsync(path)).Returns(Task.FromResult<Stream>(expectedStream));

            var target = new MarketOperatorDataStorageService(storageHandlerMock.Object);

            // act
            var actual = await target.GetMarketOperatorDataAsync(path).ConfigureAwait(false);

            // assert
            Assert.Equal(expectedStream, actual);
        }
    }
}
