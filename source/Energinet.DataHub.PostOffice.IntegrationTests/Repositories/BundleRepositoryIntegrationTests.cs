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
using Energinet.DataHub.PostOffice.Infrastructure.Model;
using Energinet.DataHub.PostOffice.Infrastructure.Repositories;
using Energinet.DataHub.PostOffice.Infrastructure.Repositories.Containers;
using Xunit;
using Xunit.Categories;

namespace Energinet.DataHub.PostOffice.IntegrationTests.Repositories
{
    [Collection("IntegrationTest")]
    [IntegrationTest]
    public sealed class BundleRepositoryIntegrationTests
    {
        private static readonly Uri _contentPathUri = new("https://test.test.dk");

        [Fact]
        public async Task GetNextUnacknowledgedAsync_NoBundle_ReturnsNull()
        {
            // Arrange
            await using var host = await MarketOperatorIntegrationTestHost.InitializeAsync().ConfigureAwait(false);
            var scope = host.BeginScope();

            var container = scope.GetInstance<IBundleRepositoryContainer>();
            var storageService = scope.GetInstance<IMarketOperatorDataStorageService>();
            var target = new BundleRepository(container, storageService);

            var recipient = new MarketOperator(new GlobalLocationNumber(Guid.NewGuid().ToString()));

            // Act
            var bundle = await target.GetNextUnacknowledgedAsync(recipient).ConfigureAwait(false);

            // Assert
            Assert.Null(bundle);
        }

        [Fact]
        public async Task GetNextUnacknowledgedAsync_HasBundle_ReturnsBundle()
        {
            // Arrange
            await using var host = await MarketOperatorIntegrationTestHost.InitializeAsync().ConfigureAwait(false);
            var scope = host.BeginScope();

            var container = scope.GetInstance<IBundleRepositoryContainer>();
            var storageService = scope.GetInstance<IMarketOperatorDataStorageService>();
            var target = new BundleRepository(container, storageService);

            var recipient = new MarketOperator(new GlobalLocationNumber(Guid.NewGuid().ToString()));
            var setupBundle = new Bundle(
                new Uuid(Guid.NewGuid()),
                DomainOrigin.TimeSeries,
                recipient,
                new[] { new Uuid(Guid.NewGuid()) });

            await target.TryAddNextUnacknowledgedAsync(setupBundle).ConfigureAwait(false);

            // Act
            var bundle = await target.GetNextUnacknowledgedAsync(recipient).ConfigureAwait(false);

            // Assert
            Assert.NotNull(bundle);
            Assert.Equal(setupBundle.BundleId, bundle!.BundleId);
            Assert.Equal(setupBundle.Origin, bundle.Origin);
            Assert.Equal(setupBundle.Recipient, bundle.Recipient);
            Assert.Equal(setupBundle.NotificationIds.Single(), bundle.NotificationIds.Single());
            Assert.False(bundle.TryGetContent(out _));
        }

        [Fact]
        public async Task GetNextUnacknowledgedAsync_HasBundleAndContent_ReturnsBundleContent()
        {
            // Arrange
            await using var host = await MarketOperatorIntegrationTestHost.InitializeAsync().ConfigureAwait(false);
            var scope = host.BeginScope();

            var container = scope.GetInstance<IBundleRepositoryContainer>();
            var storageService = scope.GetInstance<IMarketOperatorDataStorageService>();
            var target = new BundleRepository(container, storageService);

            var recipient = new MarketOperator(new GlobalLocationNumber(Guid.NewGuid().ToString()));
            var setupBundle = CreateBundle(
                recipient,
                new AzureBlobBundleContent(storageService, _contentPathUri));

            await target.TryAddNextUnacknowledgedAsync(setupBundle).ConfigureAwait(false);

            // Act
            var bundle = await target.GetNextUnacknowledgedAsync(recipient).ConfigureAwait(false);

            // Assert
            Assert.NotNull(bundle);
            Assert.Equal(setupBundle.BundleId, bundle!.BundleId);
            Assert.Equal(setupBundle.Origin, bundle.Origin);
            Assert.Equal(setupBundle.Recipient, bundle.Recipient);
            Assert.Equal(setupBundle.NotificationIds.Single(), bundle.NotificationIds.Single());
            Assert.True(bundle.TryGetContent(out var actualBundleContent));
            Assert.Equal(_contentPathUri, ((AzureBlobBundleContent)actualBundleContent!).ContentPath);
        }

