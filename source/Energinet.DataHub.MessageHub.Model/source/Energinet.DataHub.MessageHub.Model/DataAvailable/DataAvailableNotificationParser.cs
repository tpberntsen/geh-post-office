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
using System.Linq;
using Energinet.DataHub.MessageHub.Model.Exceptions;
using Energinet.DataHub.MessageHub.Model.Model;
using Energinet.DataHub.MessageHub.Model.Protobuf;
using Google.Protobuf;
using Microsoft.Azure.ServiceBus;

namespace Energinet.DataHub.MessageHub.Model.DataAvailable
{
    public sealed class DataAvailableNotificationParser : IDataAvailableNotificationParser
    {
        public DataAvailableNotificationDto Parse(byte[] dataAvailableContract)
        {
            try
            {
                var dataAvailable = DataAvailableNotificationContract.Parser.ParseFrom(dataAvailableContract);

                return new DataAvailableNotificationDto(
                    Uuid: Guid.Parse(dataAvailable.UUID),
                    Recipient: new GlobalLocationNumberDto(dataAvailable.Recipient),
                    MessageType: new MessageTypeDto(dataAvailable.MessageType),
                    Origin: Enum.Parse<DomainOrigin>(dataAvailable.Origin),
                    SupportsBundling: dataAvailable.SupportsBundling,
                    RelativeWeight: dataAvailable.RelativeWeight);
            }
            catch (Exception ex) when (ex is InvalidProtocolBufferException or FormatException)
            {
                throw new MessageHubException("Error parsing byte array for DataAvailableNotification", ex);
            }
        }

        public bool TryParse(Message message, string messageId, out DataAvailableDto dataAvailableDto)
        {
            if (message is null)
                throw new ArgumentNullException(nameof(message));

            var data = DataAvailableNotificationContract.Parser.ParseFrom(message.Body);

            if (!ValuesAreValid(data, messageId))
            {
                SetToBadValues(out dataAvailableDto);
                return false;
            }

            SetDtoToCorrectValues(out dataAvailableDto, messageId, data);

            return true;
        }

        private static bool ValuesAreValid(DataAvailableNotificationContract contract, string messageId)
        {
            var values = new List<string>
            {
                contract.UUID,
                contract.Recipient,
                contract.MessageType,
                contract.Origin,
                messageId
            };

            return !values.All(string.IsNullOrEmpty);
        }

        private static void SetToBadValues(out DataAvailableDto dataAvailableDto)
        {
            dataAvailableDto = new DataAvailableDto(
                Guid.Empty,
                new GlobalLocationNumberDto(string.Empty),
                new MessageTypeDto(string.Empty),
                DomainOrigin.Unknown,
                false,
                0,
                string.Empty,
                false);
        }

        private static void SetDtoToCorrectValues(out DataAvailableDto dataAvailableDto, string messageId, DataAvailableNotificationContract? data)
        {
            if (data is null)
                throw new ArgumentNullException(nameof(data));

            dataAvailableDto = new DataAvailableDto(
                Guid.Parse(data.UUID),
                new GlobalLocationNumberDto(data.Recipient),
                new MessageTypeDto(data.MessageType),
                Enum.Parse<DomainOrigin>(data.Origin),
                data.SupportsBundling,
                data.RelativeWeight,
                messageId,
                true);
        }
    }
}
