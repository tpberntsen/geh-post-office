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
using Energinet.DataHub.MessageHub.Model.Exceptions;
using Energinet.DataHub.MessageHub.Model.Model;
using Energinet.DataHub.MessageHub.Model.Protobuf;
using Google.Protobuf;

namespace Energinet.DataHub.MessageHub.Model.Dequeue
{
    public sealed class DequeueNotificationParser : IDequeueNotificationParser
    {
        public DequeueNotificationDto Parse(byte[] dequeueNotificationContract)
        {
            try
            {
                var dequeueContract = DequeueContract.Parser.ParseFrom(dequeueNotificationContract);
                return new DequeueNotificationDto(
                    dequeueContract.DataAvailableNotificationReferenceId,
                    new GlobalLocationNumberDto(dequeueContract.MarketOperator));
            }
            catch (Exception ex) when (ex is InvalidProtocolBufferException or FormatException)
            {
                throw new MessageHubException("Error parsing bytes for DequeueNotificationDto.", ex);
            }
        }

        public byte[] Parse(DequeueNotificationDto dequeueNotificationDto)
        {
            if (dequeueNotificationDto == null)
                throw new ArgumentNullException(nameof(dequeueNotificationDto));

            var message = new DequeueContract
            {
                DataAvailableNotificationReferenceId = dequeueNotificationDto.DataAvailableNotificationReferenceId,
                MarketOperator = dequeueNotificationDto.MarketOperator.Value,
            };

            return message.ToByteArray();
        }
    }
}
