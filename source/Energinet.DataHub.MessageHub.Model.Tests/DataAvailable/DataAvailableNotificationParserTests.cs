﻿// Copyright 2020 Energinet DataHub A/S
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

using System.Globalization;
using Energinet.DataHub.MessageHub.Model.DataAvailable;
using Energinet.DataHub.MessageHub.Model.Exceptions;
using Energinet.DataHub.MessageHub.Model.Protobuf;
using Google.Protobuf;
using Xunit;
using Xunit.Categories;

namespace Energinet.DataHub.MessageHub.Model.Tests.DataAvailable
{
    [UnitTest]
    public sealed class DataAvailableNotificationParserTests
    {
        [Fact]
        public void Parse_ValidInput_ReturnsData()
        {
            // Arrange
            var target = new DataAvailableNotificationParser();
            var contract = new DataAvailableNotificationContract
            {
                UUID = "94681547-C70D-409C-9255-83B310AF7010",
                MessageType = "messageType",
                Origin = "TimeSeries",
                Recipient = "recipient",
                RelativeWeight = 5,
                SupportsBundling = true
            };

            // Act
            var actual = target.Parse(contract.ToByteArray());

            // Assert
            Assert.NotNull(actual);
            Assert.Equal(contract.UUID, actual.Uuid.ToString().ToUpper(CultureInfo.InvariantCulture));
            Assert.Equal(contract.MessageType, actual.MessageType.Value);
            Assert.Equal(contract.Origin, actual.Origin.ToString());
            Assert.Equal(contract.Recipient, actual.Recipient.Value);
            Assert.Equal(contract.RelativeWeight, actual.RelativeWeight);
            Assert.Equal(contract.SupportsBundling, actual.SupportsBundling);
        }

        [Fact]
        public void Parse_InvalidGuid_ThrowsException()
        {
            // Arrange
            var target = new DataAvailableNotificationParser();
            var contract = new DataAvailableNotificationContract
            {
                UUID = "invalid_guid",
                MessageType = "messageType",
                Origin = "TimeSeries",
                Recipient = "recipient",
                RelativeWeight = 5,
                SupportsBundling = true
            };

            // Act + Assert
            Assert.Throws<MessageHubException>(() => target.Parse(contract.ToByteArray()));
        }

        [Fact]
        public void Parse_InvalidInput_ThrowsException()
        {
            // Arrange
            var target = new DataAvailableNotificationParser();

            // Act + Assert
            Assert.Throws<MessageHubException>(() => target.Parse(new byte[] { 1, 2, 3 }));
        }
    }
}
