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
using Energinet.DataHub.Core.FunctionApp.Common.Abstractions.Actor;
using Energinet.DataHub.PostOffice.Common.Auth;
using Moq;
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
            var actorContext = new Mock<IActorContext>();
            var identity = new MarketOperatorIdentity();
            var target = new JwtAuthenticationMiddleware(identity, actorContext.Object);

            // Act + Assert
            await Assert
                .ThrowsAsync<ArgumentNullException>(() => target.Invoke(null!, _ => Task.CompletedTask))
                .ConfigureAwait(false);
        }

        [Fact]
        public async Task Invoke_NullDelegate_ThrowsException()
        {
            // Arrange
            var actorContext = new Mock<IActorContext>();
            var identity = new MarketOperatorIdentity();
            var target = new JwtAuthenticationMiddleware(identity, actorContext.Object);

            // Act + Assert
            await Assert
                .ThrowsAsync<ArgumentNullException>(() => target.Invoke(new MockedFunctionContext(), null!))
                .ConfigureAwait(false);
        }

        [Fact]
        public async Task Invoke_ActorContextHasIdentity_AsignsGlnToIdentity()
        {
            // Arrange
            var identity = new MarketOperatorIdentity();
            var mockedFunctionContext = new MockedFunctionContext();
            var actorContext = new Mock<IActorContext>();
            actorContext.Setup(x => x.CurrentActor).Returns(new Actor(Guid.NewGuid(), "?", "1234", "?"));

            var target = new JwtAuthenticationMiddleware(identity, actorContext.Object);

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

            var actorContext = new Mock<IActorContext>();
            actorContext.Setup(x => x.CurrentActor).Returns(new Actor(Guid.NewGuid(), "?", "1234", "?"));
            var target = new JwtAuthenticationMiddleware(identity, actorContext.Object);

            // Act
            await target.Invoke(mockedFunctionContext, _ => Task.CompletedTask).ConfigureAwait(false);

            // Assert
            Assert.Equal("other", identity.Gln);
        }
    }
}
