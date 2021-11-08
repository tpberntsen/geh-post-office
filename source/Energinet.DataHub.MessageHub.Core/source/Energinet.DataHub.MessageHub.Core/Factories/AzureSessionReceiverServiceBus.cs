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
using System.Threading.Tasks;
using Azure.Messaging.ServiceBus;

namespace Energinet.DataHub.MessageHub.Core.Factories
{
    public sealed class AzureSessionReceiverServiceBus : IReceiverMessageBus
    {
        private readonly ServiceBusSessionReceiver _serviceBusSessionReceiver;

        private AzureSessionReceiverServiceBus(ServiceBusSessionReceiver serviceBusSessionReceiver)
        {
            _serviceBusSessionReceiver = serviceBusSessionReceiver;
        }

        public async ValueTask DisposeAsync()
        {
            await _serviceBusSessionReceiver.DisposeAsync().ConfigureAwait(false);
        }

        public async Task<ServiceBusReceivedMessage?> ReceiveMessageAsync<T>(TimeSpan timeout)
        {
            return await _serviceBusSessionReceiver.ReceiveMessageAsync(timeout).ConfigureAwait(false);
        }

        internal static AzureSessionReceiverServiceBus Wrap(ServiceBusSessionReceiver sessionReceiver)
        {
            return new(sessionReceiver);
        }
    }
}
