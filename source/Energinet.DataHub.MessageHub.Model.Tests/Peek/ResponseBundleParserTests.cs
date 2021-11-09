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
using Energinet.DataHub.MessageHub.Model.Model;
using Energinet.DataHub.MessageHub.Model.Peek;
using Energinet.DataHub.MessageHub.Model.Protobuf;
using Google.Protobuf;
using Xunit;
using Xunit.Categories;

namespace Energinet.DataHub.MessageHub.Model.Tests.Peek
{
    [UnitTest]
    public class ResponseBundleParserTests
    {
        [Fact]
        public void Parse_BytesValid_ReturnsSuccess()
        {
            // arrange
            var target = new ResponseBundleParser();
            var validBytes = new DataBundleResponseContract
            {
                RequestId = "636A3749-E3F8-4733-BB4C-027AF591B485",
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
            Assert.Equal(Guid.Parse("636A3749-E3F8-4733-BB4C-027AF591B485"), actual.RequestId);
            Assert.Equal(new Uri("http://localhost"), actual.ContentUri);
            Assert.Equal(new[] { new Guid("B34E47BC-21EA-40C5-AE27-A5900F42D7C6") }, actual.DataAvailableNotificationIds);
        }

        [Fact]
        public void Parse_BytesValidWithFailedRequestStatus_ReturnsFailure()
        {
            // arrange
            var target = new ResponseBundleParser();
            var validBytes = new DataBundleResponseContract
            {
                RequestId = "E3246CD7-CD9B-4FC3-851D-ED0828396030",
                Failure = new DataBundleResponseContract.Types.RequestFailure
                {
                    Reason = DataBundleResponseContract.Types.RequestFailure.Types.Reason.InternalError,
                    FailureDescription = "test_description"
                }
            }.ToByteArray();

            // act
            var actual = target.Parse(validBytes);

            // assert
            Assert.NotNull(actual);
            Assert.Equal(Guid.Parse("E3246CD7-CD9B-4FC3-851D-ED0828396030"), actual.RequestId);
            Assert.NotNull(actual.ResponseError);
            Assert.Equal(DataBundleResponseErrorReason.InternalError, actual.ResponseError.Reason);
            Assert.Equal("test_description", actual.ResponseError.FailureDescription);
        }

        [Fact]
        public void Parse_BytesCorrupt_ThrowsException()
        {
            // arrange
            var target = new ResponseBundleParser();
            var corruptBytes = new byte[] { 1, 2, 3 };

            // act, assert
            Assert.Throws<MessageHubException>(() => target.Parse(corruptBytes));
        }

        [Fact]
        public void ParseSuccess_GuidCorrupt_ThrowsException()
        {
            // arrange
            var target = new ResponseBundleParser();
            var contract = new DataBundleResponseContract
            {
                RequestId = "invalid_guid",
                Success = new DataBundleResponseContract.Types.FileResource
                {
                    ContentUri = "http://localhost",
                    DataAvailableNotificationIds = { new[] { "B34E47BC-21EA-40C5-AE27-A5900F42D7C6" } }
                }
            };

            // act, assert
            Assert.Throws<MessageHubException>(() => target.Parse(contract.ToByteArray()));
        }

        [Fact]
        public void ParseFailure_GuidCorrupt_ThrowsException()
        {
            // arrange
            var target = new ResponseBundleParser();
            var contract = new DataBundleResponseContract
            {
                RequestId = "invalid_guid",
                Failure = new DataBundleResponseContract.Types.RequestFailure
                {
                    Reason = DataBundleResponseContract.Types.RequestFailure.Types.Reason.InternalError,
                    FailureDescription = "test_description"
                }
            };

            // act, assert
            Assert.Throws<MessageHubException>(() => target.Parse(contract.ToByteArray()));
        }

        [Fact]
        public void Parse_ValidObject_ReturnsBytes()
        {
            // arrange
            var target = new ResponseBundleParser();
            var valid = new DataBundleResponseDto(
                Guid.NewGuid(),
                "A052186D-89E1-4975-8811-2B4E6137491A",
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
            var valid = new DataBundleResponseDto(
                Guid.NewGuid(),
                "A052186D-89E1-4975-8811-2B4E6137491A",
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
