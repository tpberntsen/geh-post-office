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

using System.Collections.Generic;
using System.Threading.Tasks;
using System.Xml;

namespace Energinet.DataHub.PostOffice.Common.Model
{
    public sealed record ErrorDescriptor
    {
        public ErrorDescriptor(string code, string message, string? target = null, IEnumerable<ErrorDescriptor>? details = null)
        {
            Code = code;
            Message = message;
            Target = target;
            Details = details;
        }

        public string Code { get; }
        public string Message { get; }
        public string? Target { get; }
        public IEnumerable<ErrorDescriptor>? Details { get; }

        internal async Task WriteXmlContentsAsync(XmlWriter writer)
        {
            await writer.WriteStartElementAsync(null, "Error", null).ConfigureAwait(false);
            await writer.WriteElementStringAsync(null, "Code", null, Code).ConfigureAwait(false);
            await writer.WriteElementStringAsync(null, "Message", null, Message).ConfigureAwait(false);

            if (Target != null)
            {
                await writer.WriteElementStringAsync(null, "Target", null, Target).ConfigureAwait(false);
            }

            if (Details != null)
            {
                await writer.WriteStartElementAsync(null, "Details", null).ConfigureAwait(false);

                foreach (var detail in Details)
                {
                    await detail.WriteXmlContentsAsync(writer).ConfigureAwait(false);
                }

                await writer.WriteEndElementAsync().ConfigureAwait(false);
            }

            await writer.WriteEndElementAsync().ConfigureAwait(false);
        }
    }
}
