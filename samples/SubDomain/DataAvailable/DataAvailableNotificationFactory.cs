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

using System;
using Energinet.DataHub.MessageHub.Model.Model;

namespace DataAvailableNotification
{
    public static class DataAvailableNotificationFactory
    {
        public static DataAvailableNotificationDto CreateOriginDto(DomainOrigin origin, string messageType, string recipient)
        {
            var dto = CreateDto(origin, messageType, recipient);
            return dto;
        }

        private static DataAvailableNotificationDto CreateDto(DomainOrigin origin, string messageType, string recipient)
        {
            return new DataAvailableNotificationDto(
                Guid.NewGuid(),
                new GlobalLocationNumberDto(string.IsNullOrWhiteSpace(recipient) ? GlnHelper.CreateRandomGln() : recipient),
                new MessageTypeDto(string.IsNullOrWhiteSpace(messageType) ? "timeseries" : messageType),
                origin,
                true,
                1);
        }
    }
}
