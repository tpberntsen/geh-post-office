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

using Energinet.DataHub.PostOffice.Application.Commands;
using MediatR;

namespace Energinet.DataHub.PostOffice.Application.DataAvailable
{
    public record DataAvailableCommand : IRequest<DataAvailableNotificationResponse>
    {
        public DataAvailableCommand(string uuid, string recipient, string messageType, string origin, bool supportsBundling, int relativeWeight)
        {
            UUID = uuid;
            Recipient = recipient;
            MessageType = messageType;
            Origin = origin;
            SupportsBundling = supportsBundling;
            RelativeWeight = relativeWeight;
        }

        public string UUID { get; }
        public string Recipient { get; }
        public string MessageType { get; }
        public string Origin { get; }
        public bool SupportsBundling { get; }
        public int RelativeWeight { get; }
    }
}
