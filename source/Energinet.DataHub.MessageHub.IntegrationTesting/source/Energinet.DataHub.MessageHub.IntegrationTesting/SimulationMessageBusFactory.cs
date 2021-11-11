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
using Energinet.DataHub.MessageHub.Core;
using Energinet.DataHub.MessageHub.Core.Factories;

namespace Energinet.DataHub.MessageHub.IntegrationTesting
{
    internal sealed class SimulationMessageBusFactory : IMessageBusFactory
    {
        private const string PeekSimulationQueue = "PeekSimulationQueue";
        private const string DequeueSimulationQueue = "DequeueSimulationQueue";

        private readonly ServiceBusSender _domainPeekRequestSender;
        private readonly ServiceBusSender? _domainDequeueSender;
        private readonly ServiceBusSessionEnabledReceiverFactory _domainPeekReplyReceiverFactory;

        public SimulationMessageBusFactory(MessageHubSimulationConfig configuration)
        {
            _domainPeekRequestSender = configuration.DomainPeekRequestSender;
            _domainPeekReplyReceiverFactory = configuration.DomainPeekReplyReceiverFactory;

            if (configuration.DomainDequeueSender != null)
                _domainDequeueSender = configuration.DomainDequeueSender;
        }

        public DequeueConfig SimulatedDeqeueueConfig { get; } = new(
            DequeueSimulationQueue,
            DequeueSimulationQueue,
            DequeueSimulationQueue,
            DequeueSimulationQueue,
            DequeueSimulationQueue);

        public PeekRequestConfig SimulatedPeekRequestConfig { get; } = new(
            PeekSimulationQueue,
            PeekSimulationQueue,
            PeekSimulationQueue,
            PeekSimulationQueue,
            PeekSimulationQueue,
            PeekSimulationQueue,
            PeekSimulationQueue,
            PeekSimulationQueue,
            PeekSimulationQueue,
            PeekSimulationQueue);

        public ISenderMessageBus GetSenderClient(string queueOrTopicName)
        {
            if (string.Equals(queueOrTopicName, PeekSimulationQueue, StringComparison.OrdinalIgnoreCase))
                return new SimulationSenderMessageBus(_domainPeekRequestSender);

            if (string.Equals(queueOrTopicName, DequeueSimulationQueue, StringComparison.OrdinalIgnoreCase))
            {
                if (_domainDequeueSender == null)
                    throw new InvalidOperationException("MessageHubSimulation: Simulation was not configured for Dequeue.");

                return new SimulationSenderMessageBus(_domainDequeueSender);
            }

            throw new InvalidOperationException($"MessageHubSimulation: Unexpected queue name {queueOrTopicName}.");
        }

        public async Task<IReceiverMessageBus> GetSessionReceiverClientAsync(string queueOrTopicName, string sessionId)
        {
            var receiver = await _domainPeekReplyReceiverFactory(sessionId).ConfigureAwait(false);
            return new SimulationReceiverMessageBus(receiver);
        }
    }
}
