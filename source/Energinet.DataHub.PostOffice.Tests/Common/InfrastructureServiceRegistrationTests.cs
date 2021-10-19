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

using Energinet.DataHub.MessageHub.Client.Factories;
using Energinet.DataHub.PostOffice.Common;
using Microsoft.Extensions.Configuration;
using Moq;
using SimpleInjector;
using SimpleInjector.Lifestyles;
using Xunit;
using Xunit.Categories;

namespace Energinet.DataHub.PostOffice.Tests.Common
{
    [UnitTest]
    public class InfrastructureServiceRegistrationTests
    {
        [Theory]
        [InlineData("")]
        [InlineData(null)]
        [InlineData(" ")]
        public void AddInfrastructureCalled_ConnectionStringInvalid_ThrowsWhenResolvingStorageServiceClientFactory(string connectionString)
        {
            // arrange
            var configMock = new Mock<IConfiguration>();
            configMock.Setup(x => x["BlobStorageConnectionString"]).Returns(connectionString);

            using var container = new Container();
            container.Options.EnableAutoVerification = false;
            container.Options.DefaultScopedLifestyle = new AsyncScopedLifestyle();
            container.RegisterSingleton(() => configMock.Object);

            // act
            container.AddInfrastructureServices();

            // assert
            Assert.Throws<ActivationException>(() => container.GetInstance<IStorageServiceClientFactory>());
        }

        [Fact]
        public void AddInfrastructureCalled_ConnectionStringValid_CanResolveStorageServiceClientFactory()
        {
            // arrange
            var configMock = new Mock<IConfiguration>();
            configMock.Setup(x => x["BlobStorageConnectionString"]).Returns("connectionString");

            using var container = new Container();
            container.Options.EnableAutoVerification = false;
            container.Options.DefaultScopedLifestyle = new AsyncScopedLifestyle();
            container.RegisterSingleton(() => configMock.Object);

            // act
            container.AddInfrastructureServices();
            var factory = container.GetInstance<IStorageServiceClientFactory>();

            // assert
            Assert.NotNull(factory);
        }
    }
}
