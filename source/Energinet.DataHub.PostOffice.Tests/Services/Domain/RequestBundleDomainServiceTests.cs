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
using System.Threading.Tasks;
using Energinet.DataHub.PostOffice.Domain.Model;
using Energinet.DataHub.PostOffice.Domain.Services;
using Moq;
using Xunit;
using Xunit.Categories;

namespace Energinet.DataHub.PostOffice.Tests.Services.Domain
{
    [UnitTest]
    public sealed class RequestBundleDomainServiceTests
    {
        [Fact]
        public async Task WaitForBundleContentFromSubDomainAsync_NoData_ReturnsNull()
        {
            // Arrange
            var bundleContentRequestServiceMock = new Mock<IBundleContentRequestService>();
            var target = new RequestBundleDomainService(bundleContentRequestServiceMock.Object);
            var bundle = new Bundle(
                new Uuid(Guid.NewGuid()),
                new MarketOperator(new GlobalLocationNumber("fake_value")),
                DomainOrigin.TimeSeries,
                new ContentType("fake_value"),
                Array.Empty<Uuid>(),
                Enumerable.Empty<string>());

            bundleContentRequestServiceMock
                .Setup(x => x.WaitForBundleContentFromSubDomainAsync(bundle))
                .ReturnsAsync((IBundleContent?)null);

            // Act
            var actual = await target.WaitForBundleContentFromSubDomainAsync(bundle).ConfigureAwait(false);

            // Assert
            Assert.Null(actual);
        }

        [Fact]
        public async Task WaitForBundleContentFromSubDomainAsync_WithData_ReturnsData()
        {
            // Arrange
            var bundleContentRequestServiceMock = new Mock<IBundleContentRequestService>();
            var bundleContentMock = new Mock<IBundleContent>();
            var target = new RequestBundleDomainService(bundleContentRequestServiceMock.Object);
            var bundle = new Bundle(
                new Uuid(Guid.NewGuid()),
                new MarketOperator(new GlobalLocationNumber("fake_value")),
                DomainOrigin.TimeSeries,
                new ContentType("fake_value"),
                Array.Empty<Uuid>(),
                Enumerable.Empty<string>());

            bundleContentRequestServiceMock
                .Setup(x => x.WaitForBundleContentFromSubDomainAsync(bundle))
                .ReturnsAsync(bundleContentMock.Object);

            // Act
            var actual = await target.WaitForBundleContentFromSubDomainAsync(bundle).ConfigureAwait(false);

            // Assert
            Assert.NotNull(actual);
            Assert.Equal(bundleContentMock.Object, actual);
        }
    }
}
