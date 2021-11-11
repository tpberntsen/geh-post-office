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

using Azure.Messaging.ServiceBus;

namespace Energinet.DataHub.MessageHub.IntegrationTesting
{
    public class MessageHubSimulationConfig
    {
        /// <param name="dataAvailableReceiver">A ServiceBus receiver for the 'sbq-dataavailable' queue.</param>
        /// <param name="domainPeekRequestSender">A ServiceBus sender for the 'sbq-[domain]' queue.</param>
        /// <param name="domainDequeueSender">An optional ServiceBus sender for the 'sbq-[domain]-dequeue' queue.</param>
        /// <param name="domainPeekReplyReceiverFactory">A factory for a ServiceBus session-enabled receiver for the 'sbq-[domain]-reply' queue.</param>
        public MessageHubSimulationConfig(
            ServiceBusReceiver dataAvailableReceiver,
            ServiceBusSender domainPeekRequestSender,
            ServiceBusSender domainDequeueSender,
            ServiceBusSessionEnabledReceiverFactory domainPeekReplyReceiverFactory)
        {
            DataAvailableReceiver = dataAvailableReceiver;
            DomainPeekRequestSender = domainPeekRequestSender;
            DomainPeekReplyReceiverFactory = domainPeekReplyReceiverFactory;
            DomainDequeueSender = domainDequeueSender;
        }

        /// <param name="dataAvailableReceiver">A ServiceBus receiver for the 'sbq-dataavailable' queue.</param>
        /// <param name="domainPeekRequestSender">A ServiceBus sender for the 'sbq-[domain]' queue.</param>
        /// <param name="domainPeekReplyReceiverFactory">A factory for a ServiceBus session-enabled receiver for the 'sbq-[domain]-reply' queue.</param>
        public MessageHubSimulationConfig(
            ServiceBusReceiver dataAvailableReceiver,
            ServiceBusSender domainPeekRequestSender,
            ServiceBusSessionEnabledReceiverFactory domainPeekReplyReceiverFactory)
        {
            DataAvailableReceiver = dataAvailableReceiver;
            DomainPeekRequestSender = domainPeekRequestSender;
            DomainPeekReplyReceiverFactory = domainPeekReplyReceiverFactory;
            DomainDequeueSender = null;
        }

        /// <summary>
        /// A ServiceBus receiver for the 'sbq-dataavailable' queue.
        /// </summary>
        public virtual ServiceBusReceiver DataAvailableReceiver { get; }

        /// <summary>
        /// A ServiceBus sender for the 'sbq-[domain]' queue.
        /// </summary>
        public virtual ServiceBusSender DomainPeekRequestSender { get; }

        /// <summary>
        /// A ServiceBus session-enabled receiver for the 'sbq-[domain]-reply' queue.
        /// </summary>
        public virtual ServiceBusSessionEnabledReceiverFactory DomainPeekReplyReceiverFactory { get; }

        /// <summary>
        /// An optional ServiceBus sender for the 'sbq-[domain]-dequeue' queue.
        /// </summary>
        public virtual ServiceBusSender? DomainDequeueSender { get; }
    }
}
