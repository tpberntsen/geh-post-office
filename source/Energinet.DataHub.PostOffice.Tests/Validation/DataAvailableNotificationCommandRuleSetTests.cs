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
    public sealed class DataAvailableNotificationCommandRuleSetTests
    {
        private const int ValidWeight = 1;
        private const string ValidUuid = "169B53A2-0A17-47D7-9603-4E41854E4181";
        private const string ValidOrigin = "Charges";
        private const string ValidRecipient = "5790000555550";
        private const string ValidContentType = "TimeSeries";

        [Theory]
        [InlineData("", false)]
        [InlineData(null, false)]
        [InlineData("  ", false)]
        [InlineData("8F9B8218-BAE6-412B-B91B-0C78A55FF128", true)]
        [InlineData("8F9B8218-BAE6-412B-B91B-0C78A55FF1XX", false)]
        public async Task Validate_Uuid_ValidatesProperty(string value, bool isValid)
        {
            // Arrange
            const string propertyName = nameof(DataAvailableNotificationCommand.Uuid);

            var target = new DataAvailableNotificationCommandRuleSet();
            var command = new DataAvailableNotificationCommand(
                value,
                ValidRecipient,
                ValidContentType,
                ValidOrigin,
                false,
                ValidWeight,
                1);

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

        [Theory]
        [InlineData("", false)]
        [InlineData(null, false)]
        [InlineData("  ", false)]
        [InlineData("5790000555550", true)]
        public async Task Validate_Recipient_ValidatesProperty(string value, bool isValid)
        {
            // Arrange
            const string propertyName = nameof(DataAvailableNotificationCommand.Recipient);

            var target = new DataAvailableNotificationCommandRuleSet();
            var command = new DataAvailableNotificationCommand(
                ValidUuid,
                value,
                ValidContentType,
                ValidOrigin,
                false,
                ValidWeight,
                1);

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

        [Theory]
        [InlineData("", false)]
        [InlineData(null, false)]
        [InlineData("  ", false)]
        [InlineData("Unknown", true)]
        [InlineData("TimeSeries", true)]
        [InlineData("timeseries", true)]
        public async Task Validate_ContentType_ValidatesProperty(string value, bool isValid)
        {
            // Arrange
            const string propertyName = nameof(DataAvailableNotificationCommand.ContentType);

            var target = new DataAvailableNotificationCommandRuleSet();
            var command = new DataAvailableNotificationCommand(
                ValidUuid,
                ValidRecipient,
                value,
                ValidOrigin,
                false,
                ValidWeight,
                1);

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

        [Theory]
        [InlineData("", false)]
        [InlineData(null, false)]
        [InlineData("  ", false)]
        [InlineData("Unknown", false)]
        [InlineData("Charges", true)]
        [InlineData("TimeSeries", true)]
        [InlineData("Aggregations", true)]
        public async Task Validate_Origin_ValidatesProperty(string value, bool isValid)
        {
            // Arrange
            const string propertyName = nameof(DataAvailableNotificationCommand.Origin);

            var target = new DataAvailableNotificationCommandRuleSet();
            var command = new DataAvailableNotificationCommand(
                ValidUuid,
                ValidRecipient,
                ValidContentType,
                value,
                false,
                ValidWeight,
                1);

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

        [Theory]
        [InlineData(0, false)]
        [InlineData(-1, false)]
        [InlineData(-10, false)]
        [InlineData(int.MinValue, false)]
        [InlineData(1, true)]
        [InlineData(10, true)]
        [InlineData(int.MaxValue, true)]
        public async Task Validate_Weight_ValidatesProperty(int value, bool isValid)
        {
            // Arrange
            const string propertyName = nameof(DataAvailableNotificationCommand.Weight);

            var target = new DataAvailableNotificationCommandRuleSet();
            var command = new DataAvailableNotificationCommand(
                ValidUuid,
                ValidRecipient,
                ValidContentType,
                ValidOrigin,
                false,
                value,
                1);

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
