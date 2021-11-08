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

namespace Energinet.DataHub.MessageHub.Client.Factories
{
    /// <summary>
    /// Abstraction for messagebus factory
    /// </summary>
    public interface IMessageBusFactory
    {
        /// <summary>
        /// Creates sender reference
        /// </summary>
        /// <param name="queueOrTopicName"></param>
        ISenderMessageBus GetSenderClient(string queueOrTopicName);

        /// <summary>
        /// Creates session receiver reference
        /// </summary>
        /// <returns><see cref="IReceiverMessageBus"/></returns>
        public Task<IReceiverMessageBus> GetSessionReceiverClientAsync(string queueOrTopicName, string sessionId);
    }
}
