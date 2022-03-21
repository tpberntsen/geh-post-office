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
using System.Linq;
using Energinet.DataHub.PostOffice.Domain.Model;
using Moq;
using Xunit;
using Xunit.Categories;

namespace Energinet.DataHub.PostOffice.Tests.Model
{
    [UnitTest]
    public sealed class BundleTests
    {
        [Fact]
        public void Dequeue_NotDequeue_SetsDequeueToTrue()
        {
            // Arrange
            var target = new Bundle(
                new Uuid(Guid.NewGuid()),
                new MarketOperator(new GlobalLocationNumber("fake_value")),
                DomainOrigin.TimeSeries,
                new ContentType("fake_value"),
                Array.Empty<Uuid>(),
                new Mock<IBundleContent>().Object,
                Enumerable.Empty<string>(),
                BundleReturnType.Xml);

            // Act
            target.Dequeue();

            // Assert
            Assert.True(target.Dequeued);
        }

        [Fact]
        public void TryGetContent_HasContent_ReturnsTrue()
        {
            // Arrange
            var target = new Bundle(
                new Uuid(Guid.NewGuid()),
                new MarketOperator(new GlobalLocationNumber("fake_value")),
                DomainOrigin.TimeSeries,
                new ContentType("fake_value"),
                Array.Empty<Uuid>(),
                new Mock<IBundleContent>().Object,
                Enumerable.Empty<string>(),
                BundleReturnType.Xml);

            // Act
            var actual = target.TryGetContent(out var actualContent);

            // Assert
            Assert.True(actual);
            Assert.NotNull(actualContent);
        }

        [Fact]
        public void TryGetContent_NoContent_ReturnsFalse()
        {
            // Arrange
            var target = new Bundle(
                new Uuid(Guid.NewGuid()),
                new MarketOperator(new GlobalLocationNumber("fake_value")),
                DomainOrigin.TimeSeries,
                new ContentType("fake_value"),
                Array.Empty<Uuid>(),
                Enumerable.Empty<string>(),
                BundleReturnType.Xml);

            // Act
            var actual = target.TryGetContent(out var actualContent);

            // Assert
            Assert.False(actual);
            Assert.Null(actualContent);
        }

        [Fact]
        public void AssignContent_NoPreviousContent_AssignsContent()
        {
            // Arrange
            var bundleContentMock = new Mock<IBundleContent>();
            var target = new Bundle(
                new Uuid(Guid.NewGuid()),
                new MarketOperator(new GlobalLocationNumber("fake_value")),
                DomainOrigin.TimeSeries,
                new ContentType("fake_value"),
                Array.Empty<Uuid>(),
                Enumerable.Empty<string>(),
                BundleReturnType.Xml);

            // Act
            target.AssignContent(bundleContentMock.Object);

            // Assert
            Assert.True(target.TryGetContent(out var actualContent));
            Assert.Equal(bundleContentMock.Object, actualContent);
        }

        [Fact]
        public void AssignContent_HasPreviousContent_ThrowsException()
        {
            // Arrange
            var bundleContentMock = new Mock<IBundleContent>();
            var target = new Bundle(
                new Uuid(Guid.NewGuid()),
                new MarketOperator(new GlobalLocationNumber("fake_value")),
                DomainOrigin.TimeSeries,
                new ContentType("fake_value"),
                Array.Empty<Uuid>(),
                bundleContentMock.Object,
                Enumerable.Empty<string>(),
                BundleReturnType.Xml);

            // Act + Assert
            Assert.Throws<InvalidOperationException>(() => target.AssignContent(bundleContentMock.Object));
        }
    }
}
