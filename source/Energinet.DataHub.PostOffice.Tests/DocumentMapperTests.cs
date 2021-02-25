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

using AutoFixture;
using Energinet.DataHub.PostOffice.Contracts;
using Energinet.DataHub.PostOffice.Infrastructure;
using FluentAssertions;
using Google.Protobuf.WellKnownTypes;
using Xunit;

namespace Energinet.DataHub.PostOffice.Tests
{
    public class DocumentMapperTests
    {
        private readonly Fixture _fixture;

        public DocumentMapperTests()
        {
            _fixture = new Fixture();
        }

        [Fact]
        public void DocumentShouldBeMapped()
        {
            var effectuationDate = _fixture.Create<Timestamp>();
            var type = _fixture.Create<string>();
            var recipient = _fixture.Create<string>();
            var content = "{\"document\": \"Important message.\"}";
            var document = new Document
            {
                EffectuationDate = effectuationDate,
                Type = type,
                Recipient = recipient,
                Content = content,
            };

            var actual = new DocumentMapper().Map(document);

            actual.Should().NotBeNull();
            actual.Type.Should().Be(type);
            actual.Recipient.Should().Be(recipient);
            actual.EffectuationDate?.ToDateTimeOffset().Should().Be(effectuationDate.ToDateTimeOffset());
        }
    }
}
