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

using System.Linq;
using System.Threading.Tasks;
using Energinet.DataHub.PostOffice.Application.Commands;
using Energinet.DataHub.PostOffice.Application.Validation;
using Xunit;
using Xunit.Categories;

namespace Energinet.DataHub.PostOffice.Tests.Validation
{
    [UnitTest]
    public sealed class UpdateMaximumSequenceNumberCommandRuleSetTests
    {
        [Theory]
        [InlineData(0, false)]
        [InlineData(-1, false)]
        [InlineData(-10, false)]
        [InlineData(int.MinValue, false)]
        [InlineData(1, true)]
        [InlineData(10, true)]
        [InlineData(int.MaxValue, true)]
        [InlineData(10000000000, true)]
        [InlineData(100000000000, true)]
        [InlineData(long.MaxValue, true)]
        public async Task Validate_SequenceNumber_ValidatesProperty(long value, bool isValid)
        {
            // Arrange
            const string propertyName = nameof(UpdateMaximumSequenceNumberCommand.SequenceNumber);

            var target = new UpdateMaximumSequenceNumberCommandRuleSet();
            var command = new UpdateMaximumSequenceNumberCommand(value);

            // Act
            var result = await target.ValidateAsync(command).ConfigureAwait(false);

            // Assert
            if (isValid)
            {
                Assert.True(result.IsValid);
                Assert.DoesNotContain(propertyName, result.Errors.Select(x => x.PropertyName));
            }
            else
            {
                Assert.False(result.IsValid);
                Assert.Contains(propertyName, result.Errors.Select(x => x.PropertyName));
            }
        }
    }
}
