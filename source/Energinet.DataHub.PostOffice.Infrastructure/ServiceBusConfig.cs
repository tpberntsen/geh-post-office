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

namespace Energinet.DataHub.PostOffice.Infrastructure
{
    public class ServiceBusConfig
    {
        public const string InboundQueueDataAvailableTopicNameKey = "INBOUND_QUEUE_DATAAVAILABLE_TOPIC_NAME";
        public const string InboundQueueDataAvailableSubscriptionNameKey = "INBOUND_QUEUE_DATAAVAILABLE_SUBSCRIPTION_NAME";
        public const string InboundQueueConnectionStringKey = "INBOUND_QUEUE_CONNECTION_STRING";

        public ServiceBusConfig(string inboundQueueDataAvailableTopicName, string inboundQueueDataAvailableSubscriptionName, string inboundQueueConnectionString)
        {
            if (string.IsNullOrWhiteSpace(inboundQueueDataAvailableTopicName))
                throw new ArgumentException($"{nameof(inboundQueueDataAvailableTopicName)} must be specified in {nameof(ServiceBusConfig)}", nameof(inboundQueueDataAvailableTopicName));

            if (string.IsNullOrWhiteSpace(inboundQueueDataAvailableSubscriptionName))
                throw new ArgumentException($"{nameof(inboundQueueDataAvailableSubscriptionName)} must be specified in {nameof(ServiceBusConfig)}", nameof(inboundQueueDataAvailableSubscriptionName));

            if (string.IsNullOrWhiteSpace(inboundQueueConnectionString))
                throw new ArgumentException($"{nameof(inboundQueueConnectionString)} must be specified in {nameof(ServiceBusConfig)}", nameof(inboundQueueConnectionString));

            InboundQueueDataAvailableTopicName = inboundQueueDataAvailableTopicName;
            InboundQueueDataAvailableSubscriptionName = inboundQueueDataAvailableSubscriptionName;
            InboundQueueConnectionString = inboundQueueConnectionString;
        }

        public string InboundQueueDataAvailableTopicName { get; }
        public string InboundQueueDataAvailableSubscriptionName { get; }
        public string InboundQueueConnectionString { get; }
    }
}
