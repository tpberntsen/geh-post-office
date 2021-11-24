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

using Energinet.DataHub.MessageHub.Model.Dequeue;
using Energinet.DataHub.MessageHub.Model.Exceptions;
using Energinet.DataHub.MessageHub.Model.Model;
using Energinet.DataHub.MessageHub.Model.Protobuf;
using Google.Protobuf;
using Xunit;
using Xunit.Categories;

namespace Energinet.DataHub.MessageHub.Model.Tests.Dequeue
{
    [UnitTest]
    public class RequestBundleParserTests
    {
        [Fact]
        public void Parse_BytesValid_Returns_Valid_Object()
        {
            // arrange
            var target = new DequeueNotificationParser();
            var validBytes = new DequeueContract
            {
                MarketOperator = "06FD1AB3-D650-45BC-860E-EE598A3623CA",
                DataAvailableNotificationReferenceId = "7946F84C-FE27-43F3-B3D4-EE59CBA2F82C"
            }.ToByteArray();

            // act
            var actual = target.Parse(validBytes);

            // assert
            Assert.NotNull(actual);
            Assert.Equal("06FD1AB3-D650-45BC-860E-EE598A3623CA", actual.MarketOperator.Value);
            Assert.Equal("7946F84C-FE27-43F3-B3D4-EE59CBA2F82C", actual.DataAvailableNotificationReferenceId);
        }

        [Fact]
        public void Parse_BytesInvalid_ThrowsException()
        {
            // arrange
            var target = new DequeueNotificationParser();
            var corruptBytes = new byte[] { 1, 2, 3 };

            // act, assert
            Assert.Throws<MessageHubException>(() => target.Parse(corruptBytes));
        }

        [Fact]
        public void Parse_ValidObject_Returns_Bytes()
        {
            // arrange
            var target = new DequeueNotificationParser();
            var valid = new DequeueNotificationDto(
                "F8201E4D-8989-4B75-A2C2-1E163DA7660B",
                new GlobalLocationNumberDto("test"));

            // act
            var actual = target.Parse(valid);

            // assert
            Assert.NotNull(actual);
        }
    }
}