        [Fact]
        public async Task GetNextUnacknowledgedForDomainAsync_NoBundle_ReturnsNull()
        {
            // Arrange
            await using var host = await MarketOperatorIntegrationTestHost.InitializeAsync().ConfigureAwait(false);
            var scope = host.BeginScope();

            var container = scope.GetInstance<IBundleRepositoryContainer>();
            var storageService = scope.GetInstance<IMarketOperatorDataStorageService>();
            var target = new BundleRepository(container, storageService);

            var recipient = new MarketOperator(new GlobalLocationNumber(Guid.NewGuid().ToString()));

            // Act
            var bundle = await target.GetNextUnacknowledgedForDomainAsync(recipient, DomainOrigin.TimeSeries).ConfigureAwait(false);

            // Assert
            Assert.Null(bundle);
        }

        [Fact]
        public async Task GetNextUnacknowledgedForDomainAsync_HasBundle_ReturnsBundle()
        {
            // Arrange
            await using var host = await MarketOperatorIntegrationTestHost.InitializeAsync().ConfigureAwait(false);
            var scope = host.BeginScope();

            var container = scope.GetInstance<IBundleRepositoryContainer>();
            var storageService = scope.GetInstance<IMarketOperatorDataStorageService>();
            var target = new BundleRepository(container, storageService);

            var recipient = new MarketOperator(new GlobalLocationNumber(Guid.NewGuid().ToString()));
            var setupBundle = new Bundle(
                new Uuid(Guid.NewGuid()),
                DomainOrigin.TimeSeries,
                recipient,
                new[] { new Uuid(Guid.NewGuid()) });

            await target.TryAddNextUnacknowledgedAsync(setupBundle).ConfigureAwait(false);

            // Act
            var bundle = await target.GetNextUnacknowledgedForDomainAsync(recipient, DomainOrigin.TimeSeries).ConfigureAwait(false);

            // Assert
            Assert.NotNull(bundle);
            Assert.Equal(setupBundle.BundleId, bundle!.BundleId);
            Assert.Equal(setupBundle.Origin, bundle.Origin);
            Assert.Equal(setupBundle.Recipient, bundle.Recipient);
            Assert.Equal(setupBundle.NotificationIds.Single(), bundle.NotificationIds.Single());
            Assert.False(bundle.TryGetContent(out _));
        }

        [Fact]
        public async Task GetNextUnacknowledgedForDomainAsync_HasBundleAndContent_ReturnsBundleContent()
        {
            // Arrange
            await using var host = await MarketOperatorIntegrationTestHost.InitializeAsync().ConfigureAwait(false);
            var scope = host.BeginScope();

            var container = scope.GetInstance<IBundleRepositoryContainer>();
            var storageService = scope.GetInstance<IMarketOperatorDataStorageService>();
            var target = new BundleRepository(container, storageService);

            var recipient = new MarketOperator(new GlobalLocationNumber(Guid.NewGuid().ToString()));
            var setupBundle = CreateBundle(
                recipient,
                new AzureBlobBundleContent(storageService, _contentPathUri));

            await target.TryAddNextUnacknowledgedAsync(setupBundle).ConfigureAwait(false);

            // Act
            var bundle = await target.GetNextUnacknowledgedForDomainAsync(recipient, DomainOrigin.TimeSeries).ConfigureAwait(false);

            // Assert
            Assert.NotNull(bundle);
            Assert.Equal(setupBundle.BundleId, bundle!.BundleId);
            Assert.Equal(setupBundle.Origin, bundle.Origin);
            Assert.Equal(setupBundle.Recipient, bundle.Recipient);
            Assert.Equal(setupBundle.NotificationIds.Single(), bundle.NotificationIds.Single());
            Assert.True(bundle.TryGetContent(out var actualBundleContent));
            Assert.Equal(_contentPathUri, ((AzureBlobBundleContent)actualBundleContent!).ContentPath);
        }

