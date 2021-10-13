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
using System.Threading.Tasks;
using Energinet.DataHub.PostOffice.Infrastructure.Common;
using Xunit;
using Xunit.Categories;

namespace Energinet.DataHub.PostOffice.Tests.Common.Extensions
{
    [UnitTest]
    public class CosmosExtensionsTests
    {
        [Fact]
        public async Task FirstOrDefaultAsync_EmptyCollection_ReturnsNull()
        {
            // Arrange
            var collection = GetCollectionAsync<object>();

            // Act
            var actual = await collection.FirstOrDefaultAsync().ConfigureAwait(false);

            // Assert
            Assert.Null(actual);
        }

        [Fact]
        public async Task FirstOrDefaultAsync_SingleElement_ReturnsThatElement()
        {
            // Arrange
            var expected = new { Value = 5 };
            var collection = GetCollectionAsync<object>(expected);

            // Act
            var actual = await collection.FirstOrDefaultAsync().ConfigureAwait(false);

            // Assert
            Assert.Equal(expected, actual);
        }

        [Fact]
        public async Task FirstOrDefaultAsync_MultipleElements_ReturnsFirstElement()
        {
            // Arrange
            var expected = new { Value = 5 };
            var unexpected = new { Value = 8 };
            var collection = GetCollectionAsync<object>(expected, unexpected, unexpected, unexpected);

            // Act
            var actual = await collection.FirstOrDefaultAsync().ConfigureAwait(false);

            // Assert
            Assert.Equal(expected, actual);
        }

        [Fact]
        public async Task SingleAsync_EmptyCollection_ThrowsException()
        {
            // Arrange
            var collection = GetCollectionAsync<object>();

            // Act + Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() => collection.SingleAsync()).ConfigureAwait(false);
        }

        [Fact]
        public async Task SingleAsync_SingleElement_ReturnsThatElement()
        {
            // Arrange
            var expected = new { Value = 5 };
            var collection = GetCollectionAsync<object>(expected);

            // Act
            var actual = await collection.SingleAsync().ConfigureAwait(false);

            // Assert
            Assert.Equal(expected, actual);
        }

        [Fact]
        public async Task SingleAsync_MultipleElements_ThrowsException()
        {
            // Arrange
            var unexpected = new { Value = 5 };
            var collection = GetCollectionAsync<object>(unexpected, unexpected, unexpected);

            // Act + Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() => collection.SingleAsync()).ConfigureAwait(false);
        }

        [Fact]
        public async Task ToListAsync_MultipleElements_CreatesList()
        {
            // Arrange
            var expectedA = new { Value = 5 };
            var expectedB = new { Value = 6 };
            var expectedC = new { Value = 7 };
            var collection = GetCollectionAsync<object>(expectedA, expectedB, expectedC);

            // Act + Assert
            var actual = await collection.ToListAsync().ConfigureAwait(false);

            // Assert
            Assert.Equal(3, actual.Count);
            Assert.Equal(expectedA, actual[0]);
            Assert.Equal(expectedB, actual[1]);
            Assert.Equal(expectedC, actual[2]);
        }

        private static async IAsyncEnumerable<T> GetCollectionAsync<T>(params T[] input)
        {
            await Task.CompletedTask.ConfigureAwait(false);

            foreach (var i in input)
                yield return i;
        }
    }
}
