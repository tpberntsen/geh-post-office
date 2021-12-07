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
using Newtonsoft.Json;

namespace Energinet.DataHub.Logging.SearchOptimizer.Models
{
    public sealed class Search
    {
        public Search(
            string messageId,
            DocumentType documentType,
            string processId,
            DateTime @from,
            DateTime to,
            string senderId,
            string receiverId,
            string businessReasonCode)
        {
            MessageId = messageId;
            DocumentType = documentType;
            ProcessId = processId;
            From = @from;
            To = to;
            SenderId = senderId;
            ReceiverId = receiverId;
            BusinessReasonCode = businessReasonCode;
        }

        [JsonProperty(PropertyName = "id")]
        public Guid Id { get; } = Guid.NewGuid();
        public string MessageId { get; set; }
        public DocumentType DocumentType { get; set; }
        public string ProcessId { get; set; }
        public DateTime From { get; set; }
        public DateTime To { get; set; }
        public string SenderId { get; set; }
        public string ReceiverId { get; set; }
        public string BusinessReasonCode { get; set; }
    }
}
