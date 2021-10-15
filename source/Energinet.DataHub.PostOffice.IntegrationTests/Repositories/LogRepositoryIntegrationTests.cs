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
using System.Globalization;
using System.Threading.Tasks;
using Energinet.DataHub.PostOffice.Domain.Model;
using Energinet.DataHub.PostOffice.Domain.Model.Logging;
using Energinet.DataHub.PostOffice.Infrastructure.Model;
using Energinet.DataHub.PostOffice.Infrastructure.Repositories;
using Energinet.DataHub.PostOffice.Infrastructure.Repositories.Containers;
using Microsoft.Azure.Cosmos;
using Moq;
using Xunit;
using Xunit.Categories;

namespace Energinet.DataHub.PostOffice.IntegrationTests.Repositories
{
    [Collection("IntegrationTest")]
    [IntegrationTest]
    public class LogRepositoryIntegrationTests
    {
        [Fact]
        public async Task SaveLogOccurrenceAsync_PeekLogValidData_LogOccurrenceIsSavedCorrectlyToStorage()
        {
            // Arrange
            await using var host = await MarketOperatorIntegrationTestHost.InitializeAsync().ConfigureAwait(false);
            var scope = host.BeginScope();

            var container = scope.GetInstance<ILogRepositoryContainer>();

            var target = new LogRepository(container);

            var fakeIBundleContent = new Mock<IBundleContent>();
            fakeIBundleContent
                .Setup(e => e.LogIdentifier).Returns("https://127.0.0.1");

            var processId = GetFakeProcessId();

            var logObject = new PeekLog(
                processId,
                fakeIBundleContent.Object);

            // Act
            await target.SavePeekLogOccurrenceAsync(logObject).ConfigureAwait(true);

            var cosmosItem = await container.Container.ReadItemAsync<CosmosLog>(
                logObject.Id.ToString(),
                new PartitionKey(logObject.ProcessId.Recipient.Gln.Value))
                .ConfigureAwait(true);

            // Assert
            Assert.Equal(logObject.Id.ToString(), cosmosItem.Resource.Id);
            Assert.Equal(logObject.Timestamp.ToString(CultureInfo.InvariantCulture), cosmosItem.Resource.Timestamp.ToString(CultureInfo.InvariantCulture));
            Assert.Equal(logObject.EndpointType, cosmosItem.Resource.EndpointType);
            Assert.Equal(logObject.ProcessId.Recipient.Gln.Value, cosmosItem.Resource.MarketOperator);
            Assert.Equal(logObject.ProcessId.ToString(), cosmosItem.Resource.ProcessId);
        }

        [Fact]
        public async Task SaveLogOccurrenceAsync_PeekTimeSeriesLogValidData_LogOccurrenceIsSavedCorrectlyToStorage()
        {
            // Arrange
            await using var host = await MarketOperatorIntegrationTestHost.InitializeAsync().ConfigureAwait(false);
            var scope = host.BeginScope();

            var container = scope.GetInstance<ILogRepositoryContainer>();

            var target = new LogRepository(container);

            var fakeIBundleContent = new Mock<IBundleContent>();
            fakeIBundleContent
                .Setup(e => e.LogIdentifier).Returns("https://127.0.0.1");

            var processId = GetFakeProcessId();

            var logObject = new PeekTimeseriesLog(
                processId,
                fakeIBundleContent.Object);

            // Act
            await target.SavePeekLogOccurrenceAsync(logObject).ConfigureAwait(true);

            var cosmosItem = await container.Container.ReadItemAsync<CosmosLog>(
                    logObject.Id.ToString(),
                    new PartitionKey(logObject.ProcessId.Recipient.Gln.Value))
                .ConfigureAwait(true);

            // Assert
            Assert.Equal(logObject.Id.ToString(), cosmosItem.Resource.Id);
            Assert.Equal(logObject.Timestamp.ToString(CultureInfo.InvariantCulture), cosmosItem.Resource.Timestamp.ToString(CultureInfo.InvariantCulture));
            Assert.Equal(logObject.EndpointType, cosmosItem.Resource.EndpointType);
            Assert.Equal(logObject.ProcessId.Recipient.Gln.Value, cosmosItem.Resource.MarketOperator);
            Assert.Equal(logObject.ProcessId.ToString(), cosmosItem.Resource.ProcessId);
        }

