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
using Energinet.DataHub.PostOffice.Domain.Model;
using Xunit;
using Xunit.Categories;

namespace Energinet.DataHub.PostOffice.Tests.Model
{
    [UnitTest]
    public sealed class WeightTests
    {
        [Theory]
        [InlineData(0, 0, 0)]
        [InlineData(0, 1, 1)]
        [InlineData(1, 0, 1)]
        [InlineData(1, 1, 2)]
        [InlineData(5, 7, 12)]
        public void AddOperator_DoesAddition_ReturnsResult(int left, int right, int result)
        {
            // Arrange
            var leftWeight = new Weight(left);
            var rightWeight = new Weight(right);

            // Act
            var actual = leftWeight + rightWeight;

            // Assert
            Assert.Equal(new Weight(result), actual);
        }

        [Theory]
        [InlineData(0, 0, 0)]
        [InlineData(0, 1, -1)]
        [InlineData(1, 0, 1)]
        [InlineData(1, 1, 0)]
        [InlineData(7, 5, 2)]
        [InlineData(5, 7, -2)]
        public void MinusOperator_DoesSubtraction_ReturnsResult(int left, int right, int result)
        {
            // Arrange
            var leftWeight = new Weight(left);
            var rightWeight = new Weight(right);

            // Act
            var actual = leftWeight - rightWeight;

            // Assert
            Assert.Equal(new Weight(result), actual);
        }

        [Theory]
        [InlineData(0, 0, false)]
        [InlineData(0, 1, true)]
        [InlineData(1, 0, false)]
        [InlineData(7, 5, false)]
        [InlineData(5, 7, true)]
        public void LessThanOperator_DoesComparison_ReturnsResult(int left, int right, bool result)
        {
            // Arrange
            var leftWeight = new Weight(left);
            var rightWeight = new Weight(right);

            // Act
            var actual = leftWeight < rightWeight;

            // Assert
            Assert.Equal(result, actual);
        }

        [Theory]
        [InlineData(0, 0, true)]
        [InlineData(0, 1, true)]
        [InlineData(1, 0, false)]
        [InlineData(7, 5, false)]
        [InlineData(5, 7, true)]
        public void LessThanOrEqualOperator_DoesComparison_ReturnsResult(int left, int right, bool result)
        {
            // Arrange
            var leftWeight = new Weight(left);
            var rightWeight = new Weight(right);

            // Act
            var actual = leftWeight <= rightWeight;

            // Assert
            Assert.Equal(result, actual);
        }

        [Theory]
        [InlineData(0, 0, false)]
        [InlineData(0, 1, false)]
        [InlineData(1, 0, true)]
        [InlineData(7, 5, true)]
        [InlineData(5, 7, false)]
        public void GreaterThanOperator_DoesComparison_ReturnsResult(int left, int right, bool result)
        {
            // Arrange
            var leftWeight = new Weight(left);
            var rightWeight = new Weight(right);

            // Act
            var actual = leftWeight > rightWeight;

            // Assert
            Assert.Equal(result, actual);
        }

        [Theory]
        [InlineData(0, 0, true)]
        [InlineData(0, 1, false)]
        [InlineData(1, 0, true)]
        [InlineData(7, 5, true)]
        [InlineData(5, 7, false)]
        public void GreaterThanOrEqualOperator_DoesComparison_ReturnsResult(int left, int right, bool result)
        {
            // Arrange
            var leftWeight = new Weight(left);
            var rightWeight = new Weight(right);

            // Act
            var actual = leftWeight >= rightWeight;

            // Assert
            Assert.Equal(result, actual);
        }

        [Theory]
        [InlineData(0, 0, true)]
        [InlineData(1, 1, true)]
        [InlineData(0, 1, false)]
        [InlineData(1, 0, false)]
        [InlineData(7, 5, false)]
        [InlineData(5, 7, false)]
        public void EqualsOperator_DoesComparison_ReturnsResult(int left, int right, bool result)
        {
            // Arrange
            var leftWeight = new Weight(left);
            var rightWeight = new Weight(right);

            // Act
            var actual = leftWeight == rightWeight;

            // Assert
            Assert.Equal(result, actual);
        }

        [Theory]
        [InlineData(0, 0, false)]
        [InlineData(1, 1, false)]
        [InlineData(0, 1, true)]
        [InlineData(1, 0, true)]
        [InlineData(7, 5, true)]
        [InlineData(5, 7, true)]
        public void NotEqualsOperator_DoesComparison_ReturnsResult(int left, int right, bool result)
        {
            // Arrange
            var leftWeight = new Weight(left);
            var rightWeight = new Weight(right);

            // Act
            var actual = leftWeight != rightWeight;

            // Assert
            Assert.Equal(result, actual);
        }

        [Theory]
        [InlineData(0, 0, true)]
        [InlineData(1, 1, true)]
        [InlineData(0, 1, false)]
        [InlineData(1, 0, false)]
        [InlineData(7, 5, false)]
        [InlineData(5, 7, false)]
        public void Equals_DoesComparison_ReturnsResult(int left, int right, bool result)
        {
            // Arrange
            var leftWeight = new Weight(left);
            var rightWeight = new Weight(right);

            // Act
            var actualA = leftWeight.Equals(rightWeight);
            var actualB = leftWeight.Equals((object)rightWeight);

            // Assert
            Assert.Equal(result, actualA);
            Assert.Equal(result, actualB);
        }

        [Fact]
        public void Equals_DoesNullComparison_ReturnsFalse()
        {
            // Arrange
            var weight = new Weight(5);

            // Act
            var actual = weight.Equals(null);

            // Assert
            Assert.False(actual);
        }

        [Fact]
        public void GetHashCode_ComputesHash_ReturnsValue()
        {
            // Arrange
            var weight = new Weight(5);

            // Act
            var actual = new HashSet<Weight> { weight };

            // Assert
            Assert.Contains(weight, actual);
        }
    }
}
