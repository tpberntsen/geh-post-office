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
using Energinet.DataHub.PostOffice.Common.Auth;
using Xunit;
using Xunit.Categories;

namespace Energinet.DataHub.PostOffice.Tests.Common.Auth
{
    [UnitTest]
    public sealed class MarketOperatorIdentityTests
    {
        [Fact]
        public void HasIdentity_WhenNotAssigned_ReturnsFalse()
        {
            // Arrange
            var target = new MarketOperatorIdentity();

            // Act
            var actual = target.HasIdentity;

            // Assert
            Assert.False(actual);
        }

        [Fact]
        public void HasIdentity_WhenAssigned_ReturnsTrue()
        {
            // Arrange
            var target = new MarketOperatorIdentity();
            ((IMarketOperatorIdentity)target).AssignGln("ABC");

            // Act
            var actual = target.HasIdentity;

            // Assert
            Assert.True(actual);
        }

        [Theory]
        [InlineData("")]
        [InlineData(" ")]
        [InlineData(null)]
        public void AssignGln_InvalidValue_ThrowsException(string value)
        {
            // Arrange
            var target = new MarketOperatorIdentity();

            // Act + Assert
            Assert.Throws<ArgumentException>(() => ((IMarketOperatorIdentity)target).AssignGln(value));
        }

        [Fact]
        public void AssignGln_AlreadyAssigned_ThrowsException()
        {
            // Arrange
            var target = new MarketOperatorIdentity();
            ((IMarketOperatorIdentity)target).AssignGln("other");

            // Act + Assert
            Assert.Throws<InvalidOperationException>(() => ((IMarketOperatorIdentity)target).AssignGln("value"));
        }

        [Fact]
        public void AssignGln_FirstAssignment_AssignsValue()
        {
            // Arrange
            var target = new MarketOperatorIdentity();

            // Act
            ((IMarketOperatorIdentity)target).AssignGln("123456");

            // Assert
            Assert.True(target.HasIdentity);
            Assert.Equal("123456", target.Gln);
        }

        [Fact]
        public void Gln_NotAssigned_ThrowsException()
        {
            // Arrange
            var target = new MarketOperatorIdentity();

            // Act + Assert
            Assert.Throws<InvalidOperationException>(() => target.Gln);
        }
    }
}
