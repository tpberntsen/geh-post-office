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
using Energinet.DataHub.PostOffice.Domain.Model;
using Energinet.DataHub.PostOffice.Domain.Repositories;
using Energinet.DataHub.PostOffice.Infrastructure.Documents;
using Energinet.DataHub.PostOffice.Infrastructure.Repositories.Containers;
using Energinet.DataHub.PostOffice.IntegrationTests.Common;
using Microsoft.Azure.Cosmos;
using Xunit;
using Xunit.Categories;

namespace Energinet.DataHub.PostOffice.IntegrationTests.Repositories
{
    [Collection("IntegrationTest")]
    [IntegrationTest]
    public sealed class DataAvailableNotificationRepositoryTests
    {
        [Fact]
        public async Task ReadCatalogForNextUnacknowledgedAsync_NoData_ReturnsNull()
        {
            // Arrange
            await using var host = await SubDomainIntegrationTestHost.InitializeAsync().ConfigureAwait(false);
            var scope = host.BeginScope();

            var dataAvailableNotificationRepository = scope.GetInstance<IDataAvailableNotificationRepository>();
            var recipient = new MarketOperator(new MockedGln());

            // Act
            var actual = await dataAvailableNotificationRepository
                .ReadCatalogForNextUnacknowledgedAsync(recipient, DomainOrigin.Aggregations)
                .ConfigureAwait(false);

            // Assert
            Assert.Null(actual);
        }

        [Fact]
        public async Task ReadCatalogForNextUnacknowledgedAsync_OneCatalog_ReturnsKey()
        {
            // Arrange
            await using var host = await SubDomainIntegrationTestHost.InitializeAsync().ConfigureAwait(false);
            var scope = host.BeginScope();

            var dataAvailableNotificationRepository = scope.GetInstance<IDataAvailableNotificationRepository>();
            var recipient = new MarketOperator(new MockedGln());
            var origin = DomainOrigin.Aggregations;

            var dataAvailableContainer = scope.GetInstance<IDataAvailableNotificationRepositoryContainer>();

            var cosmosCatalogEntry = new CosmosCatalogEntry { Id = Guid.NewGuid().ToString(), PartitionKey = string.Join('_', recipient.Gln.Value, origin), ContentType = "target_content_type", NextSequenceNumber = 1 };

            await dataAvailableContainer
                .Catalog
                .CreateItemAsync(cosmosCatalogEntry)
                .ConfigureAwait(false);

            // Act
            var actual = await dataAvailableNotificationRepository
                .ReadCatalogForNextUnacknowledgedAsync(recipient, origin)
                .ConfigureAwait(false);

            // Assert
            Assert.NotNull(actual);
            Assert.Equal(recipient, actual!.Recipient);
            Assert.Equal(origin, actual.Origin);
            Assert.Equal(cosmosCatalogEntry.ContentType, actual.ContentType.Value);
        }

        [Fact]
        public async Task ReadCatalogForNextUnacknowledgedAsync_OneCatalogNoDomains_ReturnsKey()
        {
            // Arrange
            await using var host = await SubDomainIntegrationTestHost.InitializeAsync().ConfigureAwait(false);
            var scope = host.BeginScope();

            var dataAvailableNotificationRepository = scope.GetInstance<IDataAvailableNotificationRepository>();
            var recipient = new MarketOperator(new MockedGln());
            var origin = DomainOrigin.Aggregations;

            var dataAvailableContainer = scope.GetInstance<IDataAvailableNotificationRepositoryContainer>();

            var cosmosCatalogEntry = new CosmosCatalogEntry { Id = Guid.NewGuid().ToString(), PartitionKey = string.Join('_', recipient.Gln.Value, origin), ContentType = "target_content_type", NextSequenceNumber = 1 };

            await dataAvailableContainer
                .Catalog
                .CreateItemAsync(cosmosCatalogEntry)
                .ConfigureAwait(false);

            // Act
            var actual = await dataAvailableNotificationRepository
                .ReadCatalogForNextUnacknowledgedAsync(recipient)
                .ConfigureAwait(false);

            // Assert
            Assert.NotNull(actual);
            Assert.Equal(recipient, actual!.Recipient);
            Assert.Equal(origin, actual.Origin);
            Assert.Equal(cosmosCatalogEntry.ContentType, actual.ContentType.Value);
        }

