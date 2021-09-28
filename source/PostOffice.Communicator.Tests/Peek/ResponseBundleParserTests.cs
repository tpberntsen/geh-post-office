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
using GreenEnergyHub.PostOffice.Communicator.Exceptions;
using GreenEnergyHub.PostOffice.Communicator.Model;
using GreenEnergyHub.PostOffice.Communicator.Peek;
using Xunit;
using Xunit.Categories;

namespace PostOffice.Communicator.Tests.Peek
{
    [UnitTest]
    public class ResponseBundleParserTests
    {
        [Fact]
        public void TryParse_BytesValid_ReturnsTrue()
        {
            // arrange
            var target = new ResponseBundleParser();
            var validBytes = new RequestBundleResponse
            {
                Success = new RequestBundleResponse.Types.FileResource
                {
                    Uri = "http://localhost",
                    UUID = { new[] { "B34E47BC-21EA-40C5-AE27-A5900F42D7C6" } }
                }
            }.ToByteArray();

            // act
            var actual = target.Parse(validBytes);

            // assert
            Assert.NotNull(actual);
            Assert.Equal(new[] { "B34E47BC-21EA-40C5-AE27-A5900F42D7C6" }, actual.DataAvailableNotificationIds);
        }

        [Fact]
        public void TryParse_BytesValidWithFailedRequestStatus_ReturnsFalse()
        {
            // arrange
            var target = new ResponseBundleParser();
            var validBytes = new RequestBundleResponse
            {
                Failure = new RequestBundleResponse.Types.RequestFailure
                {
                    Reason = RequestBundleResponse.Types.RequestFailure.Types.Reason.InternalError
                }
            }.ToByteArray();

            // act
            var actual = target.Parse(validBytes);

            // assert
            Assert.Null(actual);
        }

        [Fact]
        public void TryParse_BytesCorrupt_ReturnsFalse()
        {
            // arrange
            var target = new ResponseBundleParser();
            var rnd = new Random();
            var corruptBytes = new byte[10];
            rnd.NextBytes(corruptBytes);

            // act, assert
            Assert.Throws<PostOfficeCommunicatorException>(() => target.Parse(corruptBytes));
        }
    }
}
