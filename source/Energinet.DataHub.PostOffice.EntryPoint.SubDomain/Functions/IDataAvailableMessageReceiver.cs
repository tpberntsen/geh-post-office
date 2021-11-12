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

using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Azure.ServiceBus;

namespace Energinet.DataHub.PostOffice.EntryPoint.SubDomain.Functions
{
    /// <summary>
    /// Message receiver for DataAvailable
    /// </summary>
    public interface IDataAvailableMessageReceiver
    {
        /// <summary>
        /// Retrieves a batch of messages.
        /// </summary>
        /// <returns>A list containing the messages</returns>
        Task<IReadOnlyList<Message>> ReceiveAsync();

        /// <summary>
        /// Sends the messages contained in the list to the dead letter queue
        /// </summary>
        /// <param name="messages">The list of messages</param>
        Task DeadLetterAsync(IEnumerable<Message> messages);

        /// <summary>
        /// Marks the messages completed
        /// </summary>
        /// <param name="messages">The list of messages</param>
        Task CompleteAsync(IEnumerable<Message> messages);
    }
}