        [Fact]
        public async Task ReadCatalogForNextUnacknowledgedAsync_MultipleCatalogs_ReturnsSmallestKey()
        {
            // Arrange
            await using var host = await SubDomainIntegrationTestHost.InitializeAsync().ConfigureAwait(false);
            var scope = host.BeginScope();

            var dataAvailableNotificationRepository = scope.GetInstance<IDataAvailableNotificationRepository>();
            var recipient = new MarketOperator(new MockedGln());
            var origins = new[] { DomainOrigin.Aggregations, DomainOrigin.Charges, DomainOrigin.MarketRoles };
            var values = new[] { 12, 3, 4 };

            var dataAvailableContainer = scope.GetInstance<IDataAvailableNotificationRepositoryContainer>();

            for (var i = 0; i < origins.Length; i++)
            {
                var cosmosCatalogEntry = new CosmosCatalogEntry { Id = Guid.NewGuid().ToString(), PartitionKey = string.Join('_', recipient.Gln.Value, origins[i]), ContentType = $"content_{values[i]}", NextSequenceNumber = values[i] };

                await dataAvailableContainer
                    .Catalog
                    .CreateItemAsync(cosmosCatalogEntry)
                    .ConfigureAwait(false);

                var cosmosCatalogEntry2 = new CosmosCatalogEntry { Id = Guid.NewGuid().ToString(), PartitionKey = string.Join('_', recipient.Gln.Value, origins[i]), ContentType = $"content_{values[i]}_2", NextSequenceNumber = values[i] + 100 };

                await dataAvailableContainer
                    .Catalog
                    .CreateItemAsync(cosmosCatalogEntry2)
                    .ConfigureAwait(false);
            }

            // Act
            var actual = await dataAvailableNotificationRepository
                .ReadCatalogForNextUnacknowledgedAsync(recipient, origins)
                .ConfigureAwait(false);

            // Assert
            Assert.NotNull(actual);
            Assert.Equal(recipient, actual!.Recipient);
            Assert.Equal(origins[1], actual.Origin);
            Assert.Equal($"content_{values[1]}", actual.ContentType.Value);
        }

        [Fact]
        public async Task ReadCatalogForNextUnacknowledgedAsync_MultipleUnrelatedCatalogs_ReturnsSmallestKey()
        {
            // Arrange
            await using var host = await SubDomainIntegrationTestHost.InitializeAsync().ConfigureAwait(false);
            var scope = host.BeginScope();

            var dataAvailableNotificationRepository = scope.GetInstance<IDataAvailableNotificationRepository>();
            var recipient = new MarketOperator(new MockedGln());
            var origins = new[] { DomainOrigin.Aggregations, DomainOrigin.Charges, DomainOrigin.MarketRoles };
            var values = new[] { 11, 3, 4 };

            var dataAvailableContainer = scope.GetInstance<IDataAvailableNotificationRepositoryContainer>();

            for (var i = 0; i < origins.Length; i++)
            {
                var cosmosCatalogEntry = new CosmosCatalogEntry { Id = Guid.NewGuid().ToString(), PartitionKey = string.Join('_', recipient.Gln.Value, origins[i]), ContentType = $"content_{values[i]}", NextSequenceNumber = values[i] };

                await dataAvailableContainer
                    .Catalog
                    .CreateItemAsync(cosmosCatalogEntry)
                    .ConfigureAwait(false);

                var cosmosCatalogEntry2 = new CosmosCatalogEntry { Id = Guid.NewGuid().ToString(), PartitionKey = string.Join('_', recipient.Gln.Value, origins[i]), ContentType = $"content_{values[i]}_2", NextSequenceNumber = values[i] + 100 };

                await dataAvailableContainer
                    .Catalog
                    .CreateItemAsync(cosmosCatalogEntry2)
                    .ConfigureAwait(false);
            }

            // Act
            var actual = await dataAvailableNotificationRepository
                .ReadCatalogForNextUnacknowledgedAsync(recipient, new[] { DomainOrigin.Aggregations })
                .ConfigureAwait(false);

            // Assert
            Assert.NotNull(actual);
            Assert.Equal(recipient, actual!.Recipient);
            Assert.Equal(origins[0], actual.Origin);
            Assert.Equal($"content_{values[0]}", actual.ContentType.Value);
        }

