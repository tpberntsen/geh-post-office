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
using Google.Protobuf;
using GreenEnergyHub.PostOffice.Communicator.Contracts;
using GreenEnergyHub.PostOffice.Communicator.Dequeue;
using GreenEnergyHub.PostOffice.Communicator.Exceptions;
using Xunit;
using Xunit.Categories;

namespace PostOffice.Communicator.Tests.Dequeue
{
    [UnitTest]
    public class RequestBundleParserTests
    {
        [Fact]
        public void TryParse_BytesValid_Returns_Valid_Object()
        {
            // arrange
            var target = new DequeueNotificationParser();
            var validBytes = new DequeueContract()
            {
                Recipient = "06FD1AB3-D650-45BC-860E-EE598A3623CA",
                DataAvailableIds = { "1360036D-2AFB-4021-846E-2C3FF5AD8DBD" }
            }.ToByteArray();

            // act
            var actual = target.Parse(validBytes);

            // assert
            Assert.NotNull(actual);
            Assert.Equal("06FD1AB3-D650-45BC-860E-EE598A3623CA", actual.Recipient);
        }

        [Fact]
        public void TryParse_BytesInvalidValid_Throws_Exception()
        {
            // arrange
            var target = new DequeueNotificationParser();
            var rnd = new Random();
            var corruptBytes = new byte[10];
            rnd.NextBytes(corruptBytes);

            // act, assert
            Assert.Throws<PostOfficeCommunicatorException>(() => target.Parse(corruptBytes));
        }
    }
}
