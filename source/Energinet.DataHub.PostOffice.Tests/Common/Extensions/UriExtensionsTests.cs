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
using Energinet.DataHub.PostOffice.Common.Extensions;
using Xunit;
using Xunit.Categories;

namespace Energinet.DataHub.PostOffice.Tests.Common.Extensions
{
    [UnitTest]
    public class UriExtensionsTests
    {
        [Fact]
        public void GetQueryValue_HasName_ReturnsValue()
        {
            // Arrange
            var target = new Uri("http://localhost:8080?name=value");

            // Act
            var actual = target.GetQueryValue("name");

            // Assert
            Assert.Equal("value", actual);
        }

        [Fact]
        public void GetQueryValue_DoesNotHaveName_ReturnsEmptyString()
        {
            // Arrange
            var target = new Uri("http://localhost:8080?name=value");

            // Act
            var actual = target.GetQueryValue("other");

            // Assert
            Assert.Equal(string.Empty, actual);
        }

        [Fact]
        public void GetQueryValue_NullTarget_ThrowsException()
        {
            // Arrange
            Uri target = null!;

            // Act + Assert
            Assert.Throws<ArgumentNullException>(() => target.GetQueryValue("name"));
        }
    }
}
