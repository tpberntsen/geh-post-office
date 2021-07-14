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
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Azure.Messaging.ServiceBus;
using Energinet.DataHub.PostOffice.Application;
using Energinet.DataHub.PostOffice.Application.DataAvailable;
using Energinet.DataHub.PostOffice.Application.GetMessage.Interfaces;
using Energinet.DataHub.PostOffice.Application.Validation;
using Energinet.DataHub.PostOffice.Common;
using Energinet.DataHub.PostOffice.Common.MediatR;
using Energinet.DataHub.PostOffice.Inbound.Parsing;
using Energinet.DataHub.PostOffice.Infrastructure;
using Energinet.DataHub.PostOffice.Infrastructure.ContentPath;
using Energinet.DataHub.PostOffice.Infrastructure.GetMessage;
using Energinet.DataHub.PostOffice.Infrastructure.Mappers;
using Energinet.DataHub.PostOffice.Infrastructure.Pipeline;
using Energinet.DataHub.PostOffice.IntegrationTests.DataAvailable;
using FluentValidation;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Cosmos.Fluent;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using SimpleInjector;
using SimpleInjector.Integration.ServiceCollection;
using SimpleInjector.Lifestyles;
using Xunit;
using Xunit.Categories;
using Container = SimpleInjector.Container;

namespace Energinet.DataHub.PostOffice.IntegrationTests
{
    [Collection("IntegrationTest")]
    [IntegrationTest]
    public class IntegrationTestHost : IAsyncDisposable
    {
        private readonly Scope _scope;
        private readonly Container _container;
        private readonly IServiceProvider _serviceProvider;
        private bool _disposed;

        protected IntegrationTestHost()
        {
            _container = new Container();
            var serviceCollection = new ServiceCollection();

            serviceCollection.AddSimpleInjector(_container, options => options.DisposeContainerWithServiceProvider = false);
            _serviceProvider = serviceCollection.BuildServiceProvider().UseSimpleInjector(_container);

            _container.Register<IValidator<DataAvailableCommand>, DataAvailableRuleSet>(Lifestyle.Scoped);

            // Add Custom Services
            _container.Register<IMapper<Contracts.DataAvailable, DataAvailableCommand>, DataAvailableMapper>(Lifestyle.Scoped);
            _container.Register<IDocumentStore<Domain.DataAvailable>, CosmosDataAvailableStore>(Lifestyle.Scoped);
            _container.Register<IDataAvailableStorageService, DataAvailableStorageService>(Lifestyle.Scoped);
            _container.Register<IDataAvailableController, DataAvailableController>(Lifestyle.Scoped);
            _container.Register<IMessageResponseStorage, MessageResponseStorage>(Lifestyle.Scoped);
            _container.Register<DataAvailableContractParser>(Lifestyle.Scoped);
            _container.Register<ISendMessageToServiceBus, SendMessageToServiceBus>(Lifestyle.Scoped);
            _container.Register<IGetPathToDataFromServiceBus, GetPathToDataFromServiceBus>(Lifestyle.Scoped);
            _container.Register<IStorageService, StorageService>(Lifestyle.Scoped);

            _container.Collection.Register<IGetContentPathStrategy>(typeof(ContentPathFromSubDomain), typeof(ContentPathFromSavedResponse));
            _container.Register<IGetContentPathStrategyFactory, GetContentPathStrategyFactory>();

            _container.Register<ServiceBusClient>(() => new ServiceBusClient("Endpoint=sb://test.windows.net/;SharedAccessKeyName=test;SharedAccessKey=test"), Lifestyle.Singleton);
            _container.Register<CosmosDatabaseConfig>(() => new CosmosDatabaseConfig("CHANGE_NAME"), Lifestyle.Singleton);
            serviceCollection.AddCosmosClientBuilder(useBulkExecution: false);

            // TODO Change to use real implementation for integration test
            _container.Register(BuildMoqCosmosClient, Lifestyle.Scoped);

            serviceCollection.AddScoped<IValidator<DataAvailableCommand>, DataAvailableRuleSet>();

            _container.BuildMediator(
                new[]
                {
                    typeof(Domain.DataAvailable).Assembly,
                    typeof(DataAvailableHandler).Assembly,
                    typeof(CosmosClientBuilder).Assembly,
                },
                new[] { typeof(DataAvailablePipelineValidationBehavior<,>) });

            _container.Verify();

            _scope = AsyncScopedLifestyle.BeginScope(_container);
        }

        public async ValueTask DisposeAsync()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
            await _container.DisposeContainerAsync().ConfigureAwait(false);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (_disposed == true)
            {
                return;
            }

            _scope.Dispose();
            ((ServiceProvider)_serviceProvider).Dispose();
            _container.Dispose();
            _disposed = true;
        }

        protected TService GetService<TService>()
            where TService : class
        {
            return _container.GetInstance<TService>();
        }

        /// <summary>
        /// We test cosmos with Mock implementation. Should be change later to test against a real Cosmos DB
        /// </summary>
        private static CosmosClient BuildMoqCosmosClient()
        {
            var mockItemResponse = new Mock<ItemResponse<Domain.DataAvailable>>();
            mockItemResponse.Setup(x => x.StatusCode)
                .Returns(HttpStatusCode.Created);

            var mockContainer = new Mock<Microsoft.Azure.Cosmos.Container>();
            mockContainer
                .Setup(e => e.CreateItemAsync<Domain.DataAvailable>(
                    It.IsAny<Domain.DataAvailable>(),
                    null,
                    null,
                    default(CancellationToken)))
                .ReturnsAsync(mockItemResponse.Object);

            return new CosmosClientWrapper(
                "AccountEndpoint=https://localhost:8081/;AccountKey=1234",
                mockContainer);
        }
    }
}
