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
using Energinet.DataHub.MessageHub.Client.Exceptions;
using Energinet.DataHub.MessageHub.Client.Model;
using Energinet.DataHub.MessageHub.Client.Peek;
using Energinet.DataHub.MessageHub.Client.Protobuf;
using Google.Protobuf;
using Xunit;
using Xunit.Categories;

namespace PostOffice.Communicator.Tests.Peek
{
    [UnitTest]
    public class ResponseBundleParserTests
    {
        [Fact]
        public void Parse_BytesValid_Returns_NotNull()
        {
            // arrange
            var target = new ResponseBundleParser();
            var validBytes = new DataBundleResponseContract
            {
                Success = new DataBundleResponseContract.Types.FileResource
                {
                    ContentUri = "http://localhost",
                    DataAvailableNotificationIds = { new[] { "B34E47BC-21EA-40C5-AE27-A5900F42D7C6" } }
                }
            }.ToByteArray();

            // act
            var actual = target.Parse(validBytes);

            // assert
            Assert.NotNull(actual);
            Assert.Equal(new[] { new Guid("B34E47BC-21EA-40C5-AE27-A5900F42D7C6") }, actual.DataAvailableNotificationIds);
        }

        [Fact]
        public void Parse_BytesValidWithFailedRequestStatus_ReturnsNull()
        {
            // arrange
            var target = new ResponseBundleParser();
            var validBytes = new DataBundleResponseContract
            {
                Failure = new DataBundleResponseContract.Types.RequestFailure
                {
                    Reason = DataBundleResponseContract.Types.RequestFailure.Types.Reason.InternalError
                }
            }.ToByteArray();

            // act
            var actual = target.Parse(validBytes);

            // assert
            Assert.Null(actual);
        }

        [Fact]
        public void Parse_BytesCorrupt_Throws_Exception()
        {
            // arrange
            var target = new ResponseBundleParser();
            var corruptBytes = new byte[] { 1, 2, 3 };

            // act, assert
            Assert.Throws<PostOfficeCommunicatorException>(() => target.Parse(corruptBytes));
        }

        [Fact]
        public void Parse_ValidObject_Returns_Bytes()
        {
            // arrange
            var target = new ResponseBundleParser();
            var valid = new RequestDataBundleResponseDto(
                new Uri("https://test.test.dk"),
                new[] { Guid.NewGuid(), Guid.NewGuid() });

            // act
            var actual = target.Parse(valid);

            // assert
            Assert.NotNull(actual);
        }

        [Fact]
        public void Parse_ValidError_ReturnsBytes()
        {
            // arrange
            var target = new ResponseBundleParser();
            var valid = new RequestDataBundleResponseDto(
                new DataBundleResponseErrorDto
                {
                    FailureDescription = "error",
                    Reason = DataBundleResponseErrorReason.DatasetNotAvailable
                },
                new[] { Guid.NewGuid(), Guid.NewGuid() });

            // act
            var actual = target.Parse(valid);

            // assert
            Assert.NotNull(actual);
        }
    }
}