        [Fact]
        public async Task GetCabinetReaderAsync_OneItem_ReturnsItem()
        {
            // Arrange
            await using var host = await SubDomainIntegrationTestHost.InitializeAsync().ConfigureAwait(false);
            var scope = host.BeginScope();

            var dataAvailableNotificationRepository = scope.GetInstance<IDataAvailableNotificationRepository>();
            var recipient = new MarketOperator(new MockedGln());
            var origin = DomainOrigin.Aggregations;
            var contentType = new ContentType("target_content");

            var dataAvailableContainer = scope.GetInstance<IDataAvailableNotificationRepositoryContainer>();
            var dataAvailableId = Guid.NewGuid();

            var cosmosCatalogDrawer = new CosmosCabinetDrawer { Id = Guid.NewGuid().ToString(), PartitionKey = string.Join('_', recipient.Gln.Value, origin, contentType.Value), Position = 0, OrderBy = 1 };

            var cosmosDataAvailable = new CosmosDataAvailable
            {
                Id = dataAvailableId.ToString(),
                Recipient = recipient.Gln.Value,
                Origin = origin.ToString(),
                ContentType = contentType.Value,
                PartitionKey = cosmosCatalogDrawer.Id,
                SequenceNumber = 1
            };

            await dataAvailableContainer
                .Cabinet
                .CreateItemAsync(cosmosDataAvailable)
                .ConfigureAwait(false);

            await dataAvailableContainer
                .Cabinet
                .CreateItemAsync(cosmosCatalogDrawer)
                .ConfigureAwait(false);

            var cabinetKey = new CabinetKey(recipient, origin, contentType);

            // Act
            var actual = await dataAvailableNotificationRepository
                .GetCabinetReaderAsync(cabinetKey)
                .ConfigureAwait(false);

            // Assert
            Assert.NotNull(actual);
            Assert.True(actual.CanPeek);

            var actualValue = actual.Peek();
            Assert.Equal(dataAvailableId, actualValue.NotificationId.AsGuid());
        }

        [Fact]
        public async Task GetCabinetReaderAsync_TwoItemsMovedPosition_ReturnsItem()
        {
            // Arrange
            await using var host = await SubDomainIntegrationTestHost.InitializeAsync().ConfigureAwait(false);
            var scope = host.BeginScope();

            var dataAvailableNotificationRepository = scope.GetInstance<IDataAvailableNotificationRepository>();
            var recipient = new MarketOperator(new MockedGln());
            var origin = DomainOrigin.Aggregations;
            var contentType = new ContentType("target_content");

            var dataAvailableContainer = scope.GetInstance<IDataAvailableNotificationRepositoryContainer>();
            var dataAvailableId = Guid.NewGuid();

            var cosmosCatalogDrawer = new CosmosCabinetDrawer { Id = Guid.NewGuid().ToString(), PartitionKey = string.Join('_', recipient.Gln.Value, origin, contentType.Value), Position = 3, OrderBy = 0 };

            for (var i = 0; i < 4; i++)
            {
                dataAvailableId = Guid.NewGuid();

                var cosmosDataAvailable = new CosmosDataAvailable
                {
                    Id = dataAvailableId.ToString(),
                    Recipient = recipient.Gln.Value,
                    Origin = origin.ToString(),
                    ContentType = contentType.Value,
                    PartitionKey = cosmosCatalogDrawer.Id,
                    SequenceNumber = i
                };

                await dataAvailableContainer
                    .Cabinet
                    .CreateItemAsync(cosmosDataAvailable)
                    .ConfigureAwait(false);
            }

            await dataAvailableContainer
                .Cabinet
                .CreateItemAsync(cosmosCatalogDrawer)
                .ConfigureAwait(false);

            var cabinetKey = new CabinetKey(recipient, origin, contentType);

            // Act
            var actual = await dataAvailableNotificationRepository
                .GetCabinetReaderAsync(cabinetKey)
                .ConfigureAwait(false);

            // Assert
            Assert.NotNull(actual);
            Assert.True(actual.CanPeek);

            var actualValue = await actual.TakeAsync().ConfigureAwait(false);
            Assert.Equal(dataAvailableId, actualValue.NotificationId.AsGuid());
            Assert.False(actual.CanPeek);
        }

