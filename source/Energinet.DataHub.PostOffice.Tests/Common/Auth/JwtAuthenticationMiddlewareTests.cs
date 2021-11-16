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
using System.Threading.Tasks;
using Energinet.DataHub.PostOffice.Common.Auth;
using Xunit;
using Xunit.Categories;

namespace Energinet.DataHub.PostOffice.Tests.Common.Auth
{
    [UnitTest]
    public sealed class JwtAuthenticationMiddlewareTests
    {
        [Fact]
        public async Task Invoke_NullContext_ThrowsException()
        {
            // Arrange
            var identity = new MarketOperatorIdentity();
            var target = new JwtAuthenticationMiddleware(identity);

            // Act + Assert
            await Assert
                .ThrowsAsync<ArgumentNullException>(() => target.Invoke(null!, _ => Task.CompletedTask))
                .ConfigureAwait(false);
        }

        [Fact]
        public async Task Invoke_NullDelegate_ThrowsException()
        {
            // Arrange
            var identity = new MarketOperatorIdentity();
            var target = new JwtAuthenticationMiddleware(identity);

            // Act + Assert
            await Assert
                .ThrowsAsync<ArgumentNullException>(() => target.Invoke(new MockedFunctionContext(), null!))
                .ConfigureAwait(false);
        }

        [Fact]
        public async Task Invoke_HasBearer_AssignsGln()
        {
            // Arrange
            var data = new Dictionary<string, object?>(StringComparer.OrdinalIgnoreCase)
            {
                { "Headers", "{\"Authorization\":\"Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIxMjM0In0.vVkzbkZ6lB3srqYWXVA00ic5eXwy4R8oniHQyok0QWY\"}" }
            };

            var identity = new MarketOperatorIdentity();
            var mockedFunctionContext = new MockedFunctionContext();
            mockedFunctionContext.BindingContext.Setup(x => x.BindingData)
                .Returns(data);

            var target = new JwtAuthenticationMiddleware(identity);

            // Act
            await target.Invoke(mockedFunctionContext, _ => Task.CompletedTask).ConfigureAwait(false);

            // Assert
            Assert.Equal("1234", identity.Gln);
        }

        [Fact]
        public async Task Invoke_HasIdentity_DoesNotOverwrite()
        {
            // Arrange
            var data = new Dictionary<string, object?>(StringComparer.OrdinalIgnoreCase)
            {
                { "Headers", "{\"Authorization\":\"Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIxMjM0In0.vVkzbkZ6lB3srqYWXVA00ic5eXwy4R8oniHQyok0QWY\"}" }
            };

            var identity = new MarketOperatorIdentity();
            ((IMarketOperatorIdentity)identity).AssignGln("other");

            var mockedFunctionContext = new MockedFunctionContext();
            mockedFunctionContext.BindingContext.Setup(x => x.BindingData)
                .Returns(data);

            var target = new JwtAuthenticationMiddleware(identity);

            // Act
            await target.Invoke(mockedFunctionContext, _ => Task.CompletedTask).ConfigureAwait(false);

            // Assert
            Assert.Equal("other", identity.Gln);
        }

        [Fact]
        public async Task Invoke_BrokenJwt_DoesNothing()
        {
            // Arrange
            var data = new Dictionary<string, object?>(StringComparer.OrdinalIgnoreCase)
            {
                { "Headers", "{\"Authorization\":\"Bearer eyJhbGciO_BROKEN_I1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIxMjM0In0.vVkzbkZ6lB3srqYWXVA00ic5eXwy4R8oniHQyok0QWY\"}" }
            };

            var identity = new MarketOperatorIdentity();

            var mockedFunctionContext = new MockedFunctionContext();
            mockedFunctionContext.BindingContext.Setup(x => x.BindingData)
                .Returns(data);

            var target = new JwtAuthenticationMiddleware(identity);

            // Act
            await target.Invoke(mockedFunctionContext, _ => Task.CompletedTask).ConfigureAwait(false);

            // Assert
            Assert.False(identity.HasIdentity);
        }

        [Fact]
        public async Task Invoke_BrokenHeader_DoesNothing()
        {
            // Arrange
            var data = new Dictionary<string, object?>(StringComparer.OrdinalIgnoreCase)
            {
                { "Headers", "{\"Authorization_BROKEN\":\"Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIxMjM0In0.vVkzbkZ6lB3srqYWXVA00ic5eXwy4R8oniHQyok0QWY\"}" }
            };

            var identity = new MarketOperatorIdentity();

            var mockedFunctionContext = new MockedFunctionContext();
            mockedFunctionContext.BindingContext.Setup(x => x.BindingData)
                .Returns(data);

            var target = new JwtAuthenticationMiddleware(identity);

            // Act
            await target.Invoke(mockedFunctionContext, _ => Task.CompletedTask).ConfigureAwait(false);

            // Assert
            Assert.False(identity.HasIdentity);
        }
    }
}
