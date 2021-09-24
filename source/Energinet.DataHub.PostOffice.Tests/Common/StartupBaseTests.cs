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
using Azure.Messaging.ServiceBus;
using Energinet.DataHub.PostOffice.Common;
using Energinet.DataHub.PostOffice.Infrastructure;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Moq;
using SimpleInjector;
using Xunit;
using Xunit.Categories;
using Container = SimpleInjector.Container;

namespace Energinet.DataHub.PostOffice.Tests.Common
{
    [UnitTest]
    public sealed class StartupBaseTests
    {
        [Fact]
        public async Task Startup_ConfigureServices_ShouldVerify()
        {
            // Arrange
            var serviceCollection = new ServiceCollection();
            await using var target = new TestOfStartupBase();

            // Act
            target.ConfigureServices(serviceCollection);
            await using var serviceProvider = serviceCollection.BuildServiceProvider();
            serviceProvider.UseSimpleInjector(target.Container);

            // Assert
            target.Container.Verify();
        }

        [Fact]
        public async Task Startup_ConfigureServices_ShouldCallConfigureContainer()
        {
            // Arrange
            var serviceCollection = new ServiceCollection();
            var configureContainerMock = new Mock<Action>();
            await using var target = new TestOfStartupBase { ConfigureContainer = configureContainerMock.Object };

            // Act
            target.ConfigureServices(serviceCollection);

            // Assert
            configureContainerMock.Verify(x => x(), Times.Once);
        }

        [Fact]
        public async Task Startup_ConfigureServices_ShouldCallConfigureServiceCollection()
        {
            // Arrange
            var serviceCollection = new ServiceCollection();
            var configureServiceCollectionMock = new Mock<Action>();
            await using var target = new TestOfStartupBase { ConfigureServiceCollection = configureServiceCollectionMock.Object };

            // Act
            target.ConfigureServices(serviceCollection);

            // Assert
            configureServiceCollectionMock.Verify(x => x(), Times.Once);
        }

        private sealed class TestOfStartupBase : StartupBase
        {
            public Action? ConfigureContainer { get; init; }
            public Action? ConfigureServiceCollection { get; init; }

            protected override void Configure(Container container)
            {
                ConfigureContainer?.Invoke();
            }

            protected override void Configure(IServiceCollection serviceCollection)
            {
                AddMockConfiguration(serviceCollection);
                ConfigureServiceCollection?.Invoke();
            }

            private static void AddMockConfiguration(IServiceCollection serviceCollection)
            {
                serviceCollection.Replace(
                   new ServiceDescriptor(
                       typeof(ServiceBusClient),
                       _ => new MockedServiceBusClient(),
                       ServiceLifetime.Singleton));

                serviceCollection.Replace(
                    new ServiceDescriptor(
                        typeof(CosmosClient),
                        _ => new MockedCosmosClient(),
                        ServiceLifetime.Singleton));

                serviceCollection.Replace(
                    new ServiceDescriptor(
                        typeof(ServiceBusConfig),
                        _ => new ServiceBusConfig("fake_value", "fake_value"),
                        ServiceLifetime.Singleton));

                serviceCollection.Replace(
                    new ServiceDescriptor(
                        typeof(CosmosDatabaseConfig),
                        _ => new CosmosDatabaseConfig("fake_value"),
                        ServiceLifetime.Singleton));
            }
        }
    }
}