        [Fact]
        public async Task GetCabinetReaderAsync_UnrelatedRecipient_ReturnsOneItem()
        {
            // Arrange
            await using var host = await SubDomainIntegrationTestHost.InitializeAsync().ConfigureAwait(false);
            var scope = host.BeginScope();

            var dataAvailableNotificationRepository = scope.GetInstance<IDataAvailableNotificationRepository>();
            var recipient = new MarketOperator(new MockedGln());
            var origin = DomainOrigin.Aggregations;
            var contentType = new ContentType("target_content");

            var dataAvailableContainer = scope.GetInstance<IDataAvailableNotificationRepositoryContainer>();

            for (var i = 0; i < 2; i++)
            {
                recipient = new MarketOperator(new MockedGln());

                var cosmosCatalogDrawer = new CosmosCabinetDrawer { Id = Guid.NewGuid().ToString(), PartitionKey = string.Join('_', recipient.Gln.Value, origin, contentType.Value), Position = 0, OrderBy = 1 };

                var cosmosDataAvailable = new CosmosDataAvailable
                {
                    Id = Guid.NewGuid().ToString(),
                    Recipient = recipient.Gln.Value,
                    Origin = origin.ToString(),
                    ContentType = contentType.Value,
                    PartitionKey = cosmosCatalogDrawer.Id,
                    SequenceNumber = 1
                };

                await dataAvailableContainer
                    .Cabinet
                    .CreateItemAsync(cosmosDataAvailable)
                    .ConfigureAwait(false);

                await dataAvailableContainer
                    .Cabinet
                    .CreateItemAsync(cosmosCatalogDrawer)
                    .ConfigureAwait(false);
            }

            var cabinetKey = new CabinetKey(recipient, origin, contentType);

            // Act
            var actual = await dataAvailableNotificationRepository
                .GetCabinetReaderAsync(cabinetKey)
                .ConfigureAwait(false);

            // Assert
            Assert.NotNull(actual);
            Assert.True(actual.CanPeek);

            var items = new List<DataAvailableNotification>();

            while (actual.CanPeek)
            {
                var notification = await actual.TakeAsync().ConfigureAwait(false);
                items.Add(notification);
            }

            Assert.Single(items);
        }

        [Fact]
        public async Task GetCabinetReaderAsync_UnrelatedOrigin_ReturnsOneItem()
        {
            // Arrange
            await using var host = await SubDomainIntegrationTestHost.InitializeAsync().ConfigureAwait(false);
            var scope = host.BeginScope();

            var dataAvailableNotificationRepository = scope.GetInstance<IDataAvailableNotificationRepository>();
            var recipient = new MarketOperator(new MockedGln());
            var origin = DomainOrigin.Aggregations;
            var contentType = new ContentType("target_content");

            var dataAvailableContainer = scope.GetInstance<IDataAvailableNotificationRepositoryContainer>();

            for (var i = 0; i < 2; i++)
            {
                var cosmosCatalogDrawer = new CosmosCabinetDrawer { Id = Guid.NewGuid().ToString(), PartitionKey = string.Join('_', recipient.Gln.Value, origin, contentType.Value), Position = 0, OrderBy = 1 };

                var cosmosDataAvailable = new CosmosDataAvailable
                {
                    Id = Guid.NewGuid().ToString(),
                    Recipient = recipient.Gln.Value,
                    Origin = origin.ToString(),
                    ContentType = contentType.Value,
                    PartitionKey = cosmosCatalogDrawer.Id,
                    SequenceNumber = 1
                };

                await dataAvailableContainer
                    .Cabinet
                    .CreateItemAsync(cosmosDataAvailable)
                    .ConfigureAwait(false);

                await dataAvailableContainer
                    .Cabinet
                    .CreateItemAsync(cosmosCatalogDrawer)
                    .ConfigureAwait(false);

                origin = DomainOrigin.Charges;
            }

            var cabinetKey = new CabinetKey(recipient, DomainOrigin.Charges, contentType);

            // Act
            var actual = await dataAvailableNotificationRepository
                .GetCabinetReaderAsync(cabinetKey)
                .ConfigureAwait(false);

            // Assert
            Assert.NotNull(actual);
            Assert.True(actual.CanPeek);

            var items = new List<DataAvailableNotification>();

            while (actual.CanPeek)
            {
                var notification = await actual.TakeAsync().ConfigureAwait(false);
                items.Add(notification);
            }

            Assert.Single(items);
        }

        [Fact]
        public async Task GetCabinetReaderAsync_UnrelatedContentType_ReturnsOneItem()
        {
            // Arrange
            await using var host = await SubDomainIntegrationTestHost.InitializeAsync().ConfigureAwait(false);
            var scope = host.BeginScope();

            var dataAvailableNotificationRepository = scope.GetInstance<IDataAvailableNotificationRepository>();
            var recipient = new MarketOperator(new MockedGln());
            var origin = DomainOrigin.Aggregations;
            var contentType = new ContentType("other_content");

            var dataAvailableContainer = scope.GetInstance<IDataAvailableNotificationRepositoryContainer>();

            for (var i = 0; i < 2; i++)
            {
                var cosmosCatalogDrawer = new CosmosCabinetDrawer { Id = Guid.NewGuid().ToString(), PartitionKey = string.Join('_', recipient.Gln.Value, origin, contentType.Value), Position = 0, OrderBy = 1 };

                var cosmosDataAvailable = new CosmosDataAvailable
                {
                    Id = Guid.NewGuid().ToString(),
                    Recipient = recipient.Gln.Value,
                    Origin = origin.ToString(),
                    ContentType = contentType.Value,
                    PartitionKey = cosmosCatalogDrawer.Id,
                    SequenceNumber = 1
                };

                await dataAvailableContainer
                    .Cabinet
                    .CreateItemAsync(cosmosDataAvailable)
                    .ConfigureAwait(false);

                await dataAvailableContainer
                    .Cabinet
                    .CreateItemAsync(cosmosCatalogDrawer)
                    .ConfigureAwait(false);

                contentType = new ContentType("actual");
            }

            var cabinetKey = new CabinetKey(recipient, origin, new ContentType("actual"));

            // Act
            var actual = await dataAvailableNotificationRepository
                .GetCabinetReaderAsync(cabinetKey)
                .ConfigureAwait(false);

            // Assert
            Assert.NotNull(actual);
            Assert.True(actual.CanPeek);

            var items = new List<DataAvailableNotification>();

            while (actual.CanPeek)
            {
                var notification = await actual.TakeAsync().ConfigureAwait(false);
                items.Add(notification);
            }

            Assert.Single(items);
        }

