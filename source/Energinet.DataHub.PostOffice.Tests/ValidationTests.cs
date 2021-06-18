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
using System.Threading.Tasks;
using AutoFixture;
using Energinet.DataHub.PostOffice.Contracts;
using Energinet.DataHub.PostOffice.Inbound.Parsing;
using Energinet.DataHub.PostOffice.Tests.Tooling;
using FluentAssertions;
using Google.Protobuf.WellKnownTypes;
using Xunit;

namespace Energinet.DataHub.PostOffice.Tests
{
    public class ValidationTests
    {
        private readonly Fixture _fixture;

        public ValidationTests()
        {
            _fixture = new Fixture();
        }

        [Fact]
        public async Task All_input_validations_should_fail_for_empty_document()
        {
            var document = new Document();
            var ruleCollectionTester = RuleCollectionTester.Create<DocumentRules, Document>();

            var result = await ruleCollectionTester.InvokeAsync(document).ConfigureAwait(false);

            result.Count.Should().Be(5);
        }

        [Fact]
        public async Task A_valid_document_should_validate()
        {
            var effectuationDate = _fixture.Create<Timestamp>();
            var type = _fixture.Create<string>();
            var recipient = _fixture.Create<string>();
            var version = _fixture.Create<string>();
            var content = "{\"document\": \"Important message.\"}";
            var document = new Document
            {
                EffectuationDate = effectuationDate,
                Type = type,
                Recipient = recipient,
                Content = content,
                Version = version,
            };
            var ruleCollectionTester = RuleCollectionTester.Create<DocumentRules, Document>();

            var result = await ruleCollectionTester.InvokeAsync(document).ConfigureAwait(false);

            result.Success.Should().BeTrue();
        }

        [Fact]
        public async Task DataAvailable_request_should_be_valid()
        {
            // Arrange
            var dataAvailable = new DataAvailable
            {
                UUID = Guid.NewGuid().ToString(),
                Recipient = Guid.NewGuid().ToString(),
            };

            // Act
            var ruleCollectionTester = RuleCollectionTester.Create<DataAvailableRules, DataAvailable>();

            var result = await ruleCollectionTester.InvokeAsync(dataAvailable).ConfigureAwait(false);

            // Assert
            result.Success.Should().BeTrue();
        }
    }
}
