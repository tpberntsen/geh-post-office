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

using Google.Protobuf;
using GreenEnergyHub.PostOffice.Communicator.Model;

namespace GreenEnergyHub.PostOffice.Communicator.Dequeue
{
    /// <summary>
    /// Parses the DequeueNotification protobuf contract.
    /// </summary>
    public interface IDequeueNotificationParser
    {
        /// <summary>
        /// Parses the DequeueNotification protobuf contract.
        /// </summary>
        /// <param name="dequeueNotificationContract">A byte array containing the DequeueNotification protobuf contract.</param>
        /// <param name="dequeueNotificationDto">The parsed DequeueNotificationDto, if the function returns true.</param>
        /// <returns> _ </returns>
        /// <exception cref="InvalidProtocolBufferException">
        /// Throws an exception if byte array cannot be parsed.
        /// </exception>
        bool TryParse(byte[] dequeueNotificationContract, out DequeueNotificationDto dequeueNotificationDto);
    }
}