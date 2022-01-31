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
using System.ComponentModel.DataAnnotations;
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
        [Theory]
        [InlineData(1)]
        [InlineData(2)]
        [InlineData(10000)]
        [InlineData(10001)]
        public async Task SaveAsync_ManyNotifications_CanReadBack(int count)
        {
            // Arrange
            await using var host = await SubDomainIntegrationTestHost.InitializeAsync().ConfigureAwait(false);
            var scope = host.BeginScope();

            var dataAvailableNotificationRepository = scope.GetInstance<IDataAvailableNotificationRepository>();

            var recipient = new MarketOperator(new MockedGln());
            var notifications = CreateInfinite(recipient, 1)
                .Take(count)
                .ToList();

            // Act
            await dataAvailableNotificationRepository
                .SaveAsync(new CabinetKey(notifications[0]), notifications)
                .ConfigureAwait(false);

            var reader = await dataAvailableNotificationRepository
                .GetNextUnacknowledgedAsync(recipient, DomainOrigin.Charges)
                .ConfigureAwait(false);

            // Assert
            Assert.NotNull(reader);

            var items = await reader!
                .ReadToEndAsync()
                .ConfigureAwait(false);

            for (var i = 0; i < count; i++)
            {
                Assert.Equal(notifications[i].NotificationId, items[i].NotificationId);
                Assert.Equal(notifications[i].Recipient, items[i].Recipient);
                Assert.Equal(notifications[i].Origin, items[i].Origin);
                Assert.Equal(notifications[i].ContentType, items[i].ContentType);
                Assert.Equal(notifications[i].SupportsBundling, items[i].SupportsBundling);
                Assert.Equal(notifications[i].Weight, items[i].Weight);
                Assert.Equal(notifications[i].SequenceNumber, items[i].SequenceNumber);
            }
        }

        [Fact]
        public async Task SaveAsync_IdempotencySkip_CannotReadBack()
        {
            // Arrange
            await using var host = await SubDomainIntegrationTestHost.InitializeAsync().ConfigureAwait(false);
            var scope = host.BeginScope();

            var dataAvailableNotificationRepository = scope.GetInstance<IDataAvailableNotificationRepository>();

            var recipient = new MarketOperator(new MockedGln());
            var notifications = CreateInfinite(recipient, 1)
                .Take(1)
                .ToList();

            // Act
            await dataAvailableNotificationRepository
                .SaveAsync(new CabinetKey(notifications[0]), new[] { notifications[0], notifications[0] })
                .ConfigureAwait(false);

            var reader = await dataAvailableNotificationRepository
                .GetNextUnacknowledgedAsync(recipient, DomainOrigin.Charges)
                .ConfigureAwait(false);

            // Assert
            Assert.NotNull(reader);

            var items = await reader!
                .ReadToEndAsync()
                .ConfigureAwait(false);

            Assert.Single(items);

            for (var i = 0; i < items.Count; i++)
            {
                Assert.Equal(notifications[i].NotificationId, items[i].NotificationId);
                Assert.Equal(notifications[i].Recipient, items[i].Recipient);
                Assert.Equal(notifications[i].Origin, items[i].Origin);
                Assert.Equal(notifications[i].ContentType, items[i].ContentType);
                Assert.Equal(notifications[i].SupportsBundling, items[i].SupportsBundling);
                Assert.Equal(notifications[i].Weight, items[i].Weight);
                Assert.Equal(notifications[i].SequenceNumber, items[i].SequenceNumber);
            }
        }

        [Fact]
        public async Task SaveAsync_IdempotencyFailed_ThrowsException()
        {
            // Arrange
            await using var host = await SubDomainIntegrationTestHost.InitializeAsync().ConfigureAwait(false);
            var scope = host.BeginScope();

            var dataAvailableNotificationRepository = scope.GetInstance<IDataAvailableNotificationRepository>();

            var recipient = new MarketOperator(new MockedGln());

            var notification1 = new DataAvailableNotification(
                new Uuid(Guid.NewGuid()),
                recipient,
                new ContentType("a"),
                DomainOrigin.Charges,
                new SupportsBundling(true),
                new Weight(1),
                new SequenceNumber(1),
                new DocumentType("RSM??"));
            var notification2 = new DataAvailableNotification(
                notification1.NotificationId,
                notification1.Recipient,
                new ContentType("b"),
                notification1.Origin,
                notification1.SupportsBundling,
                notification1.Weight,
                new SequenceNumber(2),
                new DocumentType("RSM??"));

            var notifications = new[] { notification1, notification2 };

            // Act + Assert
            await Assert
                .ThrowsAsync<ValidationException>(
                () => dataAvailableNotificationRepository.SaveAsync(new CabinetKey(notifications[0]), notifications))
                .ConfigureAwait(false);
        }

        [Fact]
        public async Task GetNextUnacknowledgedAsync_MultipleDomains_ReturnsSmallestSequenceNumber()
        {
            // Arrange
            await using var host = await MarketOperatorIntegrationTestHost.InitializeAsync().ConfigureAwait(false);
            var scope = host.BeginScope();

            var dataAvailableNotificationRepository = scope.GetInstance<IDataAvailableNotificationRepository>();

            var recipient = new MarketOperator(new MockedGln());
            var aggregations = CreateInfinite(recipient, 9, DomainOrigin.Aggregations).Take(5).ToList();
            var marketRoles = CreateInfinite(recipient, 1, DomainOrigin.MarketRoles).Take(5).ToList();
            var timeSeries = CreateInfinite(recipient, 19, DomainOrigin.TimeSeries).Take(5).ToList();

            await dataAvailableNotificationRepository
                .SaveAsync(new CabinetKey(aggregations[0]), aggregations)
                .ConfigureAwait(false);

            await dataAvailableNotificationRepository
                .SaveAsync(new CabinetKey(marketRoles[0]), marketRoles)
                .ConfigureAwait(false);

            await dataAvailableNotificationRepository
                .SaveAsync(new CabinetKey(timeSeries[0]), timeSeries)
                .ConfigureAwait(false);

            // Act
            var reader = await dataAvailableNotificationRepository
                .GetNextUnacknowledgedAsync(
                    recipient,
                    DomainOrigin.Aggregations,
                    DomainOrigin.MarketRoles,
                    DomainOrigin.TimeSeries)
                .ConfigureAwait(false);

            // Assert
            Assert.NotNull(reader);

            var items = await reader!
                .ReadToEndAsync()
                .ConfigureAwait(false);

            Assert.Equal(5, items.Count);

            for (var i = 0; i < items.Count; i++)
            {
                Assert.Equal(marketRoles[i].NotificationId, items[i].NotificationId);
                Assert.Equal(marketRoles[i].Recipient, items[i].Recipient);
                Assert.Equal(marketRoles[i].Origin, items[i].Origin);
                Assert.Equal(marketRoles[i].ContentType, items[i].ContentType);
                Assert.Equal(marketRoles[i].SupportsBundling, items[i].SupportsBundling);
                Assert.Equal(marketRoles[i].Weight, items[i].Weight);
                Assert.Equal(marketRoles[i].SequenceNumber, items[i].SequenceNumber);
            }
        }

        [Fact]
        public async Task GetNextUnacknowledgedAsync_NoData_ReturnsNull()
        {
            // Arrange
            await using var host = await MarketOperatorIntegrationTestHost.InitializeAsync().ConfigureAwait(false);
            var scope = host.BeginScope();

            var dataAvailableNotificationRepository = scope.GetInstance<IDataAvailableNotificationRepository>();

            var recipient = new MarketOperator(new MockedGln());
            var aggregations = CreateInfinite(recipient, 9, DomainOrigin.Aggregations).Take(5).ToList();
            var marketRoles = CreateInfinite(recipient, 1, DomainOrigin.MarketRoles).Take(5).ToList();

            await dataAvailableNotificationRepository
                .SaveAsync(new CabinetKey(aggregations[0]), aggregations)
                .ConfigureAwait(false);

            await dataAvailableNotificationRepository
                .SaveAsync(new CabinetKey(marketRoles[0]), marketRoles)
                .ConfigureAwait(false);

            // Act
            var reader = await dataAvailableNotificationRepository
                .GetNextUnacknowledgedAsync(recipient, DomainOrigin.TimeSeries)
                .ConfigureAwait(false);

            // Assert
            Assert.Null(reader);
        }

        [Fact]
        public async Task GetNextUnacknowledgedAsync_SequenceNumberTooHigh_ReturnsNull()
        {
            // Arrange
            await using var host = await MarketOperatorIntegrationTestHost.InitializeAsync().ConfigureAwait(false);
            var scope = host.BeginScope();

            var dataAvailableNotificationRepository = scope.GetInstance<IDataAvailableNotificationRepository>();

            var recipient = new MarketOperator(new MockedGln());
            var notifications = CreateInfinite(recipient, int.MaxValue * 2L, DomainOrigin.Aggregations)
                .Take(5)
                .ToList();

            await dataAvailableNotificationRepository
                .SaveAsync(new CabinetKey(notifications[0]), notifications)
                .ConfigureAwait(false);

            // Act
            var reader = await dataAvailableNotificationRepository
                .GetNextUnacknowledgedAsync(recipient, DomainOrigin.Aggregations)
                .ConfigureAwait(false);

            // Assert
            Assert.Null(reader);
        }

        [Fact]
        public async Task GetNextUnacknowledgedAsync_SequenceNumberTooHigh_ReturnsUpToNumber()
        {
            // Arrange
            await using var host = await MarketOperatorIntegrationTestHost.InitializeAsync().ConfigureAwait(false);
            var scope = host.BeginScope();

            var dataAvailableNotificationRepository = scope.GetInstance<IDataAvailableNotificationRepository>();

            var recipient = new MarketOperator(new MockedGln());
            var notifications = CreateInfinite(recipient, 1, DomainOrigin.Aggregations)
                .Take(5)
                .ToList();

            var forbidden = CreateInfinite(recipient, int.MaxValue * 2L, DomainOrigin.Aggregations)
                .Take(5)
                .ToList();

            await dataAvailableNotificationRepository
                .SaveAsync(new CabinetKey(notifications[0]), notifications)
                .ConfigureAwait(false);

            await dataAvailableNotificationRepository
                .SaveAsync(new CabinetKey(forbidden[0]), forbidden)
                .ConfigureAwait(false);

            // Act
            var reader = await dataAvailableNotificationRepository
                .GetNextUnacknowledgedAsync(recipient, DomainOrigin.Aggregations)
                .ConfigureAwait(false);

            // Assert
            Assert.NotNull(reader);

            var items = await reader!
                .ReadToEndAsync()
                .ConfigureAwait(false);

            Assert.Equal(5, items.Count);

            for (var i = 0; i < items.Count; i++)
            {
                Assert.Equal(notifications[i].NotificationId, items[i].NotificationId);
                Assert.Equal(notifications[i].Recipient, items[i].Recipient);
                Assert.Equal(notifications[i].Origin, items[i].Origin);
                Assert.Equal(notifications[i].ContentType, items[i].ContentType);
                Assert.Equal(notifications[i].SupportsBundling, items[i].SupportsBundling);
                Assert.Equal(notifications[i].Weight, items[i].Weight);
                Assert.Equal(notifications[i].SequenceNumber, items[i].SequenceNumber);
            }
        }

        [Fact]
        public async Task AcknowledgeAsync_OneItem_CanReadBackRest()
        {
            // Arrange
            await using var host = await MarketOperatorIntegrationTestHost.InitializeAsync().ConfigureAwait(false);
            var scope = host.BeginScope();

            var dataAvailableNotificationRepository = scope.GetInstance<IDataAvailableNotificationRepository>();
            var bundleRepository = scope.GetInstance<IBundleRepository>();

            var recipient = new MarketOperator(new MockedGln());
            var notifications = CreateInfinite(recipient, 1, DomainOrigin.Charges)
                .Take(5)
                .ToList();

            await dataAvailableNotificationRepository
                .SaveAsync(new CabinetKey(notifications[0]), notifications)
                .ConfigureAwait(false);

            var readForBundle = await dataAvailableNotificationRepository
                .GetNextUnacknowledgedAsync(recipient, DomainOrigin.Charges)
                .ConfigureAwait(false);

            await readForBundle!.TakeAsync().ConfigureAwait(false);

            var bundle = new Bundle(
                new Uuid(Guid.NewGuid()),
                notifications[0].Recipient,
                notifications[0].Origin,
                notifications[0].ContentType,
                new[] { notifications[0].NotificationId },
                Enumerable.Empty<string>());

            await bundleRepository
                .TryAddNextUnacknowledgedAsync(bundle, readForBundle)
                .ConfigureAwait(false);

            // Act
            await dataAvailableNotificationRepository
                .AcknowledgeAsync(bundle)
                .ConfigureAwait(false);

            // Assert
            var reader = await dataAvailableNotificationRepository
                .GetNextUnacknowledgedAsync(recipient, DomainOrigin.Charges)
                .ConfigureAwait(false);

            Assert.NotNull(reader);

            var items = await reader!
                .ReadToEndAsync()
                .ConfigureAwait(false);

            Assert.Equal(4, items.Count);

            for (var i = 0; i < items.Count; i++)
            {
                Assert.Equal(notifications[i + 1].NotificationId, items[i].NotificationId);
                Assert.Equal(notifications[i + 1].Recipient, items[i].Recipient);
                Assert.Equal(notifications[i + 1].Origin, items[i].Origin);
                Assert.Equal(notifications[i + 1].ContentType, items[i].ContentType);
                Assert.Equal(notifications[i + 1].SupportsBundling, items[i].SupportsBundling);
                Assert.Equal(notifications[i + 1].Weight, items[i].Weight);
                Assert.Equal(notifications[i + 1].SequenceNumber, items[i].SequenceNumber);
            }
        }

        [Fact]
        public async Task AcknowledgeAsync_AllItems_CannotReadBack()
        {
            // Arrange
            await using var host = await MarketOperatorIntegrationTestHost.InitializeAsync().ConfigureAwait(false);
            var scope = host.BeginScope();

            var dataAvailableNotificationRepository = scope.GetInstance<IDataAvailableNotificationRepository>();
            var bundleRepository = scope.GetInstance<IBundleRepository>();

            var recipient = new MarketOperator(new MockedGln());
            var notifications = CreateInfinite(recipient, 1, DomainOrigin.Charges)
                .Take(5)
                .ToList();

            await dataAvailableNotificationRepository
                .SaveAsync(new CabinetKey(notifications[0]), notifications)
                .ConfigureAwait(false);

            var readForBundle = await dataAvailableNotificationRepository
                .GetNextUnacknowledgedAsync(recipient, DomainOrigin.Charges)
                .ConfigureAwait(false);

            await readForBundle!.ReadToEndAsync().ConfigureAwait(false);

            var bundle = new Bundle(
                new Uuid(Guid.NewGuid()),
                notifications[0].Recipient,
                notifications[0].Origin,
                notifications[0].ContentType,
                new[] { notifications[0].NotificationId },
                Enumerable.Empty<string>());

            await bundleRepository
                .TryAddNextUnacknowledgedAsync(bundle, readForBundle!)
                .ConfigureAwait(false);

            // Act
            await dataAvailableNotificationRepository
                .AcknowledgeAsync(bundle)
                .ConfigureAwait(false);

            // Assert
            var reader = await dataAvailableNotificationRepository
                .GetNextUnacknowledgedAsync(recipient, DomainOrigin.Charges)
                .ConfigureAwait(false);

            Assert.Null(reader);
        }

        private static IEnumerable<DataAvailableNotification> CreateInfinite(
            MarketOperator recipient,
            long initialSequenceNumber,
            DomainOrigin domainOrigin = DomainOrigin.Charges,
            string contentType = "default_content_type",
            bool supportsBundling = true,
            int weight = 1)
        {
            while (true)
            {
                yield return new DataAvailableNotification(
                    new Uuid(Guid.NewGuid()),
                    recipient,
                    new ContentType(contentType),
                    domainOrigin,
                    new SupportsBundling(supportsBundling),
                    new Weight(weight),
                    new SequenceNumber(initialSequenceNumber++),
                    new DocumentType("RSM??"));
            }
        }
    }
}
