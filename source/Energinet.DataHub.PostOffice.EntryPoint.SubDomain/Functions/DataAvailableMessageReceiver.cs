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
using System.Threading.Tasks;
using Microsoft.Azure.ServiceBus;
using Microsoft.Azure.ServiceBus.Core;

namespace Energinet.DataHub.PostOffice.EntryPoint.SubDomain.Functions
{
    public class DataAvailableMessageReceiver : IDataAvailableMessageReceiver
    {
        private readonly IMessageReceiver _messageReceiver;
        private readonly int _batchSize;
        private readonly TimeSpan _timeout;

        public DataAvailableMessageReceiver(IMessageReceiver messageReceiver, int batchSize, TimeSpan timeout)
        {
            _messageReceiver = messageReceiver;
            _batchSize = batchSize;
            _timeout = timeout;
        }

        public async Task<IReadOnlyList<Message>> ReceiveAsync()
        {
            return (await _messageReceiver.ReceiveAsync(_batchSize, _timeout).ConfigureAwait(false)).ToList();
        }

        public Task DeadLetterAsync(IEnumerable<Message> messages)
        {
            var tasks = messages.Select(x => _messageReceiver.DeadLetterAsync(x.SystemProperties.LockToken));
            return Task.WhenAll(tasks);
        }

        public Task CompleteAsync(IEnumerable<Message> messages)
        {
            return _messageReceiver.CompleteAsync(messages.Select(x => x.SystemProperties.LockToken));
        }
    }
}
