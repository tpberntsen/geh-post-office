﻿// Copyright 2020 Energinet DataHub A/S
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
using FluentAssertions;
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
        public async Task GetNextUnacknowledgedAsync_NoData_ReturnsNull()
        {
            // Arrange
            await using var host = await SubDomainIntegrationTestHost.InitializeAsync().ConfigureAwait(false);
            var scope = host.BeginScope();

            var dataAvailableNotificationRepository = scope.GetInstance<IDataAvailableNotificationRepository>();
            var recipient = new MarketOperator(new MockedGln());

            // Act
            var actual = await dataAvailableNotificationRepository
                .GetNextUnacknowledgedAsync(recipient)
                .ConfigureAwait(false);

            // Assert
            Assert.Null(actual);
        }

        [Fact]
        public async Task GetNextUnacknowledgedAsync_HasData_ReturnsData()
        {
            // Arrange
            await using var host = await SubDomainIntegrationTestHost.InitializeAsync().ConfigureAwait(false);
            var scope = host.BeginScope();

            var dataAvailableNotificationRepository = scope.GetInstance<IDataAvailableNotificationRepository>();

            var recipient = new MarketOperator(new MockedGln());
            var expected = new DataAvailableNotification(
                new Uuid(Guid.NewGuid()),
                recipient,
                new ContentType("fake_value"),
                DomainOrigin.Aggregations,
                new SupportsBundling(false),
                new Weight(1));

            await dataAvailableNotificationRepository.SaveAsync(expected).ConfigureAwait(false);

            // Act
            var actual = await dataAvailableNotificationRepository
                .GetNextUnacknowledgedAsync(recipient)
                .ConfigureAwait(false);

            // Assert
            Assert.NotNull(actual);
            Assert.Equal(expected.NotificationId, actual!.NotificationId);
            Assert.Equal(expected.ContentType, actual.ContentType);
            Assert.Equal(expected.Recipient, actual.Recipient);
            Assert.Equal(expected.Origin, actual.Origin);
            Assert.Equal(expected.SupportsBundling, actual.SupportsBundling);
            Assert.Equal(expected.Weight, actual.Weight);
        }

        [Fact]
        public async Task GetNextUnacknowledgedAsync_HasUnrelatedData_ReturnsData()
        {
            // Arrange
            await using var host = await SubDomainIntegrationTestHost.InitializeAsync().ConfigureAwait(false);
            var scope = host.BeginScope();

            var dataAvailableNotificationRepository = scope.GetInstance<IDataAvailableNotificationRepository>();

            var recipient = new MarketOperator(new MockedGln());
            var expected = new DataAvailableNotification(
                new Uuid(Guid.NewGuid()),
                recipient,
                new ContentType("fake_value"),
                DomainOrigin.Aggregations,
                new SupportsBundling(false),
                new Weight(1));

            await dataAvailableNotificationRepository.SaveAsync(expected).ConfigureAwait(false);

            // Act
            var actual = await dataAvailableNotificationRepository
                .GetNextUnacknowledgedAsync(recipient, DomainOrigin.MarketRoles)
                .ConfigureAwait(false);

            // Assert
            Assert.Null(actual);
        }

        [Fact]
        public async Task GetNextUnacknowledgedAsync_HasMultipleData_ReturnsOldestData()
        {
            // Arrange
            await using var host = await SubDomainIntegrationTestHost.InitializeAsync().ConfigureAwait(false);
            var scope = host.BeginScope();

            var dataAvailableNotificationRepository = scope.GetInstance<IDataAvailableNotificationRepository>();

            var recipient = new MarketOperator(new MockedGln());
            var expected = new DataAvailableNotification(
                new Uuid(Guid.NewGuid()),
                recipient,
                new ContentType("fake_value"),
                DomainOrigin.Aggregations,
                new SupportsBundling(false),
                new Weight(1));

            await dataAvailableNotificationRepository.SaveAsync(expected).ConfigureAwait(false);

            for (var i = 0; i < 5; i++)
            {
                var other = new DataAvailableNotification(
                    new Uuid(Guid.NewGuid()),
                    expected.Recipient,
                    expected.ContentType,
                    expected.Origin,
                    expected.SupportsBundling,
                    expected.Weight);

                await dataAvailableNotificationRepository.SaveAsync(other).ConfigureAwait(false);
            }

            // Act
            var actual = await dataAvailableNotificationRepository
                .GetNextUnacknowledgedAsync(recipient)
                .ConfigureAwait(false);

            // Assert
            Assert.NotNull(actual);
            Assert.Equal(expected.NotificationId, actual!.NotificationId);
            Assert.Equal(expected.ContentType, actual.ContentType);
            Assert.Equal(expected.Recipient, actual.Recipient);
            Assert.Equal(expected.Origin, actual.Origin);
            Assert.Equal(expected.SupportsBundling, actual.SupportsBundling);
            Assert.Equal(expected.Weight, actual.Weight);
        }

        [Fact]
        public async Task GetNextUnacknowledgedAsync_MultipleContentType_ReturnsAllForSameContentType()
        {
            // Arrange
            await using var host = await SubDomainIntegrationTestHost.InitializeAsync().ConfigureAwait(false);
            var scope = host.BeginScope();

            var dataAvailableNotificationRepository = scope.GetInstance<IDataAvailableNotificationRepository>();

            var recipient = new MarketOperator(new MockedGln());
            var expected = new DataAvailableNotification(
                new Uuid(Guid.NewGuid()),
                recipient,
                new ContentType("fake_value"),
                DomainOrigin.Aggregations,
                new SupportsBundling(true),
                new Weight(1));

            for (var i = 0; i < 5; i++)
            {
                var other = new DataAvailableNotification(
                    new Uuid(Guid.NewGuid()),
                    expected.Recipient,
                    new ContentType("target"),
                    expected.Origin,
                    expected.SupportsBundling,
                    expected.Weight);

                await dataAvailableNotificationRepository.SaveAsync(other).ConfigureAwait(false);
            }

            await dataAvailableNotificationRepository.SaveAsync(expected).ConfigureAwait(false);

            // Act
            var actual = await dataAvailableNotificationRepository
                .GetNextUnacknowledgedAsync(recipient, DomainOrigin.Aggregations, new ContentType("target"), new Weight(5))
                .ConfigureAwait(false);

            // Assert
            Assert.Equal(5, actual.Count());
        }

        [Fact]
        public async Task GetNextUnacknowledgedAsync_MultipleContentType_ReturnsFilteredContentType()
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
                new SupportsBundling(true),
                new Weight(1));

            for (var i = 0; i < 5; i++)
            {
                var other = new DataAvailableNotification(
                    new Uuid(Guid.NewGuid()),
                    expected.Recipient,
                    new ContentType("fake_value"),
                    expected.Origin,
                    expected.SupportsBundling,
                    expected.Weight);

                await dataAvailableNotificationRepository.SaveAsync(other).ConfigureAwait(false);
            }

            await dataAvailableNotificationRepository.SaveAsync(expected).ConfigureAwait(false);

            // Act
            var actual = await dataAvailableNotificationRepository
                .GetNextUnacknowledgedAsync(recipient, DomainOrigin.Aggregations, expected.ContentType, new Weight(1))
                .ConfigureAwait(false);

            // Assert
            Assert.NotNull(actual);

            var list = actual.ToList();
            Assert.Single(list);
            Assert.Single(list, x => x.NotificationId == expected.NotificationId);
        }

        [Fact]
        public async Task GetNextUnacknowledgedAsync_LimitedWeight_ReturnsUpToWeight()
        {
            // Arrange
            await using var host = await SubDomainIntegrationTestHost.InitializeAsync().ConfigureAwait(false);
            var scope = host.BeginScope();

            var dataAvailableNotificationRepository = scope.GetInstance<IDataAvailableNotificationRepository>();

            var maxWeight = new Weight(3);
            var recipient = new MarketOperator(new MockedGln());
            var expected = new DataAvailableNotification(
                new Uuid(Guid.NewGuid()),
                recipient,
                new ContentType("target"),
                DomainOrigin.Aggregations,
                new SupportsBundling(true),
                new Weight(1));

            for (var i = 0; i < 5; i++)
            {
                var other = new DataAvailableNotification(
                    new Uuid(Guid.NewGuid()),
                    expected.Recipient,
                    expected.ContentType,
                    expected.Origin,
                    expected.SupportsBundling,
                    expected.Weight);

                await dataAvailableNotificationRepository.SaveAsync(other).ConfigureAwait(false);
            }

            await dataAvailableNotificationRepository.SaveAsync(expected).ConfigureAwait(false);

            // Act
            var actual = await dataAvailableNotificationRepository
                .GetNextUnacknowledgedAsync(recipient, DomainOrigin.Aggregations, new ContentType("target"), maxWeight)
                .ConfigureAwait(false);

            // Assert
            Assert.Equal(3, actual.Count());
        }

        [Fact]
        public async Task GetNextUnacknowledgedAsync_LargeWeight_ReturnsAtLeastOneItem()
        {
            // Arrange
            await using var host = await SubDomainIntegrationTestHost.InitializeAsync().ConfigureAwait(false);
            var scope = host.BeginScope();

            var dataAvailableNotificationRepository = scope.GetInstance<IDataAvailableNotificationRepository>();

            var maxWeight = new Weight(3);
            var recipient = new MarketOperator(new MockedGln());
            var expected = new DataAvailableNotification(
                new Uuid(Guid.NewGuid()),
                recipient,
                new ContentType("target"),
                DomainOrigin.Aggregations,
                new SupportsBundling(true),
                new Weight(10));

            await dataAvailableNotificationRepository.SaveAsync(expected).ConfigureAwait(false);

            // Act
            var actual = await dataAvailableNotificationRepository
                .GetNextUnacknowledgedAsync(recipient, DomainOrigin.Aggregations, new ContentType("target"), maxWeight)
                .ConfigureAwait(false);

            // Assert
            Assert.Single(actual);
        }

        [Fact]
        public async Task GetNextUnacknowledgedAsync_NoBundling_ReturnsAtLeastOneItem()
        {
            // Arrange
            await using var host = await SubDomainIntegrationTestHost.InitializeAsync().ConfigureAwait(false);
            var scope = host.BeginScope();

            var dataAvailableNotificationRepository = scope.GetInstance<IDataAvailableNotificationRepository>();

            var maxWeight = new Weight(100);
            var recipient = new MarketOperator(new MockedGln());
            var expected = new DataAvailableNotification(
                new Uuid(Guid.NewGuid()),
                recipient,
                new ContentType("target"),
                DomainOrigin.Aggregations,
                new SupportsBundling(false),
                new Weight(0));

            await dataAvailableNotificationRepository.SaveAsync(expected).ConfigureAwait(false);

            // Act
            var actual = await dataAvailableNotificationRepository
                .GetNextUnacknowledgedAsync(recipient, DomainOrigin.Aggregations, new ContentType("target"), maxWeight)
                .ConfigureAwait(false);

            // Assert
            Assert.Single(actual);
        }

        [Fact]
        public async Task AcknowledgeAsync_AcknowledgedData_DataNotReturned()
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
                new Weight(1));

            await dataAvailableNotificationRepository.SaveAsync(expected).ConfigureAwait(false);

            // Act
            var notAcknowledged = await dataAvailableNotificationRepository
                .GetNextUnacknowledgedAsync(recipient)
                .ConfigureAwait(false);

            await dataAvailableNotificationRepository
                .AcknowledgeAsync(recipient, new[] { expected.NotificationId })
                .ConfigureAwait(false);

            var acknowledged = await dataAvailableNotificationRepository
                .GetNextUnacknowledgedAsync(recipient)
                .ConfigureAwait(false);

            // Assert
            Assert.NotNull(notAcknowledged);
            Assert.Null(acknowledged);
        }

        [Fact]
        public async Task AcknowledgeAsync_BundleOverItemCap_AcknowledgesWithoutError()
        {
            // Arrange
            await using var host = await SubDomainIntegrationTestHost.InitializeAsync().ConfigureAwait(false);
            var scope = host.BeginScope();

            var dataAvailableNotificationRepository = scope.GetInstance<IDataAvailableNotificationRepository>();

            var recipient = new MarketOperator(new MockedGln());
            var addedUuids = new List<Uuid>();

            for (var i = 0; i < 110; i++)
            {
                var notificationId = new Uuid(Guid.NewGuid());
                addedUuids.Add(notificationId);

                var expected = new DataAvailableNotification(
                    notificationId,
                    recipient,
                    new ContentType("target"),
                    DomainOrigin.Aggregations,
                    new SupportsBundling(true),
                    new Weight(1));

                await dataAvailableNotificationRepository.SaveAsync(expected).ConfigureAwait(false);
            }

            // Act
            await dataAvailableNotificationRepository
                .AcknowledgeAsync(recipient, addedUuids)
                .ConfigureAwait(false);

            var acknowledged = await dataAvailableNotificationRepository
                .GetNextUnacknowledgedAsync(recipient)
                .ConfigureAwait(false);

            // Assert
            Assert.Null(acknowledged);
        }

        [Fact]
        public async Task AcknowledgeAsync_AcknowledgedData_UpdatesSubPartitionLookup()
        {
            // Arrange
            await using var host = await SubDomainIntegrationTestHost.InitializeAsync().ConfigureAwait(false);
            var scope = host.BeginScope();

            var dataAvailableContainer = scope.GetInstance<IDataAvailableNotificationRepositoryContainer>();

            var recipient = new MockedGln().ToString();
            var origin = DomainOrigin.MeteringPoints.ToString();
            var contentType = "mp_content_type";

            var initialCosmosSubPartitionLookup = new CosmosSubPartitionLookup
            {
                Id = Guid.NewGuid().ToString(),
                PartitionKey = string.Join('_', recipient, origin, contentType),
                InitialSequenceNumber = 1,
                CurrentCursor = 3
            };

            var createdSubPartitionLookup = await dataAvailableContainer
                .Container
                .CreateItemAsync(initialCosmosSubPartitionLookup)
                .ConfigureAwait(false);

            var initialCosmosContentTypeLookup = new CosmosContentTypeLookup
            {
                Id = Guid.NewGuid().ToString(),
                PartitionKey = string.Join('_', recipient, origin),
                ContentType = contentType,
                NextSequenceNumber = 3
            };

            await dataAvailableContainer
                .Container
                .CreateItemAsync(initialCosmosContentTypeLookup)
                .ConfigureAwait(false);

            var cosmosSubPartitionPeekChanges = new CosmosSubPartitionPeekChanges
            {
                ContentTypeLookupId = Guid.NewGuid().ToString(),
                ContentTypeLookupSequenceNumber = initialCosmosContentTypeLookup.NextSequenceNumber,
                SubPartitionLookupId = initialCosmosSubPartitionLookup.Id,
                SubPartitionInitialSequenceNumber = initialCosmosSubPartitionLookup.InitialSequenceNumber,
                SubPartitionNextCursorPosition = 5,
                SubPartitionNextSequenceNumber = 12,
                SubPartitionLookupExpectedETag = createdSubPartitionLookup.ETag
            };

            var bundleId = Guid.NewGuid().ToString();
            var bundleDocument = new CosmosBundleDocument2
            {
                Id = bundleId,
                ProcessId = string.Join('_', bundleId, recipient),
                Recipient = recipient,
                Origin = origin,
                MessageType = contentType,
                Dequeued = false,
                ContentPath = "/nowhere",
                AffectedSubPartitions = { cosmosSubPartitionPeekChanges }
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
            var updatedCosmosSubPartitionLookup = await dataAvailableContainer
                .Container
                .ReadItemAsync<CosmosSubPartitionLookup>(
                    initialCosmosSubPartitionLookup.Id,
                    new PartitionKey(initialCosmosSubPartitionLookup.PartitionKey))
                .ConfigureAwait(false);

            var updatedCosmosContentTypeLookup = await dataAvailableContainer
                .Container
                .ReadItemAsync<CosmosContentTypeLookup>(
                    cosmosSubPartitionPeekChanges.ContentTypeLookupId,
                    new PartitionKey(initialCosmosContentTypeLookup.PartitionKey))
                .ConfigureAwait(false);

            Assert.Equal(
                cosmosSubPartitionPeekChanges.SubPartitionNextCursorPosition,
                updatedCosmosSubPartitionLookup.Resource.CurrentCursor);

            Assert.Equal(
                cosmosSubPartitionPeekChanges.SubPartitionNextSequenceNumber,
                updatedCosmosContentTypeLookup.Resource.NextSequenceNumber);
        }

        [Fact]
        public async Task AcknowledgeAsync_AcknowledgedDataTwice_NoExceptionWhenUpdatingCursor()
        {
            // Arrange
            await using var host = await SubDomainIntegrationTestHost.InitializeAsync().ConfigureAwait(false);
            var scope = host.BeginScope();

            var dataAvailableContainer = scope.GetInstance<IDataAvailableNotificationRepositoryContainer>();

            var recipient = new MockedGln().ToString();
            var origin = DomainOrigin.MeteringPoints.ToString();
            var contentType = "mp_content_type";

            var initialCosmosSubPartitionLookup = new CosmosSubPartitionLookup
            {
                Id = Guid.NewGuid().ToString(),
                PartitionKey = string.Join('_', recipient, origin, contentType),
                InitialSequenceNumber = 1,
                CurrentCursor = 3
            };

            await dataAvailableContainer
                .Container
                .CreateItemAsync(initialCosmosSubPartitionLookup)
                .ConfigureAwait(false);

            var initialCosmosContentTypeLookup = new CosmosContentTypeLookup
            {
                Id = Guid.NewGuid().ToString(),
                PartitionKey = string.Join('_', recipient, origin),
                ContentType = contentType,
                NextSequenceNumber = 3
            };

            await dataAvailableContainer
                .Container
                .CreateItemAsync(initialCosmosContentTypeLookup)
                .ConfigureAwait(false);

            var cosmosSubPartitionPeekChanges = new CosmosSubPartitionPeekChanges
            {
                ContentTypeLookupId = Guid.NewGuid().ToString(),
                ContentTypeLookupSequenceNumber = initialCosmosContentTypeLookup.NextSequenceNumber,
                SubPartitionLookupId = initialCosmosSubPartitionLookup.Id,
                SubPartitionInitialSequenceNumber = initialCosmosSubPartitionLookup.InitialSequenceNumber,
                SubPartitionNextCursorPosition = 5,
                SubPartitionNextSequenceNumber = 12,
                SubPartitionLookupExpectedETag = "no_match"
            };

            var bundleId = Guid.NewGuid().ToString();
            var bundleDocument = new CosmosBundleDocument2
            {
                Id = bundleId,
                ProcessId = string.Join('_', bundleId, recipient),
                Recipient = recipient,
                Origin = origin,
                MessageType = contentType,
                Dequeued = false,
                ContentPath = "/nowhere",
                AffectedSubPartitions = { cosmosSubPartitionPeekChanges }
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
            var updatedCosmosSubPartitionLookup = await dataAvailableContainer
                .Container
                .ReadItemAsync<CosmosSubPartitionLookup>(
                    initialCosmosSubPartitionLookup.Id,
                    new PartitionKey(initialCosmosSubPartitionLookup.PartitionKey))
                .ConfigureAwait(false);

            var updatedCosmosContentTypeLookup = await dataAvailableContainer
                .Container
                .ReadItemAsync<CosmosContentTypeLookup>(
                    cosmosSubPartitionPeekChanges.ContentTypeLookupId,
                    new PartitionKey(initialCosmosContentTypeLookup.PartitionKey))
                .ConfigureAwait(false);

            Assert.Equal(
                initialCosmosSubPartitionLookup.CurrentCursor,
                updatedCosmosSubPartitionLookup.Resource.CurrentCursor);

            Assert.Equal(
                cosmosSubPartitionPeekChanges.SubPartitionNextSequenceNumber,
                updatedCosmosContentTypeLookup.Resource.NextSequenceNumber);
        }

        [Fact]
        public async Task AcknowledgeAsync_AcknowledgedDataTwice_NoContentTypeLookupToDelete()
        {
            // Arrange
            await using var host = await SubDomainIntegrationTestHost.InitializeAsync().ConfigureAwait(false);
            var scope = host.BeginScope();

            var dataAvailableContainer = scope.GetInstance<IDataAvailableNotificationRepositoryContainer>();

            var recipient = new MockedGln().ToString();
            var origin = DomainOrigin.MeteringPoints.ToString();
            var contentType = "mp_content_type";

            var initialCosmosSubPartitionLookup = new CosmosSubPartitionLookup
            {
                Id = Guid.NewGuid().ToString(),
                PartitionKey = string.Join('_', recipient, origin, contentType),
                InitialSequenceNumber = 1,
                CurrentCursor = 3
            };

            var createdSubPartitionLookup = await dataAvailableContainer
                .Container
                .CreateItemAsync(initialCosmosSubPartitionLookup)
                .ConfigureAwait(false);

            var cosmosSubPartitionPeekChanges = new CosmosSubPartitionPeekChanges
            {
                ContentTypeLookupId = Guid.NewGuid().ToString(),
                ContentTypeLookupSequenceNumber = 13,
                SubPartitionLookupId = initialCosmosSubPartitionLookup.Id,
                SubPartitionInitialSequenceNumber = initialCosmosSubPartitionLookup.InitialSequenceNumber,
                SubPartitionNextCursorPosition = 5,
                SubPartitionNextSequenceNumber = 12,
                SubPartitionLookupExpectedETag = createdSubPartitionLookup.ETag
            };

            var bundleId = Guid.NewGuid().ToString();
            var bundleDocument = new CosmosBundleDocument2
            {
                Id = bundleId,
                ProcessId = string.Join('_', bundleId, recipient),
                Recipient = recipient,
                Origin = origin,
                MessageType = contentType,
                Dequeued = false,
                ContentPath = "/nowhere",
                AffectedSubPartitions = { cosmosSubPartitionPeekChanges }
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
            var updatedCosmosSubPartitionLookup = await dataAvailableContainer
                .Container
                .ReadItemAsync<CosmosSubPartitionLookup>(
                    initialCosmosSubPartitionLookup.Id,
                    new PartitionKey(initialCosmosSubPartitionLookup.PartitionKey))
                .ConfigureAwait(false);

            Assert.Equal(
                cosmosSubPartitionPeekChanges.SubPartitionNextCursorPosition,
                updatedCosmosSubPartitionLookup.Resource.CurrentCursor);
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
                new Weight(1));

            // Act
            await dataAvailableNotificationRepository
                .SaveAsync(expected)
                .ConfigureAwait(false);

            var actual = await dataAvailableNotificationRepository
                .GetNextUnacknowledgedAsync(recipient)
                .ConfigureAwait(false);

            // Assert
            Assert.NotNull(actual);
            Assert.Equal(expected.NotificationId, actual!.NotificationId);
            Assert.Equal(expected.ContentType, actual.ContentType);
            Assert.Equal(expected.Recipient, actual.Recipient);
            Assert.Equal(expected.Origin, actual.Origin);
            Assert.Equal(expected.SupportsBundling, actual.SupportsBundling);
            Assert.Equal(expected.Weight, actual.Weight);
        }
    }
}
