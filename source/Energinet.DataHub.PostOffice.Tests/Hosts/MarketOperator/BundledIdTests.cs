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
using System.Linq;
using System.Threading.Tasks;
using Energinet.DataHub.PostOffice.Application.Commands;
using Energinet.DataHub.PostOffice.EntryPoint.MarketOperator;
using Energinet.DataHub.PostOffice.EntryPoint.MarketOperator.Functions;
using Energinet.DataHub.PostOffice.Tests.Common.Auth;
using Energinet.DataHub.PostOffice.Utilities;
using FluentAssertions;
using MediatR;
using Microsoft.Azure.Functions.Isolated.TestDoubles;
using Moq;
using Xunit;

namespace Energinet.DataHub.PostOffice.Tests.Hosts.MarketOperator
{
    public class BundledIdTests
    {
        [Fact]
        public async Task Given_PeekAggregations_WhenBundleIdIsPresentInQuery_ShouldSetBundledIdHeader()
        {
            // Arrange
            var bundleId = Guid.NewGuid().ToString("N");
            Uri path = new($"https://localhost?{Constants.BundleIdQueryName}={bundleId}");

            var request = MockHelpers.CreateHttpRequestData(url: path);
            var mediator = new Mock<IMediator>();
            mediator.Setup(p => p.Send(It.IsAny<PeekAggregationsCommand>(), default)).ReturnsAsync(new PeekResponse(false, bundleId, Stream.Null, Enumerable.Empty<string>()));
            var identifier = new MockedMarketOperatorIdentity("fake_value");

            // Act
            var sut = new PeekAggregationsFunction(
                mediator.Object,
                identifier,
                new Mock<IFeatureFlags>().Object,
                new ExternalBundleIdProvider(),
                new BundleReturnTypeProvider());

            var response = await sut.RunAsync(request).ConfigureAwait(false);

            // Assert
            response.Headers.Should()
                .ContainSingle(header =>
                    header.Key.Equals(Constants.BundleIdHeaderName, StringComparison.Ordinal) &&
                    header.Value.Single().Equals(bundleId, StringComparison.Ordinal));
        }

        [Fact]
        public async Task Given_Peek_WhenBundleIdIsPresentInQuery_ShouldSetBundleIdHeader()
        {
            // Arrange
            var bundleId = Guid.NewGuid().ToString("N");
            Uri path = new($"https://localhost?{Constants.BundleIdQueryName}={bundleId}");

            var request = MockHelpers.CreateHttpRequestData(url: path);
            var mediator = new Mock<IMediator>();
            mediator.Setup(p => p.Send(It.IsAny<PeekCommand>(), default)).ReturnsAsync(new PeekResponse(false, bundleId, Stream.Null, Enumerable.Empty<string>()));
            var identifier = new MockedMarketOperatorIdentity("fake_value");

            // Act
            var sut = new PeekFunction(
                mediator.Object,
                identifier,
                new Mock<IFeatureFlags>().Object,
                new ExternalBundleIdProvider(),
                new BundleReturnTypeProvider());

            var response = await sut.RunAsync(request).ConfigureAwait(false);

            // Assert
            response.Headers.Should()
                .ContainSingle(header =>
                    header.Key.Equals(Constants.BundleIdHeaderName, StringComparison.Ordinal) &&
                    header.Value.Single().Equals(bundleId, StringComparison.Ordinal));
        }

        [Fact]
        public async Task Given_PeekMasterData_WhenBundleIdIsPresentInQuery_ShouldSetBundleIdHeader()
        {
            // Arrange
            var bundleId = Guid.NewGuid().ToString("N");
            Uri path = new($"https://localhost?{Constants.BundleIdQueryName}={bundleId}");

            var request = MockHelpers.CreateHttpRequestData(url: path);
            var mediator = new Mock<IMediator>();
            mediator.Setup(p => p.Send(It.IsAny<PeekMasterDataCommand>(), default)).ReturnsAsync(new PeekResponse(false, bundleId, Stream.Null, Enumerable.Empty<string>()));
            var identifier = new MockedMarketOperatorIdentity("fake_value");

            // Act
            var sut = new PeekMasterDataFunction(
                mediator.Object,
                identifier,
                new Mock<IFeatureFlags>().Object,
                new ExternalBundleIdProvider(),
                new BundleReturnTypeProvider());

            var response = await sut.RunAsync(request).ConfigureAwait(false);

            // Assert
            response.Headers.Should()
                .ContainSingle(header =>
                    header.Key.Equals(Constants.BundleIdHeaderName, StringComparison.Ordinal) &&
                    header.Value.Single().Equals(bundleId, StringComparison.Ordinal));
        }

        [Fact]
        public async Task Given_PeekTimeSeries_WhenBundleIdIsPresentInQuery_ShouldSetBundleIdHeader()
        {
            // Arrange
            var bundleId = Guid.NewGuid().ToString("N");
            Uri path = new($"https://localhost?{Constants.BundleIdQueryName}={bundleId}");

            var request = MockHelpers.CreateHttpRequestData(url: path);
            var mediator = new Mock<IMediator>();
            mediator.Setup(p => p.Send(It.IsAny<PeekTimeSeriesCommand>(), default)).ReturnsAsync(new PeekResponse(false, bundleId, Stream.Null, Enumerable.Empty<string>()));
            var identifier = new MockedMarketOperatorIdentity("fake_value");

            // Act
            var sut = new PeekTimeSeriesFunction(
                mediator.Object,
                identifier,
                new Mock<IFeatureFlags>().Object,
                new ExternalBundleIdProvider(),
                new BundleReturnTypeProvider());
            var response = await sut.RunAsync(request).ConfigureAwait(false);

            // Assert
            response.Headers.Should()
                .ContainSingle(header =>
                    header.Key.Equals(Constants.BundleIdHeaderName, StringComparison.Ordinal) &&
                    header.Value.Single().Equals(bundleId, StringComparison.Ordinal));
        }
    }
}
