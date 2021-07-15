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
using System.Threading.Tasks;
using Energinet.DataHub.PostOffice.Application.GetMessage.Interfaces;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;

namespace Energinet.DataHub.PostOffice.Infrastructure.GetMessage
{
    public class MessageResponseStorage : IMessageResponseStorage
    {
        private static Dictionary<string, string> _savedMessageResponses = new Dictionary<string, string>();

        public Task<string?> GetMessageResponseAsync(string messageKey)
        {
            var elementExists = _savedMessageResponses.TryGetValue(messageKey, out var path);

            return Task.FromResult(elementExists ? path : null);
        }

        public Task SaveMessageReplyAsync(string messageKey, Uri contentUrl)
        {
            if (contentUrl is null) throw new ArgumentNullException(nameof(contentUrl));

            _savedMessageResponses.TryAdd(messageKey, contentUrl.AbsoluteUri);

            return Task.CompletedTask;
        }
    }
}
