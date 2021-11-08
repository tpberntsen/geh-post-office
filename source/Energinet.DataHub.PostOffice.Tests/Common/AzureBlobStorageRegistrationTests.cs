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

using Energinet.DataHub.MessageHub.Core;
using Energinet.DataHub.MessageHub.Core.Factories;
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
    public sealed class AzureBlobStorageRegistrationTests
    {
        [Theory]
        [InlineData("")]
        [InlineData(null)]
        [InlineData(" ")]
        public void AddAzureBlobStorage_ConnectionStringInvalid_ThrowsWhenResolvingStorageServiceClientFactory(string connectionString)
        {
            // arrange
            var configMock = new Mock<IConfiguration>();
            configMock.Setup(x => x["BlobStorageConnectionString"]).Returns(connectionString);

            using var container = new Container();
            container.Options.EnableAutoVerification = false;
            container.Options.DefaultScopedLifestyle = new AsyncScopedLifestyle();
            container.RegisterSingleton(() => configMock.Object);

            // act
            container.AddAzureBlobStorage();

            // assert
            Assert.Throws<ActivationException>(() => container.GetInstance<IStorageServiceClientFactory>());
        }

        [Fact]
        public void AddAzureBlobStorage_ConnectionStringValid_CanResolveStorageServiceClientFactory()
        {
            // arrange
            var configMock = new Mock<IConfiguration>();
            configMock.Setup(x => x["BlobStorageConnectionString"]).Returns("connectionString");

            using var container = new Container();
            container.Options.EnableAutoVerification = false;
            container.Options.DefaultScopedLifestyle = new AsyncScopedLifestyle();
            container.RegisterSingleton(() => configMock.Object);

            // act
            container.AddAzureBlobStorage();
            var factory = container.GetInstance<IStorageServiceClientFactory>();

            // assert
            Assert.NotNull(factory);
        }

        [Theory]
        [InlineData("")]
        [InlineData(null)]
        [InlineData(" ")]
        public void AddAzureBlobStorageConfig_ContainerNameInvalid_ThrowsWhenResolvingStorageConfig(string containerName)
        {
            // arrange
            var configMock = new Mock<IConfiguration>();
            var sectionMock = new Mock<IConfigurationSection>();
            sectionMock.Setup(x => x.Value).Returns(containerName);
            configMock.Setup(x => x.GetSection("BlobStorageContainerName")).Returns(sectionMock.Object);

            using var container = new Container();
            container.Options.EnableAutoVerification = false;
            container.Options.DefaultScopedLifestyle = new AsyncScopedLifestyle();
            container.RegisterSingleton(() => configMock.Object);

            // act
            container.AddAzureBlobStorageConfig();

            // assert
            Assert.Throws<ActivationException>(() => container.GetInstance<StorageConfig>());
        }

        [Fact]
        public void AddAzureBlobStorageConfig_ContainerNameInvalid_CanResolveStorageConfig()
        {
            // arrange
            var configMock = new Mock<IConfiguration>();
            var sectionMock = new Mock<IConfigurationSection>();
            sectionMock.Setup(x => x.Value).Returns("post-office-container-name");
            configMock.Setup(x => x.GetSection("BlobStorageContainerName")).Returns(sectionMock.Object);

            using var container = new Container();
            container.Options.EnableAutoVerification = false;
            container.Options.DefaultScopedLifestyle = new AsyncScopedLifestyle();
            container.RegisterSingleton(() => configMock.Object);

            // act
            container.AddAzureBlobStorageConfig();
            var config = container.GetInstance<StorageConfig>();

            // assert
            Assert.NotNull(config);
        }
    }
}
