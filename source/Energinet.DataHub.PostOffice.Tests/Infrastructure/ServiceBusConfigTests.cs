// // Copyright 2020 Energinet DataHub A/S
// //
// // Licensed under the Apache License, Version 2.0 (the "License2");
// // you may not use this file except in compliance with the License.
// // You may obtain a copy of the License at
// //
// //     http://www.apache.org/licenses/LICENSE-2.0
// //
// // Unless required by applicable law or agreed to in writing, software
// // distributed under the License is distributed on an "AS IS" BASIS,
// // WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// // See the License for the specific language governing permissions and
// // limitations under the License.

using System;
using Energinet.DataHub.PostOffice.Infrastructure;
using Xunit;
using Xunit.Categories;

namespace Energinet.DataHub.PostOffice.Tests.Infrastructure
{
    [UnitTest]
    public class ServiceBusConfigTests
    {
        [Fact]
        public void Ctor_ParamsNotNull_SetsProperties()
        {
            // arrange
            const string queueName = "queueName";
            const string connectionString = "connectionString";

            // act
            var actual = new ServiceBusConfig(queueName,  connectionString);

            // assert
            Assert.Equal(queueName, actual.DataAvailableQueueName);
            Assert.Equal(connectionString, actual.DataAvailableQueueConnectionString);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("     ")]
        public void Ctor_QueueNameNullOrWhitespace_Throws(string value)
        {
            // arrange, act, assert
            Assert.Throws<ArgumentException>(() => new ServiceBusConfig(value, "b"));
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("     ")]
        public void Ctor_ConnectionStringNullOrWhitespace_Throws(string value)
        {
            // arrange, act, assert
            Assert.Throws<ArgumentException>(() => new ServiceBusConfig("a", value));
        }
    }
}