        [Fact]
        public async Task GetCabinetReaderAsync_FromSeveralDrawers_ReturnsAllItems()
        {
            // Arrange
            await using var host = await SubDomainIntegrationTestHost.InitializeAsync().ConfigureAwait(false);
            var scope = host.BeginScope();

            var dataAvailableNotificationRepository = scope.GetInstance<IDataAvailableNotificationRepository>();
            var recipient = new MarketOperator(new MockedGln());
            var origin = DomainOrigin.Aggregations;
            var contentType = new ContentType("target_content");

            var dataAvailableContainer = scope.GetInstance<IDataAvailableNotificationRepositoryContainer>();
            var seqNum = 0;

            for (var i = 0; i < 6; i++)
            {
                var cosmosCatalogDrawer = new CosmosCabinetDrawer { Id = Guid.NewGuid().ToString(), PartitionKey = string.Join('_', recipient.Gln.Value, origin, contentType.Value), Position = 0, OrderBy = seqNum };

                var insertions = new List<Task>();

                for (var j = 0; j < 10000; j++)
                {
                    var cosmosDataAvailable = new CosmosDataAvailable
                    {
                        Id = Guid.NewGuid().ToString(),
                        Recipient = recipient.Gln.Value,
                        Origin = origin.ToString(),
                        ContentType = contentType.Value,
                        PartitionKey = cosmosCatalogDrawer.Id,
                        SequenceNumber = seqNum
                    };

                    seqNum++;

                    insertions.Add(dataAvailableContainer
                        .Cabinet
                        .CreateItemAsync(cosmosDataAvailable));
                }

                await Task.WhenAll(insertions).ConfigureAwait(false);

                await dataAvailableContainer
                    .Cabinet
                    .CreateItemAsync(cosmosCatalogDrawer)
                    .ConfigureAwait(false);
            }

            var cabinetKey = new CabinetKey(recipient, origin, contentType);

            // Act
            var actual = await dataAvailableNotificationRepository
                .GetCabinetReaderAsync(cabinetKey)
                .ConfigureAwait(false);

            // Assert
            Assert.NotNull(actual);
            Assert.True(actual.CanPeek);

            // Must read all 60.000 items back.
            var items = new List<DataAvailableNotification>();

            while (actual.CanPeek)
            {
                var notification = await actual.TakeAsync().ConfigureAwait(false);
                items.Add(notification);
            }

            Assert.Equal(60000, items.Count);
        }

