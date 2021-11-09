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
using System.Threading.Tasks;
using Energinet.DataHub.MessageHub.Core.Peek;
using Energinet.DataHub.MessageHub.Model.Model;
using Energinet.DataHub.PostOffice.Domain.Model;
using Energinet.DataHub.PostOffice.Domain.Services;
using Energinet.DataHub.PostOffice.Infrastructure.Model;
using Energinet.DataHub.PostOffice.Infrastructure.Services;
using Moq;
using Xunit;
using Xunit.Categories;
using DomainOrigin = Energinet.DataHub.PostOffice.Domain.Model.DomainOrigin;

namespace Energinet.DataHub.PostOffice.Tests.Services.Infrastructure
{
    [UnitTest]
    public sealed class BundleContentRequestServiceTests
    {
        [Fact]
        public async Task WaitForBundleContentFromSubDomainAsync_NoData_ReturnsNull()
        {
            // Arrange
            var marketOperatorDataStorageServiceMock = new Mock<IMarketOperatorDataStorageService>();
            var dataBundleRequestSenderMock = new Mock<IDataBundleRequestSender>();
            var target = new BundleContentRequestService(marketOperatorDataStorageServiceMock.Object, dataBundleRequestSenderMock.Object);

            var bundle = new Bundle(
                new Uuid(Guid.NewGuid()),
                new MarketOperator(new GlobalLocationNumber("fake_value")),
                DomainOrigin.TimeSeries,
                new ContentType("fake_value"),
                Array.Empty<Uuid>());

            dataBundleRequestSenderMock
                .Setup(x => x.SendAsync(It.IsAny<DataBundleRequestDto>(), MessageHub.Model.Model.DomainOrigin.TimeSeries))
                .ReturnsAsync((DataBundleResponseDto?)null);

            // Act
            var actual = await target.WaitForBundleContentFromSubDomainAsync(bundle).ConfigureAwait(false);

            // Assert
            Assert.Null(actual);
        }

        [Fact]
        public async Task WaitForBundleContentFromSubDomainAsync_WithData_ReturnsData()
        {
            // Arrange
            var marketOperatorDataStorageServiceMock = new Mock<IMarketOperatorDataStorageService>();
            var dataBundleRequestSenderMock = new Mock<IDataBundleRequestSender>();
            var target = new BundleContentRequestService(marketOperatorDataStorageServiceMock.Object, dataBundleRequestSenderMock.Object);

            var bundle = new Bundle(
                new Uuid(Guid.NewGuid()),
                new MarketOperator(new GlobalLocationNumber("fake_value")),
                DomainOrigin.TimeSeries,
                new ContentType("fake_value"),
                Array.Empty<Uuid>());

            var contentUri = new Uri("https://test.test.dk");
            var response = new DataBundleResponseDto(
                Guid.NewGuid(),
                string.Empty,
                contentUri,
                Array.Empty<Guid>());

            dataBundleRequestSenderMock
                .Setup(x => x.SendAsync(It.IsAny<DataBundleRequestDto>(), MessageHub.Model.Model.DomainOrigin.TimeSeries))
                .ReturnsAsync(response);

            // Act
            var actual = (AzureBlobBundleContent?)await target.WaitForBundleContentFromSubDomainAsync(bundle).ConfigureAwait(false);

            // Assert
            Assert.NotNull(actual);
            Assert.Equal(contentUri, actual!.ContentPath);
        }
    }
}