        [Fact]
        public async Task AcknowledgeAsync_BundleAcknowledged_ReturnsNoNextBundle()
        {
            // Arrange
            await using var host = await MarketOperatorIntegrationTestHost.InitializeAsync().ConfigureAwait(false);
            var scope = host.BeginScope();

            var container = scope.GetInstance<IBundleRepositoryContainer>();
            var storageService = scope.GetInstance<IMarketOperatorDataStorageService>();
            var target = new BundleRepository(container, storageService);

            var recipient = new MarketOperator(new GlobalLocationNumber(Guid.NewGuid().ToString()));
            var setupBundle = CreateBundle(recipient);

            var beforeAdd = await target.GetNextUnacknowledgedAsync(recipient).ConfigureAwait(false);
            await target.TryAddNextUnacknowledgedAsync(setupBundle).ConfigureAwait(false);
            var afterAdd = await target.GetNextUnacknowledgedAsync(recipient).ConfigureAwait(false);

            // Act
            await target.AcknowledgeAsync(setupBundle.BundleId).ConfigureAwait(false);

            // Assert
            Assert.Null(beforeAdd);
            Assert.NotNull(afterAdd);
            Assert.Null(await target.GetNextUnacknowledgedAsync(recipient).ConfigureAwait(false));
        }

        [Fact]
        public async Task TryAddNextUnacknowledgedAsync_NoExistingBundle_ReturnsTrue()
        {
            // Arrange
            await using var host = await MarketOperatorIntegrationTestHost.InitializeAsync().ConfigureAwait(false);
            var scope = host.BeginScope();

            var container = scope.GetInstance<IBundleRepositoryContainer>();
            var storageService = scope.GetInstance<IMarketOperatorDataStorageService>();
            var target = new BundleRepository(container, storageService);

            var recipient = new MarketOperator(new GlobalLocationNumber(Guid.NewGuid().ToString()));
            var setupBundle = CreateBundle(recipient);

            // Act
            var couldAdd = await target.TryAddNextUnacknowledgedAsync(setupBundle).ConfigureAwait(false);

            // Assert
            Assert.Equal(BundleCreatedResponse.Success, couldAdd);
            Assert.NotNull(await target.GetNextUnacknowledgedAsync(recipient).ConfigureAwait(false));
        }

        [Fact]
        public async Task TryAddNextUnacknowledgedAsync_HasExistingBundle_ReturnsError()
        {
            // Arrange
            await using var host = await MarketOperatorIntegrationTestHost.InitializeAsync().ConfigureAwait(false);
            var scope = host.BeginScope();

            var container = scope.GetInstance<IBundleRepositoryContainer>();
            var storageService = scope.GetInstance<IMarketOperatorDataStorageService>();
            var target = new BundleRepository(container, storageService);

            var recipient = new MarketOperator(new GlobalLocationNumber(Guid.NewGuid().ToString()));
            var setupBundle = CreateBundle(recipient);
            var existingBundle = CreateBundle(recipient);

            await target.TryAddNextUnacknowledgedAsync(existingBundle).ConfigureAwait(false);

            // Act
            var couldAdd = await target.TryAddNextUnacknowledgedAsync(setupBundle).ConfigureAwait(false);

            // Assert
            Assert.Equal(BundleCreatedResponse.AnotherBundleExists, couldAdd);
        }

        [Fact]
        public async Task TryAddNextUnacknowledgedAsync_BundleIdExistsInsidePartition_ReturnsDuplicateError()
        {
            // Arrange
            await using var host = await MarketOperatorIntegrationTestHost.InitializeAsync().ConfigureAwait(false);
            var scope = host.BeginScope();

            var container = scope.GetInstance<IBundleRepositoryContainer>();
            var storageService = scope.GetInstance<IMarketOperatorDataStorageService>();
            var target = new BundleRepository(container, storageService);

            var recipientGln = new GlobalLocationNumber(Guid.NewGuid().ToString());
            var bundleId = new Uuid(Guid.NewGuid());

            var recipient = new MarketOperator(recipientGln);
            var setupBundle = CreateBundle(bundleId.AsGuid(), recipient);

            var recipient2 = new MarketOperator(recipientGln);
            var bundleWithDuplicateId = CreateBundle(bundleId.AsGuid(), recipient2);

            await target.TryAddNextUnacknowledgedAsync(setupBundle).ConfigureAwait(false);

            // Act
            var couldAddBundleWithDuplicateId = await target.TryAddNextUnacknowledgedAsync(bundleWithDuplicateId).ConfigureAwait(false);

            // Assert
            Assert.Equal(BundleCreatedResponse.BundleIdAlreadyInUse, couldAddBundleWithDuplicateId);
        }

