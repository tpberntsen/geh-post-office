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
using Energinet.DataHub.PostOffice.Application.GetMessage.Queries;
using Energinet.DataHub.PostOffice.Application.Validation;
using Xunit;
using Xunit.Categories;

namespace Energinet.DataHub.PostOffice.Tests.Validation
{
    [UnitTest]
    public class GetMessageRuleSetTests
    {
        [Fact]
        public void IsValid_WhenStringIsGuid_CanBePassedToGuid()
        {
            // Arrange
            var query = new GetMessageQuery(Guid.NewGuid().ToString());
            var ruleSet = new GetMessageRuleSetValidator();

            // Act
            var validationResult = ruleSet.Validate(query);

            // Assert
            Assert.True(validationResult.IsValid);
        }

        [Fact]
        public void IsValid_WhenStringIsNotGuid_CanNotBePassedToGuid()
        {
            // Arrange
            var query = new GetMessageQuery("This is not a guid");
            var ruleSet = new GetMessageRuleSetValidator();

            // Act
            var validationResult = ruleSet.Validate(query);

            // Assert
            Assert.False(validationResult.IsValid);
        }
    }
}
