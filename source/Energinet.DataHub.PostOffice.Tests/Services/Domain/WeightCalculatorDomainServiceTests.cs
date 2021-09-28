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
using Energinet.DataHub.PostOffice.Domain.Model;
using Energinet.DataHub.PostOffice.Domain.Services;
using Xunit;
using Xunit.Categories;

namespace Energinet.DataHub.PostOffice.Tests.Services.Domain
{
    [UnitTest]
    public class WeightCalculatorDomainServiceTests
    {
        [Theory]
        [InlineData(DomainOrigin.TimeSeries, 1)]
        [InlineData(DomainOrigin.Aggregations, 1)]
        [InlineData(DomainOrigin.MeteringPoints, 1)]
        [InlineData(DomainOrigin.MarketRoles, 1)]
        [InlineData(DomainOrigin.Charges, 1)]
        public void Map_ContentType_ReturnsWeight(DomainOrigin domainOrigin, int expectedWeight)
        {
            // arrange
            var target = new WeightCalculatorDomainService();

            // act
            var actual = target.CalculateMaxWeight(domainOrigin);

            // assert
            Assert.Equal(new Weight(expectedWeight), actual);
        }

        [Fact]
        public void Map_DefaultContentType_ThrowsException()
        {
            // arrange
            var target = new WeightCalculatorDomainService();

            // act, assert
            Assert.Throws<InvalidOperationException>(() => target.CalculateMaxWeight(default));
        }

        [Fact]
        public void Map_ContentTypeUndefined_ThrowsException()
        {
            // arrange
            var target = new WeightCalculatorDomainService();

            // act, assert
            Assert.Throws<ArgumentOutOfRangeException>(() => target.CalculateMaxWeight((DomainOrigin)(-1)));
        }
    }
}
