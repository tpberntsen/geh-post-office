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
using Azure.Messaging.ServiceBus;
using Energinet.DataHub.MessageHub.Model.IntegrationEvents;

namespace Energinet.DataHub.MessageHub.Client.Extensions
{
    internal static class ServiceBusMessageExtensions
    {
        public static ServiceBusMessage AddDataBundleResponseIntegrationEvents(this ServiceBusMessage serviceBusMessage, string operationCorrelationId)
        {
            return serviceBusMessage.AddIntegrationsEvents(
                operationCorrelationId,
                IntegrationEventsMessageType.DataBundleResponse,
                Guid.NewGuid().ToString());
        }

        public static ServiceBusMessage AddDataAvailableIntegrationEvents(this ServiceBusMessage serviceBusMessage, string operationCorrelationId)
        {
            return serviceBusMessage.AddIntegrationsEvents(
                operationCorrelationId,
                IntegrationEventsMessageType.DataAvailable,
                Guid.NewGuid().ToString());
        }

        private static ServiceBusMessage AddIntegrationsEvents(
            this ServiceBusMessage serviceBusMessage,
            string operationCorrelationId,
            IntegrationEventsMessageType messageType,
            string eventIdentification)
        {
            serviceBusMessage.ApplicationProperties.Add("OperationTimestamp", DateTimeOffset.UtcNow);
            serviceBusMessage.ApplicationProperties.Add("OperationCorrelationId", operationCorrelationId);
            serviceBusMessage.ApplicationProperties.Add("MessageVersion", 1);
            serviceBusMessage.ApplicationProperties.Add("MessageType", messageType.ToString());
            serviceBusMessage.ApplicationProperties.Add("EventIdentification", eventIdentification);
            return serviceBusMessage;
        }
    }
}
