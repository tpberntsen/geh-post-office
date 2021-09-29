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
using Energinet.DataHub.PostOffice.Application;
using Energinet.DataHub.PostOffice.Common.Extensions;
using Energinet.DataHub.PostOffice.Common.MediatR;
using Energinet.DataHub.PostOffice.Common.SimpleInjector;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using SimpleInjector;

namespace Energinet.DataHub.PostOffice.Common
{
    public abstract class StartupBase : IAsyncDisposable
    {
        protected StartupBase()
        {
            Container = new Container();
        }

        public Container Container { get; }

        public async ValueTask DisposeAsync()
        {
            await DisposeAsyncCore().ConfigureAwait(false);
            GC.SuppressFinalize(this);
        }

        public void ConfigureServices(IServiceCollection services)
        {
            SwitchToSimpleInjector(services);

            // Configuration
            services.AddDatabaseCosmosConfig();
            services.AddServiceBusConfig();
            services.AddCosmosClientBuilder(false);
            services.AddServiceBus();

            // FluentValidation
            services.DiscoverValidation(new[] { typeof(ApplicationAssemblyReference).Assembly });

            Configure(services);

            services.AddLogging();
            services.AddSimpleInjector(Container, x => x.DisposeContainerWithServiceProvider = !true);

            Container.AddRepositories();
            Container.AddDomainServices();
            Container.AddApplicationServices();
            Container.AddInfrastructureServices();

            // Add MediatR
            Container.BuildMediator(new[] { typeof(ApplicationAssemblyReference).Assembly });

            Configure(Container);
        }

        // Recommended convention is DisposeAsyncCore, Core being last.
#pragma warning disable VSTHRD200
        protected virtual ValueTask DisposeAsyncCore()
#pragma warning restore VSTHRD200
        {
            return Container.DisposeAsync();
        }

        protected abstract void Configure(Container container);
        protected abstract void Configure(IServiceCollection serviceCollection);

        private static void SwitchToSimpleInjector(IServiceCollection services)
        {
            var descriptor = new ServiceDescriptor(
                typeof(IFunctionActivator),
                typeof(SimpleInjectorActivator),
                ServiceLifetime.Singleton);

            services.Replace(descriptor);
        }
    }
}
