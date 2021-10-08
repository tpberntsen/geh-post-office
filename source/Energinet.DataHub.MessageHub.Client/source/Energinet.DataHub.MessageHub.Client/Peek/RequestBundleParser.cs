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
using System.Linq;
using Energinet.DataHub.MessageHub.Client.Exceptions;
using Energinet.DataHub.MessageHub.Client.Model;
using Energinet.DataHub.MessageHub.Client.Protobuf;
using Google.Protobuf;

namespace Energinet.DataHub.MessageHub.Client.Peek
{
    public sealed class RequestBundleParser : IRequestBundleParser
    {
        public byte[] Parse(DataBundleRequestDto request)
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request));

            var message = new DataBundleRequestContract
            {
                IdempotencyId = request.IdempotencyId,
                DataAvailableNotificationIds = { request.DataAvailableNotificationIds.Select(x => x.ToString()) }
            };
            return message.ToByteArray();
        }

        public DataBundleRequestDto Parse(byte[] dataBundleRequestContract)
        {
            try
            {
                var bundleResponse = DataBundleRequestContract.Parser.ParseFrom(dataBundleRequestContract);
                return new DataBundleRequestDto(bundleResponse.IdempotencyId, bundleResponse.DataAvailableNotificationIds.Select(Guid.Parse).ToList());
            }
            catch (InvalidProtocolBufferException e)
            {
                throw new PostOfficeCommunicatorException("Error parsing bytes for DataBundleRequestDto", e);
            }
        }
    }
}
