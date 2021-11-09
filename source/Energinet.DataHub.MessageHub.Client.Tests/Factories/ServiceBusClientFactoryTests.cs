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

using Azure.Messaging.ServiceBus;
using Energinet.DataHub.MessageHub.Client.Factories;
using Xunit;

namespace Energinet.DataHub.MessageHub.Client.Tests.Factories
{
    public sealed class ServiceBusClientFactoryTests
    {
        [Fact]
        public void Create_ReturnsServiceBusClient()
        {
            // arrange
            var target = new ServiceBusClientFactory("Endpoint=sb://sbn-postoffice.servicebus.windows.net/;SharedAccessKeyName=Hello;SharedAccessKey=there");

            // act
            var actual = target.Create();

            // assert
            Assert.IsType<ServiceBusClient>(actual);
            Assert.NotNull(actual);
        }

        [Fact]
        public void Create_ReturnsSingleton()
        {
            // arrange
            var target = new ServiceBusClientFactory("Endpoint=sb://sbn-postoffice.servicebus.windows.net/;SharedAccessKeyName=Hello;SharedAccessKey=there");

            // act
            var actualA = target.Create();
            var actualB = target.Create();

            // assert
            Assert.NotNull(actualA);
            Assert.Equal(actualA, actualB);
        }
    }
}
