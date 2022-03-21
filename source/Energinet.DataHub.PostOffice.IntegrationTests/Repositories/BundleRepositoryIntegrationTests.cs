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
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Energinet.DataHub.MessageHub.Core.Storage;
using Energinet.DataHub.PostOffice.Domain.Model;
using Energinet.DataHub.PostOffice.Domain.Services;
using Energinet.DataHub.PostOffice.Infrastructure.Documents;
using Energinet.DataHub.PostOffice.Infrastructure.Model;
using Energinet.DataHub.PostOffice.Infrastructure.Repositories;
using Energinet.DataHub.PostOffice.Infrastructure.Repositories.Containers;
using Energinet.DataHub.PostOffice.IntegrationTests.Common;
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
            var storageHandler = scope.GetInstance<IStorageHandler>();
            var target = new BundleRepository(storageHandler, container, storageService);

            var recipient = new MarketOperator(new MockedGln());

            // Act
            var bundle = await target.GetNextUnacknowledgedAsync(recipient).ConfigureAwait(false);

            // Assert
            Assert.Null(bundle);
        }

        [Fact]
        public async Task GetNextUnacknowledgedAsync_HasUnrelatedBundle_ReturnsNull()
        {
            // Arrange
            await using var host = await MarketOperatorIntegrationTestHost.InitializeAsync().ConfigureAwait(false);
            var scope = host.BeginScope();

            var container = scope.GetInstance<IBundleRepositoryContainer>();
            var storageService = scope.GetInstance<IMarketOperatorDataStorageService>();
            var storageHandler = scope.GetInstance<IStorageHandler>();
            var target = new BundleRepository(storageHandler, container, storageService);

            var recipient = new MarketOperator(new MockedGln());
            var reader = CreateMockedReader();
            var setupBundle = new Bundle(
                new Uuid(Guid.NewGuid()),
                recipient,
                DomainOrigin.TimeSeries,
                new ContentType("fake_value"),
                new[] { new Uuid(Guid.NewGuid()) },
                Enumerable.Empty<string>(),
                BundleReturnType.Xml);

            await target.TryAddNextUnacknowledgedAsync(setupBundle, reader).ConfigureAwait(false);

            // Act
            var bundle = await target
                .GetNextUnacknowledgedAsync(recipient, DomainOrigin.MarketRoles)
                .ConfigureAwait(false);

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
            var storageHandler = scope.GetInstance<IStorageHandler>();
            var target = new BundleRepository(storageHandler, container, storageService);

            var recipient = new MarketOperator(new MockedGln());
            var reader = CreateMockedReader();
            var setupBundle = new Bundle(
                new Uuid(Guid.NewGuid()),
                recipient,
                DomainOrigin.TimeSeries,
                new ContentType("fake_value"),
                new[] { new Uuid(Guid.NewGuid()) },
                Enumerable.Empty<string>(),
                BundleReturnType.Xml);

            await target.TryAddNextUnacknowledgedAsync(setupBundle, reader).ConfigureAwait(false);

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
            var storageHandler = scope.GetInstance<IStorageHandler>();
            var target = new BundleRepository(storageHandler, container, storageService);

            var recipient = new MarketOperator(new MockedGln());
            var reader = CreateMockedReader();
            var setupBundle = CreateBundle(
                recipient,
                new AzureBlobBundleContent(storageService, _contentPathUri));

            await target.TryAddNextUnacknowledgedAsync(setupBundle, reader).ConfigureAwait(false);

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
        public async Task AcknowledgeAsync_BundleAcknowledged_ReturnsNoNextBundle()
        {
            // Arrange
            await using var host = await MarketOperatorIntegrationTestHost.InitializeAsync().ConfigureAwait(false);
            var scope = host.BeginScope();

            var container = scope.GetInstance<IBundleRepositoryContainer>();
            var storageService = scope.GetInstance<IMarketOperatorDataStorageService>();
            var storageHandler = scope.GetInstance<IStorageHandler>();
            var target = new BundleRepository(storageHandler, container, storageService);

            var recipient = new MarketOperator(new MockedGln());
            var setupBundle = CreateBundle(recipient);

            var beforeAdd = await target.GetNextUnacknowledgedAsync(recipient).ConfigureAwait(false);
            await target.TryAddNextUnacknowledgedAsync(setupBundle, CreateMockedReader()).ConfigureAwait(false);
            var afterAdd = await target.GetNextUnacknowledgedAsync(recipient).ConfigureAwait(false);

            // Act
            await target.AcknowledgeAsync(recipient, setupBundle.BundleId).ConfigureAwait(false);

            // Assert
            Assert.Null(beforeAdd);
            Assert.NotNull(afterAdd);
            Assert.Null(await target.GetNextUnacknowledgedAsync(recipient).ConfigureAwait(false));
        }

        [Fact]
        public async Task AcknowledgeAsync_AcrossPartitionKeys_OneRecipientCannotAffectAnother()
        {
            // Arrange
            await using var host = await MarketOperatorIntegrationTestHost.InitializeAsync().ConfigureAwait(false);
            var scope = host.BeginScope();

            var container = scope.GetInstance<IBundleRepositoryContainer>();
            var storageService = scope.GetInstance<IMarketOperatorDataStorageService>();
            var storageHandler = scope.GetInstance<IStorageHandler>();
            var target = new BundleRepository(storageHandler, container, storageService);

            var recipientA = new MarketOperator(new MockedGln());
            var recipientB = new MarketOperator(new MockedGln());
            var commonGuid = new Uuid(Guid.NewGuid());

            // Two identical bundles for two different recipients.
            // The uuid is only unique pr. partition and can be reused.
            var bundleA = new Bundle(
                commonGuid,
                recipientA,
                DomainOrigin.TimeSeries,
                new ContentType("fake_value"),
                new[] { commonGuid },
                Enumerable.Empty<string>(),
                BundleReturnType.Xml);

            // Everything should match to detect change of partition key.
            var bundleB = new Bundle(
                bundleA.BundleId,
                recipientB,
                bundleA.Origin,
                new ContentType("fake_value"),
                bundleA.NotificationIds,
                Enumerable.Empty<string>(),
                BundleReturnType.Xml);

            await target.TryAddNextUnacknowledgedAsync(bundleA, CreateMockedReader()).ConfigureAwait(false);
            await target.TryAddNextUnacknowledgedAsync(bundleB, CreateMockedReader()).ConfigureAwait(false);

            // Assert: Read bundles back.
            Assert.NotNull(await target.GetNextUnacknowledgedAsync(recipientA).ConfigureAwait(false));
            Assert.NotNull(await target.GetNextUnacknowledgedAsync(recipientB).ConfigureAwait(false));

            // Act
            await target.AcknowledgeAsync(recipientA, commonGuid).ConfigureAwait(false);

            // Assert: Only one bundle should be acknowledged.
            Assert.Null(await target.GetNextUnacknowledgedAsync(recipientA).ConfigureAwait(false));
            Assert.NotNull(await target.GetNextUnacknowledgedAsync(recipientB).ConfigureAwait(false));
        }

        [Fact]
        public async Task TryAddNextUnacknowledgedAsync_NoExistingBundle_ReturnsTrue()
        {
            // Arrange
            await using var host = await MarketOperatorIntegrationTestHost.InitializeAsync().ConfigureAwait(false);
            var scope = host.BeginScope();

            var container = scope.GetInstance<IBundleRepositoryContainer>();
            var storageService = scope.GetInstance<IMarketOperatorDataStorageService>();
            var storageHandler = scope.GetInstance<IStorageHandler>();
            var target = new BundleRepository(storageHandler, container, storageService);

            var recipient = new MarketOperator(new MockedGln());
            var setupBundle = CreateBundle(recipient);

            // Act
            var couldAdd = await target
                .TryAddNextUnacknowledgedAsync(setupBundle, CreateMockedReader())
                .ConfigureAwait(false);

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
            var storageHandler = scope.GetInstance<IStorageHandler>();
            var target = new BundleRepository(storageHandler, container, storageService);

            var recipient = new MarketOperator(new MockedGln());
            var setupBundle = CreateBundle(recipient);
            var existingBundle = CreateBundle(recipient);

            await target
                .TryAddNextUnacknowledgedAsync(existingBundle, CreateMockedReader())
                .ConfigureAwait(false);

            // Act
            var couldAdd = await target
                .TryAddNextUnacknowledgedAsync(setupBundle, CreateMockedReader())
                .ConfigureAwait(false);

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
            var storageHandler = scope.GetInstance<IStorageHandler>();
            var target = new BundleRepository(storageHandler, container, storageService);

            var recipientGln = new MockedGln();
            var bundleId = new Uuid(Guid.NewGuid());

            var recipient = new MarketOperator(recipientGln);
            var setupBundle = CreateBundle(bundleId.AsGuid(), recipient);

            var recipient2 = new MarketOperator(recipientGln);
            var bundleWithDuplicateId = CreateBundle(bundleId.AsGuid(), recipient2);

            await target
                .TryAddNextUnacknowledgedAsync(setupBundle, CreateMockedReader())
                .ConfigureAwait(false);

            // Act
            var couldAddBundleWithDuplicateId = await target
                .TryAddNextUnacknowledgedAsync(bundleWithDuplicateId, CreateMockedReader())
                .ConfigureAwait(false);

            // Assert
            Assert.Equal(BundleCreatedResponse.BundleIdAlreadyInUse, couldAddBundleWithDuplicateId);
        }

        [Theory]
        [InlineData(DomainOrigin.Aggregations, DomainOrigin.Aggregations)]
        [InlineData(DomainOrigin.TimeSeries, DomainOrigin.TimeSeries)]
        [InlineData(DomainOrigin.MarketRoles, DomainOrigin.MarketRoles)]
        [InlineData(DomainOrigin.MarketRoles, DomainOrigin.MeteringPoints)]
        [InlineData(DomainOrigin.MarketRoles, DomainOrigin.Charges)]
        [InlineData(DomainOrigin.Charges, DomainOrigin.Charges)]
        [InlineData(DomainOrigin.Charges, DomainOrigin.MarketRoles)]
        [InlineData(DomainOrigin.Charges, DomainOrigin.MeteringPoints)]
        [InlineData(DomainOrigin.MeteringPoints, DomainOrigin.MeteringPoints)]
        [InlineData(DomainOrigin.MeteringPoints, DomainOrigin.MarketRoles)]
        [InlineData(DomainOrigin.MeteringPoints, DomainOrigin.Charges)]
        public async Task TryAddNextUnacknowledgedAsync_HasConflictingDomains_ReturnsFalse(DomainOrigin initial, DomainOrigin conflicting)
        {
            // Arrange
            await using var host = await MarketOperatorIntegrationTestHost.InitializeAsync().ConfigureAwait(false);
            var scope = host.BeginScope();

            var container = scope.GetInstance<IBundleRepositoryContainer>();
            var storageService = scope.GetInstance<IMarketOperatorDataStorageService>();
            var storageHandler = scope.GetInstance<IStorageHandler>();
            var target = new BundleRepository(storageHandler, container, storageService);

            var recipient = new MarketOperator(new MockedGln());
            var setupBundle = CreateBundle(recipient, domainOrigin: initial);
            var conflictBundle = CreateBundle(recipient, domainOrigin: conflicting);

            await target
                .TryAddNextUnacknowledgedAsync(conflictBundle, CreateMockedReader())
                .ConfigureAwait(false);

            // Act
            var couldAdd = await target
                .TryAddNextUnacknowledgedAsync(setupBundle, CreateMockedReader())
                .ConfigureAwait(false);

            // Assert
            Assert.Equal(BundleCreatedResponse.AnotherBundleExists, couldAdd);
        }

        [Theory]
        [InlineData(DomainOrigin.Aggregations, DomainOrigin.TimeSeries)]
        [InlineData(DomainOrigin.Aggregations, DomainOrigin.MarketRoles)]
        [InlineData(DomainOrigin.Aggregations, DomainOrigin.MeteringPoints)]
        [InlineData(DomainOrigin.Aggregations, DomainOrigin.Charges)]
        [InlineData(DomainOrigin.TimeSeries, DomainOrigin.MarketRoles)]
        [InlineData(DomainOrigin.TimeSeries, DomainOrigin.MeteringPoints)]
        [InlineData(DomainOrigin.TimeSeries, DomainOrigin.Charges)]
        public async Task TryAddNextUnacknowledgedAsync_HasNotConflictingDomains_ReturnsTrue(DomainOrigin initial, DomainOrigin conflicting)
        {
            // Arrange
            await using var host = await MarketOperatorIntegrationTestHost.InitializeAsync().ConfigureAwait(false);
            var scope = host.BeginScope();

            var container = scope.GetInstance<IBundleRepositoryContainer>();
            var storageService = scope.GetInstance<IMarketOperatorDataStorageService>();
            var storageHandler = scope.GetInstance<IStorageHandler>();
            var target = new BundleRepository(storageHandler, container, storageService);

            var recipient = new MarketOperator(new MockedGln());
            var setupBundle = CreateBundle(recipient, domainOrigin: initial);
            var conflictBundle = CreateBundle(recipient, domainOrigin: conflicting);

            await target
                .TryAddNextUnacknowledgedAsync(conflictBundle, CreateMockedReader())
                .ConfigureAwait(false);

            // Act
            var couldAdd = await target
                .TryAddNextUnacknowledgedAsync(setupBundle, CreateMockedReader())
                .ConfigureAwait(false);

            // Assert
            Assert.Equal(BundleCreatedResponse.Success, couldAdd);
        }

        [Fact]
        public async Task SaveAsync_WithContentPath_ReturnsBundleContent()
        {
            // Arrange
            await using var host = await MarketOperatorIntegrationTestHost.InitializeAsync().ConfigureAwait(false);
            var scope = host.BeginScope();

            var container = scope.GetInstance<IBundleRepositoryContainer>();
            var storageService = scope.GetInstance<IMarketOperatorDataStorageService>();
            var storageHandler = scope.GetInstance<IStorageHandler>();
            var target = new BundleRepository(storageHandler, container, storageService);

            var recipient = new MarketOperator(new MockedGln());
            var bundleContent = new AzureBlobBundleContent(storageService, _contentPathUri);

            await target
                .TryAddNextUnacknowledgedAsync(CreateBundle(recipient), CreateMockedReader())
                .ConfigureAwait(false);

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
                recipient,
                domainOrigin,
                new ContentType("fake_value"),
                new[] { new Uuid(Guid.NewGuid()) },
                bundleContent,
                Enumerable.Empty<string>(),
                BundleReturnType.Xml);
        }

        private static Bundle CreateBundle(Guid bundleId, MarketOperator recipient, IBundleContent? bundleContent = null)
        {
            return new Bundle(
                new Uuid(bundleId),
                recipient,
                DomainOrigin.TimeSeries,
                new ContentType("fake_value"),
                new[] { new Uuid(Guid.NewGuid()) },
                bundleContent,
                Enumerable.Empty<string>(),
                BundleReturnType.Xml);
        }

        private static AsyncCabinetReader CreateMockedReader()
        {
            return new AsyncCabinetReader(
                null!,
                Array.Empty<CosmosCabinetDrawer>(),
                Array.Empty<Task<IEnumerable<CosmosDataAvailable>>>());
        }
    }
}
