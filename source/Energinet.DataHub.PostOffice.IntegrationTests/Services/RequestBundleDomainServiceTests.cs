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
        [Fact]
        public async Task RequestData_From_SubDomain_Should_Return_Data()
        {
            await using var host = await InboundIntegrationTestHost.InitializeAsync().ConfigureAwait(false);
            var scope = host.BeginScope();
            var bundleService = scope.GetInstance<IRequestBundleDomainService>();

            var recipient = new MarketOperator(System.Guid.NewGuid().ToString());
            var messageType = new ContentType(1, "fake_value");
            var dataAvailableNotifications = new List<DataAvailableNotification>()
            {
                CreateDataAvailableNotifications(recipient, messageType),
            };

            var session = await bundleService
                .RequestBundledDataFromSubDomainAsync(dataAvailableNotifications, SubDomain.TimeSeries)
                .ConfigureAwait(false);
            var replyData = await bundleService
                .WaitForReplyFromSubDomainAsync(session, SubDomain.TimeSeries)
                .ConfigureAwait(false);

            Assert.True(replyData.Success);
            Assert.NotNull(replyData.UriToContent);
        }

        private static DataAvailableNotification CreateDataAvailableNotifications(
            MarketOperator recipient,
            ContentType contentType)
        {
            return new DataAvailableNotification(
                new Uuid(System.Guid.NewGuid().ToString()),
                recipient,
                contentType,
                SubDomain.TimeSeries,
                new Weight(1));
        }
    }
}
