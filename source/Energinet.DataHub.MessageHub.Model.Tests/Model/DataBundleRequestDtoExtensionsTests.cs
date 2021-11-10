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
using System.Collections.Generic;
using Energinet.DataHub.MessageHub.Model.Extensions;
using Energinet.DataHub.MessageHub.Model.Model;
using Xunit;
using Xunit.Categories;

namespace Energinet.DataHub.MessageHub.Model.Tests.Model
{
    [UnitTest]
    public class DataBundleRequestDtoExtensionsTests
    {
        [Fact]
        public void CreateResponse_RequestNull_Throws()
        {
            // arrange, act, assert
            Assert.Throws<ArgumentNullException>(() => ((DataBundleRequestDto)null)!.CreateResponse(new Uri("http://localhost")));
        }

        [Fact]
        public void CreateErrorResponse_RequestNull_Throws()
        {
            // arrange, act, assert
            Assert.Throws<ArgumentNullException>(() => ((DataBundleRequestDto)null)!.CreateErrorResponse(new DataBundleResponseErrorDto()));
        }

        [Fact]
        public void CreateResponse_ReturnsResponse()
        {
            // arrage
            var dataAvailableNotificationIds = new List<Guid> { Guid.NewGuid() };
            var requestId = Guid.Parse("BCDFAF35-B914-488E-A8FB-C41FC377097D");
            var uri = new Uri("http://localhost");
            var request = new DataBundleRequestDto(
                requestId,
                "D5D400AD-CC11-409A-B757-75EB9AA8B0EA",
                "message_type",
                dataAvailableNotificationIds);

            // act
            var actual = request.CreateResponse(uri);

            // assert
            Assert.Equal(requestId, actual.RequestId);
            Assert.Equal(uri, actual.ContentUri);
            Assert.Equal(dataAvailableNotificationIds, actual.DataAvailableNotificationIds);
        }

        [Fact]
        public void CreateErrorResponse_ReturnsResponse()
        {
            // arrage
            var dataAvailableNotificationIds = new List<Guid> { Guid.NewGuid() };
            var requestId = Guid.Parse("BCDFAF35-B914-488E-A8FB-C41FC377097D");
            var request = new DataBundleRequestDto(
                requestId,
                "D5D400AD-CC11-409A-B757-75EB9AA8B0EA",
                "message_type",
                dataAvailableNotificationIds);

            var dataBundleResponseErrorDto = new DataBundleResponseErrorDto();

            // act
            var actual = request.CreateErrorResponse(dataBundleResponseErrorDto);

            // assert
            Assert.Equal(requestId, actual.RequestId);
            Assert.Equal(dataBundleResponseErrorDto, actual.ResponseError);
        }
    }
}