        [Fact]
        public async Task AcknowledgeAsync_AcknowledgedData_UpdatesCabinetDrawer()
        {
            // Arrange
            await using var host = await SubDomainIntegrationTestHost.InitializeAsync().ConfigureAwait(false);
            var scope = host.BeginScope();

            var dataAvailableContainer = scope.GetInstance<IDataAvailableNotificationRepositoryContainer>();

            var recipient = new MockedGln().ToString();
            var origin = DomainOrigin.MeteringPoints.ToString();
            var contentType = "mp_content_type";

            var initialCabinetDrawer = new CosmosCabinetDrawer { Id = Guid.NewGuid().ToString(), PartitionKey = string.Join('_', recipient, origin, contentType), OrderBy = 1, Position = 3 };

            var createdCabinetDrawer = await dataAvailableContainer
                .Cabinet
                .CreateItemAsync(initialCabinetDrawer)
                .ConfigureAwait(false);

            var initialCatalogEntry = new CosmosCatalogEntry { Id = Guid.NewGuid().ToString(), PartitionKey = string.Join('_', recipient, origin), ContentType = contentType, NextSequenceNumber = 3 };

            await dataAvailableContainer
                .Catalog
                .CreateItemAsync(initialCatalogEntry)
                .ConfigureAwait(false);

            var cabinetDrawerChanges = new CosmosCabinetDrawerChanges { UpdatedDrawer = createdCabinetDrawer.Resource with { Position = 5 }, UpdatedCatalogEntry = initialCatalogEntry with { Id = Guid.NewGuid().ToString(), NextSequenceNumber = 12 }, InitialCatalogEntrySequenceNumber = 3 };

            var bundleId = Guid.NewGuid().ToString();
            var bundleDocument = new CosmosBundleDocument
            {
                Id = bundleId,
                ProcessId = string.Join('_', bundleId, recipient),
                Recipient = recipient,
                Origin = origin,
                ContentType = contentType,
                Dequeued = false,
                ContentPath = "/nowhere",
                AffectedDrawers = { cabinetDrawerChanges }
            };

            var bundleContainer = scope.GetInstance<IBundleRepositoryContainer>();
            await bundleContainer
                .Container
                .UpsertItemAsync(bundleDocument)
                .ConfigureAwait(false);

            var bundle = new Bundle(
                new Uuid(bundleId),
                new MarketOperator(new GlobalLocationNumber(recipient)),
                DomainOrigin.MeteringPoints,
                new ContentType(contentType),
                Array.Empty<Uuid>());

            var target = scope.GetInstance<IDataAvailableNotificationRepository>();

            // Act
            await target
                .AcknowledgeAsync(bundle)
                .ConfigureAwait(false);

            // Assert
            var updatedCabinetDrawer = await dataAvailableContainer
                .Cabinet
                .ReadItemAsync<CosmosCabinetDrawer>(
                    initialCabinetDrawer.Id,
                    new PartitionKey(initialCabinetDrawer.PartitionKey))
                .ConfigureAwait(false);

            var updatedCatalogEntry = await dataAvailableContainer
                .Catalog
                .ReadItemAsync<CosmosCatalogEntry>(
                    cabinetDrawerChanges.UpdatedCatalogEntry.Id,
                    new PartitionKey(initialCatalogEntry.PartitionKey))
                .ConfigureAwait(false);

            Assert.Equal(
                cabinetDrawerChanges.UpdatedDrawer.Position,
                updatedCabinetDrawer.Resource.Position);

            Assert.Equal(
                cabinetDrawerChanges.UpdatedCatalogEntry.NextSequenceNumber,
                updatedCatalogEntry.Resource.NextSequenceNumber);
        }

        [Fact]
        public async Task AcknowledgeAsync_AcknowledgedDataTwice_NoExceptionWhenUpdatingPosition()
        {
            // Arrange
            await using var host = await SubDomainIntegrationTestHost.InitializeAsync().ConfigureAwait(false);
            var scope = host.BeginScope();

            var dataAvailableContainer = scope.GetInstance<IDataAvailableNotificationRepositoryContainer>();

            var recipient = new MockedGln().ToString();
            var origin = DomainOrigin.MeteringPoints.ToString();
            var contentType = "mp_content_type";

            var initialCabinetDrawer = new CosmosCabinetDrawer { Id = Guid.NewGuid().ToString(), PartitionKey = string.Join('_', recipient, origin, contentType), OrderBy = 1, Position = 3 };

            await dataAvailableContainer
                .Cabinet
                .CreateItemAsync(initialCabinetDrawer)
                .ConfigureAwait(false);

            var initialCatalogEntry = new CosmosCatalogEntry { Id = Guid.NewGuid().ToString(), PartitionKey = string.Join('_', recipient, origin), ContentType = contentType, NextSequenceNumber = 3 };

            await dataAvailableContainer
                .Catalog
                .CreateItemAsync(initialCatalogEntry)
                .ConfigureAwait(false);

            var cabinetDrawerChanges = new CosmosCabinetDrawerChanges { UpdatedDrawer = initialCabinetDrawer with { Position = 5, ETag = "no_match" }, UpdatedCatalogEntry = initialCatalogEntry with { Id = Guid.NewGuid().ToString(), NextSequenceNumber = 12 }, InitialCatalogEntrySequenceNumber = 3 };

            var bundleId = Guid.NewGuid().ToString();
            var bundleDocument = new CosmosBundleDocument
            {
                Id = bundleId,
                ProcessId = string.Join('_', bundleId, recipient),
                Recipient = recipient,
                Origin = origin,
                ContentType = contentType,
                Dequeued = false,
                ContentPath = "/nowhere",
                AffectedDrawers = { cabinetDrawerChanges }
            };

            var bundleContainer = scope.GetInstance<IBundleRepositoryContainer>();
            await bundleContainer
                .Container
                .UpsertItemAsync(bundleDocument)
                .ConfigureAwait(false);

            var bundle = new Bundle(
                new Uuid(bundleId),
                new MarketOperator(new GlobalLocationNumber(recipient)),
                DomainOrigin.MeteringPoints,
                new ContentType(contentType),
                Array.Empty<Uuid>());

            var target = scope.GetInstance<IDataAvailableNotificationRepository>();

            // Act
            await target
                .AcknowledgeAsync(bundle)
                .ConfigureAwait(false);

            // Assert
            var updatedCabinetDrawer = await dataAvailableContainer
                .Cabinet
                .ReadItemAsync<CosmosCabinetDrawer>(
                    initialCabinetDrawer.Id,
                    new PartitionKey(initialCabinetDrawer.PartitionKey))
                .ConfigureAwait(false);

            var updatedCatalogEntry = await dataAvailableContainer
                .Catalog
                .ReadItemAsync<CosmosCatalogEntry>(
                    cabinetDrawerChanges.UpdatedCatalogEntry.Id,
                    new PartitionKey(initialCatalogEntry.PartitionKey))
                .ConfigureAwait(false);

            Assert.Equal(
                initialCabinetDrawer.Position,
                updatedCabinetDrawer.Resource.Position);

            Assert.Equal(
                cabinetDrawerChanges.UpdatedCatalogEntry.NextSequenceNumber,
                updatedCatalogEntry.Resource.NextSequenceNumber);
        }

