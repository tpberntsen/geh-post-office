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
using Energinet.DataHub.PostOffice.Domain.Repositories;
using Energinet.DataHub.PostOffice.IntegrationTests.Common;
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
        public async Task AcknowledgeAsync_AcrossPartitionKeys_OneRecipientCannotAffectAnother()
        {
            // Arrange
            await using var host = await SubDomainIntegrationTestHost.InitializeAsync().ConfigureAwait(false);
            var scope = host.BeginScope();

            var dataAvailableNotificationRepository = scope.GetInstance<IDataAvailableNotificationRepository>();

            var recipientA = new MarketOperator(new MockedGln());
            var recipientB = new MarketOperator(new MockedGln());
            var commonGuid = new Uuid(Guid.NewGuid());

            // Two identical notifications for two different recipients.
            // The uuid is only unique pr. partition and can be reused.
            var notificationA = new DataAvailableNotification(
                commonGuid,
                recipientA,
                new ContentType("target"),
                DomainOrigin.Aggregations,
                new SupportsBundling(false),
                new Weight(1));

            // Everything should match to detect change of partition key.
            var notificationB = new DataAvailableNotification(
                commonGuid,
                recipientB,
                notificationA.ContentType,
                notificationA.Origin,
                notificationA.SupportsBundling,
                notificationA.Weight);

            await dataAvailableNotificationRepository.SaveAsync(notificationA).ConfigureAwait(false);
            await dataAvailableNotificationRepository.SaveAsync(notificationB).ConfigureAwait(false);

            // Assert: Read notifications back.
            Assert.NotNull(await dataAvailableNotificationRepository.GetNextUnacknowledgedAsync(recipientA).ConfigureAwait(false));
            Assert.NotNull(await dataAvailableNotificationRepository.GetNextUnacknowledgedAsync(recipientB).ConfigureAwait(false));

            // Act
            await dataAvailableNotificationRepository
                .AcknowledgeAsync(recipientA, new[] { commonGuid })
                .ConfigureAwait(false);

            // Assert: Only one notification should be acknowledged.
            Assert.Null(await dataAvailableNotificationRepository.GetNextUnacknowledgedAsync(recipientA).ConfigureAwait(false));
            Assert.NotNull(await dataAvailableNotificationRepository.GetNextUnacknowledgedAsync(recipientB).ConfigureAwait(false));
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
