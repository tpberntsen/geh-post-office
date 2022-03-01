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
using Energinet.DataHub.PostOffice.EntryPoint.MarketOperator;
using FluentAssertions;
using Microsoft.Azure.Functions.Isolated.TestDoubles;
using Xunit;
using Xunit.Categories;

namespace Energinet.DataHub.PostOffice.Tests.Hosts.MarketOperator
{
    [UnitTest]
    public sealed class ExternalBundleIdProviderTests
    {
        [Fact]
        public void Given_Request_When_BundleIdIsPresent_Then_ItIsReturned()
        {
            // Arrange
            var expectedBundleId = Guid.NewGuid().ToString("N");
            Uri uri = new($"https://localhost/?{Constants.BundleIdQueryName}={expectedBundleId}");
            var request = MockHelpers.CreateHttpRequestData(url: uri);

            // Act
            var sut = new ExternalBundleIdProvider();
            var actualId = sut.TryGetBundleId(request);

            // Assert
            actualId.Should().Be(expectedBundleId);
        }

        [Fact]
        public void Given_Request_When_BundleIdIsNotInQuery_Then_NullIsReturned()
        {
            Uri uri = new($"https://localhost/");
            var request = MockHelpers.CreateHttpRequestData(url: uri);

            var sut = new ExternalBundleIdProvider();
            var actualId = sut.TryGetBundleId(request);

            actualId.Should().BeNull();
        }
    }
}