        [Fact]
        public async Task AcknowledgeAsync_AcknowledgedDataTwice_NoCatalogEntryToDelete()
        {
            // Arrange
            await using var host = await SubDomainIntegrationTestHost.InitializeAsync().ConfigureAwait(false);
            var scope = host.BeginScope();

            var dataAvailableContainer = scope.GetInstance<IDataAvailableNotificationRepositoryContainer>();

            var recipient = new MockedGln().ToString();
            var origin = DomainOrigin.MeteringPoints.ToString();
            var contentType = "mp_content_type";

            var initialCabinetDrawer = new CosmosCabinetDrawer { Id = Guid.NewGuid().ToString(), PartitionKey = string.Join('_', recipient, origin, contentType), OrderBy = 1, Position = 3 };

            var createdCabinetDrawer = await dataAvailableContainer
                .Cabinet
                .CreateItemAsync(initialCabinetDrawer)
                .ConfigureAwait(false);

            var cosmosCabinetDrawerChanges = new CosmosCabinetDrawerChanges { UpdatedDrawer = createdCabinetDrawer.Resource with { Position = 5 }, UpdatedCatalogEntry = null, InitialCatalogEntrySequenceNumber = 12 };

            var bundleId = Guid.NewGuid().ToString();
            var bundleDocument = new CosmosBundleDocument
            {
                Id = bundleId,
                ProcessId = string.Join('_', bundleId, recipient),
                Recipient = recipient,
                Origin = origin,
                ContentType = contentType,
                Dequeued = false,
                ContentPath = "/nowhere",
                AffectedDrawers = { cosmosCabinetDrawerChanges }
            };

            var bundleContainer = scope.GetInstance<IBundleRepositoryContainer>();
            await bundleContainer
                .Container
                .UpsertItemAsync(bundleDocument)
                .ConfigureAwait(false);

            var bundle = new Bundle(
                new Uuid(bundleId),
                new MarketOperator(new GlobalLocationNumber(recipient)),
                DomainOrigin.MeteringPoints,
                new ContentType(contentType),
                Array.Empty<Uuid>());

            var target = scope.GetInstance<IDataAvailableNotificationRepository>();

            // Act
            await target
                .AcknowledgeAsync(bundle)
                .ConfigureAwait(false);

            // Assert
            var updatedCabinetDrawer = await dataAvailableContainer
                .Cabinet
                .ReadItemAsync<CosmosCabinetDrawer>(
                    initialCabinetDrawer.Id,
                    new PartitionKey(initialCabinetDrawer.PartitionKey))
                .ConfigureAwait(false);

            Assert.Equal(
                cosmosCabinetDrawerChanges.UpdatedDrawer.Position,
                updatedCabinetDrawer.Resource.Position);
        }

