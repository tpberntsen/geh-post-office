﻿// Copyright 2020 Energinet DataHub A/S
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
        public const string DataAvailableQueueNameKey = "DATAAVAILABLE_QUEUE_NAME";
        public const string DataAvailableCleanUpQueueNameKey = "DATAAVAILABLE_CLEANUP_QUEUE_NAME";
        public const string DataAvailableQueueConnectionStringKey = "DATAAVAILABLE_QUEUE_CONNECTION_STRING";

        public ServiceBusConfig(string dataAvailableQueueName, string dataAvailableCleanUpQueueName, string dataAvailableQueueConnectionString)
        {
            if (string.IsNullOrWhiteSpace(dataAvailableQueueName))
                throw new ArgumentException($"{nameof(dataAvailableQueueName)} must be specified in {nameof(ServiceBusConfig)}", nameof(dataAvailableQueueName));

            if (string.IsNullOrWhiteSpace(dataAvailableCleanUpQueueName))
                throw new ArgumentException($"{nameof(dataAvailableCleanUpQueueName)} must be specified in {nameof(ServiceBusConfig)}", nameof(dataAvailableCleanUpQueueName));

            if (string.IsNullOrWhiteSpace(dataAvailableQueueConnectionString))
                throw new ArgumentException($"{nameof(dataAvailableQueueConnectionString)} must be specified in {nameof(ServiceBusConfig)}", nameof(dataAvailableQueueConnectionString));

            DataAvailableQueueName = dataAvailableQueueName;
            DataAvailableCleanUpQueueName = dataAvailableCleanUpQueueName;
            DataAvailableQueueConnectionString = dataAvailableQueueConnectionString;
        }

        public string DataAvailableQueueName { get; }
        public string DataAvailableCleanUpQueueName { get; }
        public string DataAvailableQueueConnectionString { get; }
    }
}