        [Fact]
        public async Task SaveLogOccurrenceAsync_PeekMasterDataLogValidData_LogOccurrenceIsSavedCorrectlyToStorage()
        {
            // Arrange
            await using var host = await MarketOperatorIntegrationTestHost.InitializeAsync().ConfigureAwait(false);
            var scope = host.BeginScope();

            var container = scope.GetInstance<ILogRepositoryContainer>();

            var target = new LogRepository(container);

            var fakeIBundleContent = new Mock<IBundleContent>();
            fakeIBundleContent
                .Setup(e => e.LogIdentifier).Returns("https://127.0.0.1");

            var processId = GetFakeProcessId();

            var logObject = new PeekMasterDataLog(
                processId,
                fakeIBundleContent.Object);

            // Act
            await target.SavePeekLogOccurrenceAsync(logObject).ConfigureAwait(true);

            var cosmosItem = await container.Container.ReadItemAsync<CosmosLog>(
                    logObject.Id.ToString(),
                    new PartitionKey(logObject.ProcessId.Recipient.Gln.Value))
                .ConfigureAwait(true);

            // Assert
            Assert.Equal(logObject.Id.ToString(), cosmosItem.Resource.Id);
            Assert.Equal(logObject.Timestamp.ToString(CultureInfo.InvariantCulture), cosmosItem.Resource.Timestamp.ToString(CultureInfo.InvariantCulture));
            Assert.Equal(logObject.EndpointType, cosmosItem.Resource.EndpointType);
            Assert.Equal(logObject.ProcessId.Recipient.Gln.Value, cosmosItem.Resource.MarketOperator);
            Assert.Equal(logObject.ProcessId.ToString(), cosmosItem.Resource.ProcessId);
        }

        [Fact]
        public async Task SaveLogOccurrenceAsync_PeekAggregationsLogValidData_LogOccurrenceIsSavedCorrectlyToStorage()
        {
            // Arrange
            await using var host = await MarketOperatorIntegrationTestHost.InitializeAsync().ConfigureAwait(false);
            var scope = host.BeginScope();

            var container = scope.GetInstance<ILogRepositoryContainer>();

            var target = new LogRepository(container);

            var fakeIBundleContent = new Mock<IBundleContent>();
            fakeIBundleContent
                .Setup(e => e.LogIdentifier).Returns("https://127.0.0.1");

            var processId = GetFakeProcessId();

            var logObject = new PeekAggregationsLog(
                processId,
                fakeIBundleContent.Object);

            // Act
            await target.SavePeekLogOccurrenceAsync(logObject).ConfigureAwait(true);

            var cosmosItem = await container.Container.ReadItemAsync<CosmosLog>(
                    logObject.Id.ToString(),
                    new PartitionKey(logObject.ProcessId.Recipient.Gln.Value))
                .ConfigureAwait(true);

            // Assert
            Assert.Equal(logObject.Id.ToString(), cosmosItem.Resource.Id);
            Assert.Equal(logObject.Timestamp.ToString(CultureInfo.InvariantCulture), cosmosItem.Resource.Timestamp.ToString(CultureInfo.InvariantCulture));
            Assert.Equal(logObject.EndpointType, cosmosItem.Resource.EndpointType);
            Assert.Equal(logObject.ProcessId.Recipient.Gln.Value, cosmosItem.Resource.MarketOperator);
            Assert.Equal(logObject.ProcessId.ToString(), cosmosItem.Resource.ProcessId);
        }

        [Fact]
        public async Task SaveLogOccurrenceAsync_DequeueLogValidData_LogOccurrenceIsSavedCorrectlyToStorage()
        {
            // Arrange
            await using var host = await MarketOperatorIntegrationTestHost.InitializeAsync().ConfigureAwait(false);
            var scope = host.BeginScope();

            var container = scope.GetInstance<ILogRepositoryContainer>();

            var target = new LogRepository(container);

            var processId = GetFakeProcessId();

            var logObject = new DequeueLog(processId);

            // Act
            await target.SaveDequeueLogOccurrenceAsync(logObject).ConfigureAwait(false);

            var cosmosItem = await container.Container.ReadItemAsync<CosmosLog>(
                logObject.Id.ToString(),
                new PartitionKey(logObject.ProcessId.Recipient.Gln.Value)).ConfigureAwait(false);

            // Assert
            Assert.Equal(logObject.Id.ToString(), cosmosItem.Resource.Id);
            Assert.Equal(logObject.Timestamp.ToString(CultureInfo.InvariantCulture), cosmosItem.Resource.Timestamp.ToString(CultureInfo.InvariantCulture));
            Assert.Equal(logObject.EndpointType, cosmosItem.Resource.EndpointType);
            Assert.Equal(logObject.ProcessId.Recipient.Gln.Value, cosmosItem.Resource.MarketOperator);
            Assert.Equal(logObject.ProcessId.ToString(), cosmosItem.Resource.ProcessId);
        }

        private static ProcessId GetFakeProcessId()
        {
            return new(
                new Uuid(Guid.NewGuid()),
                new MarketOperator(new GlobalLocationNumber("fake_value")));
        }
    }
}
