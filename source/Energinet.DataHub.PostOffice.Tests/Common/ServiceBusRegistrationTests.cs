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

using System.Threading.Tasks;
using Energinet.DataHub.MessageHub.Core.Factories;
using Energinet.DataHub.PostOffice.Common;
using Energinet.DataHub.PostOffice.Infrastructure;
using Microsoft.Extensions.Configuration;
using Moq;
using SimpleInjector;
using Xunit;
using Xunit.Categories;

namespace Energinet.DataHub.PostOffice.Tests.Common
{
    [UnitTest]
    public class ServiceBusRegistrationTests
    {
        [Fact]
        public async Task AddServiceBus_AllGood_FactroyIsRegistered()
        {
            // arrange
            var configMock = new Mock<IConfiguration>();
            configMock.Setup(x => x["ServiceBusConnectionString"]).Returns(
                "Endpoint=sb://test.servicebus.windows.net/;SharedAccessKeyName=test;SharedAccessKey=test");

            await using var container = new Container();
            container.Register(() => configMock.Object, Lifestyle.Singleton);

            // act
            container.AddServiceBus();
            var actual = container.GetInstance<IServiceBusClientFactory>();

            // assert
            Assert.NotNull(actual);
        }

        [Fact]
        public async Task AddServiceBus_NoConnectionString_Throws()
        {
            // arrange
            var configMock = new Mock<IConfiguration>();
            configMock.Setup(x => x["ServiceBusConnectionString"]).Returns(string.Empty);

            await using var container = new Container();
            container.Register(() => configMock.Object, Lifestyle.Singleton);

            // act
            container.AddServiceBus();

            // assert
            Assert.Throws<ActivationException>(() => container.GetInstance<IServiceBusClientFactory>());
        }

        [Fact]
        public async Task AddServiceBusConfig_AllGood_ServiceBusConfigIsRegistered()
        {
            // arrange
            const string expectedQueueName = "fake_queue_name";
            const string expectedConnectionString = "fake_connection_string";
            const string expectedDequeueCleanUpQueueName = "fake_queue_name";

            var queueNameSectionMock = new Mock<IConfigurationSection>();
            queueNameSectionMock.Setup(x => x.Value).Returns(expectedQueueName);

            var connectionStringSectionMock = new Mock<IConfigurationSection>();
            connectionStringSectionMock.Setup(x => x.Value).Returns(expectedConnectionString);

            var dequeueCleanUpQueueName = new Mock<IConfigurationSection>();
            dequeueCleanUpQueueName.Setup(x => x.Value).Returns(expectedDequeueCleanUpQueueName);

            var configMock = new Mock<IConfiguration>();
            configMock.Setup(x => x.GetSection(ServiceBusConfig.DataAvailableQueueNameKey)).Returns(queueNameSectionMock.Object);
            configMock.Setup(x => x.GetSection(ServiceBusConfig.DataAvailableQueueConnectionStringKey)).Returns(connectionStringSectionMock.Object);
            configMock.Setup(x => x.GetSection(ServiceBusConfig.DequeueCleanUpQueueNameKey)).Returns(dequeueCleanUpQueueName.Object);

            await using var container = new Container();
            container.Register(() => configMock.Object, Lifestyle.Singleton);

            // act
            container.AddServiceBusConfig();
            var actual = container.GetInstance<ServiceBusConfig>();

            // assert
            Assert.NotNull(actual);
            Assert.Equal(expectedQueueName, actual.DataAvailableQueueName);
            Assert.Equal(expectedConnectionString, actual.DataAvailableQueueConnectionString);
        }
    }
}