        [Fact]
        public async Task TryAddNextUnacknowledgedAsync_HasAggregationsBundle_ReturnsFalse()
        {
            // Arrange
            await using var host = await MarketOperatorIntegrationTestHost.InitializeAsync().ConfigureAwait(false);
            var scope = host.BeginScope();

            var container = scope.GetInstance<IBundleRepositoryContainer>();
            var storageService = scope.GetInstance<IMarketOperatorDataStorageService>();
            var target = new BundleRepository(container, storageService);

            var recipient = new MarketOperator(new GlobalLocationNumber(Guid.NewGuid().ToString()));
            var setupBundle = CreateBundle(recipient);
            var conflictBundle = CreateBundle(recipient, domainOrigin: DomainOrigin.Aggregations);

            await target.TryAddNextUnacknowledgedAsync(conflictBundle).ConfigureAwait(false);

            // Act
            var couldAdd = await target.TryAddNextUnacknowledgedAsync(setupBundle).ConfigureAwait(false);

            // Assert
            Assert.Equal(BundleCreatedResponse.AnotherBundleExists, couldAdd);
        }

        [Fact]
        public async Task TryAddNextUnacknowledgedAsync_HasTimeSeriesBundle_ReturnsFalse()
        {
            // Arrange
            await using var host = await MarketOperatorIntegrationTestHost.InitializeAsync().ConfigureAwait(false);
            var scope = host.BeginScope();

            var container = scope.GetInstance<IBundleRepositoryContainer>();
            var storageService = scope.GetInstance<IMarketOperatorDataStorageService>();
            var target = new BundleRepository(container, storageService);

            var recipient = new MarketOperator(new GlobalLocationNumber(Guid.NewGuid().ToString()));
            var setupBundle = CreateBundle(recipient, domainOrigin: DomainOrigin.Aggregations);
            var conflictBundle = CreateBundle(recipient);

            await target.TryAddNextUnacknowledgedAsync(conflictBundle).ConfigureAwait(false);

            // Act
            var couldAdd = await target.TryAddNextUnacknowledgedAsync(setupBundle).ConfigureAwait(false);

            // Assert
            Assert.Equal(BundleCreatedResponse.AnotherBundleExists, couldAdd);
        }

        [Fact]
        public async Task SaveAsync_WithContentPath_ReturnsBundleContent()
        {
            // Arrange
            await using var host = await MarketOperatorIntegrationTestHost.InitializeAsync().ConfigureAwait(false);
            var scope = host.BeginScope();

            var container = scope.GetInstance<IBundleRepositoryContainer>();
            var storageService = scope.GetInstance<IMarketOperatorDataStorageService>();
            var target = new BundleRepository(container, storageService);

            var recipient = new MarketOperator(new GlobalLocationNumber(Guid.NewGuid().ToString()));
            var bundleContent = new AzureBlobBundleContent(storageService, _contentPathUri);

            await target.TryAddNextUnacknowledgedAsync(CreateBundle(recipient)).ConfigureAwait(false);
            var modifiedBundle = await target.GetNextUnacknowledgedAsync(recipient).ConfigureAwait(false);
            modifiedBundle!.AssignContent(bundleContent);

            // Act
            await target.SaveAsync(modifiedBundle).ConfigureAwait(false);

            // Assert
            var actualBundle = await target.GetNextUnacknowledgedAsync(recipient).ConfigureAwait(false);
            Assert.NotNull(actualBundle);
            Assert.True(actualBundle!.TryGetContent(out var actualBundleContent));
            Assert.Equal(_contentPathUri, ((AzureBlobBundleContent)actualBundleContent!).ContentPath);
        }

        private static Bundle CreateBundle(
            MarketOperator recipient,
            IBundleContent? bundleContent = null,
            DomainOrigin domainOrigin = DomainOrigin.TimeSeries)
        {
            return new Bundle(
                new Uuid(Guid.NewGuid()),
                domainOrigin,
                recipient,
                new[] { new Uuid(Guid.NewGuid()) },
                bundleContent);
        }

        private static Bundle CreateBundle(Guid bundleId, MarketOperator recipient, IBundleContent? bundleContent = null)
        {
            return new Bundle(
                new Uuid(bundleId),
                DomainOrigin.TimeSeries,
                recipient,
                new[] { new Uuid(Guid.NewGuid()) },
                bundleContent);
        }
    }
}
