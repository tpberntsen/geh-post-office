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
using Energinet.DataHub.PostOffice.Common.Auth;
using Energinet.DataHub.PostOffice.EntryPoint.MarketOperator.Functions;
using Energinet.DataHub.PostOffice.Tests.Common.Auth;
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
        public async Task Given_PeekAggregations_WhenBundledIdIsPresentInQuery_ShouldSetBundledIdHeader()
        {
            // Arrange
            var bundledId = Guid.NewGuid().ToString("N");
            var path = $"https://localhost?{PeekAggregationsFunction.BundleIdQueryName}={bundledId}";

            var request = MockHelpers.CreateHttpRequestData(url: path);

            var mediator = new Mock<IMediator>();
            mediator
                .Setup(p => p.Send(It.IsAny<PeekAggregationsCommand>(), default))
                .ReturnsAsync(new PeekResponse(false, Stream.Null));

            var identifier = new MockedMarketOperatorIdentity("fake_value");

            // Act
            var sut = new PeekAggregationsFunction(mediator.Object, identifier);
            var response = await sut.RunAsync(request).ConfigureAwait(false);

            // Assert
            response.Headers.Should()
                .ContainSingle(header =>
                    header.Key.Equals(PeekAggregationsFunction.BundleIdHeaderName, StringComparison.Ordinal) &&
                    header.Value.Equals(bundledId));
        }
    }
}
