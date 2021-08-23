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
using System.Linq;
using System.Threading.Tasks;
using AutoFixture;
using Energinet.DataHub.PostOffice.Application.DataAvailable;
using Energinet.DataHub.PostOffice.Application.Validation;
using FluentAssertions;
using GreenEnergyHub.Messaging.Validation;
using Xunit;
using Xunit.Categories;

namespace Energinet.DataHub.PostOffice.Tests.Validation
{
    [UnitTest]
    public class ValidationTests
    {
        [Fact]
        public async Task DataAvailable_request_should_be_valid()
        {
            // Arrange
            var dataAvailable = new DataAvailableCommand(Guid.NewGuid().ToString(), Guid.NewGuid().ToString(), "1", "1", false, 1);

            // Act
            var ruleSet = new DataAvailableRuleSet();
            var validationResult = await ruleSet.ValidateAsync(dataAvailable).ConfigureAwait(false);

            var result = validationResult.Errors
                .Select(error => error.CustomState as PropertyRule)
                .ToList();

            // Assert
            result.Should().BeEmpty();
        }
    }
}
