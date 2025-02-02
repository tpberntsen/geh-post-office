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

namespace Energinet.DataHub.MessageHub.Model.Model
{
    /// <summary>
    /// Represents a request for a bundle.
    /// <param name="RequestId">Uniquely identififies the current request.</param>
    /// <param name="DataAvailableNotificationReferenceId">A reference id used to obtain the list of requested DataAvailableNotification ids.</param>
    /// <param name="IdempotencyId">Uniquely identififies the contents of the message. Domains can use this property to ensure idempotency.</param>
    /// <param name="MessageType">Specifies the common message type for the requested bundle.</param>
    /// </summary>
    public sealed record DataBundleRequestDto(Guid RequestId, string DataAvailableNotificationReferenceId, string IdempotencyId, string MessageType);
}
