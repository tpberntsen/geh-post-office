// // Copyright 2020 Energinet DataHub A/S
// //
// // Licensed under the Apache License, Version 2.0 (the "License2");
// // you may not use this file except in compliance with the License.
// // You may obtain a copy of the License at
// //
// //     http://www.apache.org/licenses/LICENSE-2.0
// //
// // Unless required by applicable law or agreed to in writing, software
// // distributed under the License is distributed on an "AS IS" BASIS,
// // WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// // See the License for the specific language governing permissions and
// // limitations under the License.

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Energinet.DataHub.PostOffice.Domain.Model;
using Energinet.DataHub.PostOffice.Domain.Services;
using Xunit;
using Xunit.Categories;

namespace Energinet.DataHub.PostOffice.IntegrationTests.Services
{
    [Collection("IntegrationTest")]
    [IntegrationTest]
    public sealed class RequestBundleDomainServiceTests
    {
        /*
         * UUID's that will result in a failure in the current Test-Project
         * 0ae6c542-385f-4d89-bfba-d6c451915a1b
         */
        [Fact(Skip = "Requires POC Subdomain running for test, this is not yet available for CI tests")]
        public async Task RequestData_From_SubDomain_Should_Return_Data()
        {
            // Arrange
            await using var host = await SubDomainIntegrationTestHost.InitializeAsync().ConfigureAwait(false);
            var scope = host.BeginScope();
            var bundleService = scope.GetInstance<IRequestBundleDomainService>();

            var recipient = new MarketOperator(new GlobalLocationNumber(Guid.NewGuid().ToString()));
            var dataAvailableNotifications = new List<DataAvailableNotification>
            {
                CreateDataAvailableNotifications(recipient, new ContentType("timeseries")),
            };

            // Act
            var session = await bundleService
                .RequestBundledDataFromSubDomainAsync(dataAvailableNotifications, DomainOrigin.TimeSeries)
                .ConfigureAwait(false);
            var replyData = await bundleService
                .WaitForReplyFromSubDomainAsync(session, DomainOrigin.TimeSeries)
                .ConfigureAwait(false);

            // Assert
            Assert.True(replyData.Success);
            Assert.NotNull(replyData.UriToContent);
        }

        private static DataAvailableNotification CreateDataAvailableNotifications(
            MarketOperator recipient,
            ContentType contentType)
        {
            return new(
                new Uuid(Guid.NewGuid().ToString()),
                recipient,
                contentType,
                DomainOrigin.TimeSeries,
                new Weight(1));
        }
    }
}