        [Fact]
        public async Task SaveAsync_NewData_CanBeRead()
        {
            // Arrange
            await using var host = await SubDomainIntegrationTestHost.InitializeAsync().ConfigureAwait(false);
            var scope = host.BeginScope();

            var dataAvailableNotificationRepository = scope.GetInstance<IDataAvailableNotificationRepository>();

            var recipient = new MarketOperator(new MockedGln());
            var expected = new DataAvailableNotification(
                new Uuid(Guid.NewGuid()),
                recipient,
                new ContentType("target"),
                DomainOrigin.Aggregations,
                new SupportsBundling(false),
                new Weight(1),
                new SequenceNumber(1));

            // Act
            //await dataAvailableNotificationRepository
            //    .SaveAsync(expected)
            //    .ConfigureAwait(false);

            // TODO: Must be fixed before completing.
            //var actual = await dataAvailableNotificationRepository
            //    .GetNextUnacknowledgedAsync(recipient)
            //    .ConfigureAwait(false);
            //// Assert
            //Assert.NotNull(actual);
            //Assert.Equal(expected.NotificationId, actual!.NotificationId);
            //Assert.Equal(expected.ContentType, actual.ContentType);
            //Assert.Equal(expected.Recipient, actual.Recipient);
            //Assert.Equal(expected.Origin, actual.Origin);
            //Assert.Equal(expected.SupportsBundling, actual.SupportsBundling);
            //Assert.Equal(expected.Weight, actual.Weight);
        }

        [Fact]
        public async Task SaveAsync_OneCabinetKey_DataIsSaved()
        {
            // Arrange
            await using var host = await SubDomainIntegrationTestHost.InitializeAsync().ConfigureAwait(false);
            var scope = host.BeginScope();

            var dataAvailableNotificationRepository = scope.GetInstance<IDataAvailableNotificationRepository>();
            var sequenceNumberRepository = scope.GetInstance<ISequenceNumberRepository>();

            var (expectedDataAvailableNotifications, key) = GetNewData();
            var expectedDataAvailableNotificationList = expectedDataAvailableNotifications.ToList();

            var cabinetKey = new CabinetKey(key.Recipient, key.Origin, key.ContentType);
            var maxSequenceNumber = expectedDataAvailableNotificationList.Max(x => x.SequenceNumber.Value);

            // Act
            await dataAvailableNotificationRepository.SaveAsync(expectedDataAvailableNotificationList, key).ConfigureAwait(false);

            await sequenceNumberRepository
                .AdvanceSequenceNumberAsync(new SequenceNumber(maxSequenceNumber))
                .ConfigureAwait(false);

            var reader = await dataAvailableNotificationRepository
                .GetCabinetReaderAsync(cabinetKey)
                .ConfigureAwait(false);

            var actualDataAvailableNotifications = new List<DataAvailableNotification>();

            while (reader.CanPeek)
            {
                actualDataAvailableNotifications.Add(await reader.TakeAsync().ConfigureAwait(false));
            }

            // Assert
            Assert.Equal(expectedDataAvailableNotificationList[0].NotificationId, actualDataAvailableNotifications[0].NotificationId);
            Assert.Equal(expectedDataAvailableNotificationList[1].NotificationId, actualDataAvailableNotifications[1].NotificationId);
            Assert.Equal(expectedDataAvailableNotificationList[2].NotificationId, actualDataAvailableNotifications[2].NotificationId);
        }

        private static (IEnumerable<DataAvailableNotification> DataAvailableNotifications, CabinetKey Key) GetNewData()
        {
            var mockedGln = new MockedGln();

            var dataAvailableNotifications = new List<DataAvailableNotification>
            {
                new DataAvailableNotification(
                    new Uuid(Guid.NewGuid()),
                    new MarketOperator(mockedGln),
                    new ContentType("fake_value_Charges123"),
                    DomainOrigin.Charges,
                    new SupportsBundling(true),
                    new Weight(1),
                    new SequenceNumber(1)),
                new DataAvailableNotification(
                    new Uuid(Guid.NewGuid()),
                    new MarketOperator(mockedGln),
                    new ContentType("fake_value_Charges123"),
                    DomainOrigin.Charges,
                    new SupportsBundling(true),
                    new Weight(1),
                    new SequenceNumber(4)),
                new DataAvailableNotification(
                    new Uuid(Guid.NewGuid()),
                    new MarketOperator(mockedGln),
                    new ContentType("fake_value_Charges123"),
                    DomainOrigin.Charges,
                    new SupportsBundling(true),
                    new Weight(1),
                    new SequenceNumber(11)),
            };

            var cabinetKey = new CabinetKey(
                dataAvailableNotifications[0].Recipient,
                dataAvailableNotifications[0].Origin,
                dataAvailableNotifications[0].ContentType);

            return (dataAvailableNotifications, cabinetKey);
        }
    }
}
