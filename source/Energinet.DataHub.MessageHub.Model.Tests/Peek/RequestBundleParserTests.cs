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
using Energinet.DataHub.MessageHub.Model.Exceptions;
using Energinet.DataHub.MessageHub.Model.Peek;
using Energinet.DataHub.MessageHub.Model.Protobuf;
using Google.Protobuf;
using Xunit;
using Xunit.Categories;

namespace Energinet.DataHub.MessageHub.Model.Tests.Peek
{
    [UnitTest]
    public class RequestBundleParserTests
    {
        [Fact]
        public void Parse_BytesValid_ReturnsValidObject()
        {
            // arrange
            var target = new RequestBundleParser();
            var validBytes = new DataBundleRequestContract
            {
                RequestId = "07814976-6567-4E43-8C31-26630FEA3671",
                DataAvailableNotificationReferenceId = "2139E118-96D9-4D68-9094-44C15D8CEACF",
                IdempotencyId = "06FD1AB3-D650-45BC-860E-EE598A3623CA",
                MessageType = "some_message_type"
            }.ToByteArray();

            // act
            var actual = target.Parse(validBytes);

            // assert
            Assert.NotNull(actual);
            Assert.Equal(Guid.Parse("07814976-6567-4E43-8C31-26630FEA3671"), actual.RequestId);
            Assert.Equal("2139E118-96D9-4D68-9094-44C15D8CEACF", actual.DataAvailableNotificationReferenceId);
            Assert.Equal("06FD1AB3-D650-45BC-860E-EE598A3623CA", actual.IdempotencyId);
            Assert.Equal("some_message_type", actual.MessageType);
        }

        [Fact]
        public void Parse_BytesInvalid_ThrowsException()
        {
            // arrange
            var target = new RequestBundleParser();
            var corruptBytes = new byte[] { 1, 2, 3 };

            // act, assert
            Assert.Throws<MessageHubException>(() => target.Parse(corruptBytes));
        }

        [Fact]
        public void Parse_GuidInvalid_ThrowsException()
        {
            // arrange
            var target = new RequestBundleParser();
            var contract = new DataBundleRequestContract
            {
                RequestId = "invalid_guid",
                DataAvailableNotificationReferenceId = "2139E118-96D9-4D68-9094-44C15D8CEACF",
                IdempotencyId = "06FD1AB3-D650-45BC-860E-EE598A3623CA",
                MessageType = "some_message_type"
            };

            // act, assert
            Assert.Throws<MessageHubException>(() => target.Parse(contract.ToByteArray()));
        }
    }
}
