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

using System.Threading.Tasks;
using Energinet.DataHub.PostOffice.Domain.Model;

namespace Energinet.DataHub.PostOffice.Domain.Services
{
    /// <summary>
    /// Provides recipients with access to their messages. The messages may be grouped and returned as a bundle.
    /// </summary>
    public interface IWarehouseDomainService
    {
        /// <summary>
        /// Peeks the next message for the given recipient.
        /// Returns null when there are no new messages.
        /// If a message is available, groups one or more messages into a bundle and returns that bundle.
        /// Once a bundle has been created and returned, it has to be acknowledged through DequeueAsync, before the next bundle can be obtained.
        /// </summary>
        /// <param name="recipient">The recipient of the messages.</param>
        /// <returns>A bundle of the next group of messages; or null, if there are no new messages.</returns>
        Task<IBundle?> PeekAsync(Recipient recipient);

        /// <summary>
        /// Acknowledges the current message bundle, as returned by PeekAsync.
        /// If there is nothing to acknowledge or the id does not match the peeked bundle, the method returns false.
        /// </summary>
        /// <param name="recipient">The recipient of the messages.</param>
        /// <param name="expectedId">The id of the bundle that is being acknowledged.</param>
        /// <returns>true is the bundle was acknowledged; false if the id is incorrect or there is nothing to peek.</returns>
        Task<bool> TryDequeueAsync(Recipient recipient, Uuid expectedId);
    }
}
