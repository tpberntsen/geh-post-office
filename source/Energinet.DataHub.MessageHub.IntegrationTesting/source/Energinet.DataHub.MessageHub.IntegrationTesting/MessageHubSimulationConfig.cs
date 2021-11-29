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
using Energinet.DataHub.MessageHub.Core;

namespace Energinet.DataHub.MessageHub.IntegrationTesting
{
    public class MessageHubSimulationConfig
    {
        /// <param name="serviceBusReadWriteConnectionString">The service bus connection string to use for the simulation. The connection string must have read and write access rights.</param>
        /// <param name="dataAvailableQueueName">The 'sbq-dataavailable' queue name.</param>
        /// <param name="domainQueueName">The 'sbq-[domain]' queue name.</param>
        /// <param name="domainReplyQueueName">The 'sbq-[domain]-reply' queue name.</param>
        /// <param name="domainDequeueQueueName">The 'sbq-[domain]-dequeue' queue name.</param>
        /// <param name="blobStorageConnectionString">The connection string to the Blob Storage used to store the generated bundles.</param>
        /// <param name="blobStorageContainerName">The container name of the Blob Storage used to store the generated bundles.</param>
        public MessageHubSimulationConfig(
            string serviceBusReadWriteConnectionString,
            string dataAvailableQueueName,
            string domainQueueName,
            string domainReplyQueueName,
            string domainDequeueQueueName,
            string blobStorageConnectionString,
            string blobStorageContainerName)
        {
            ServiceBusReadWriteConnectionString = serviceBusReadWriteConnectionString;
            DataAvailableQueueName = dataAvailableQueueName;
            DomainQueueName = domainQueueName;
            DomainReplyQueueName = domainReplyQueueName;
            DomainDequeueQueueName = domainDequeueQueueName;
            BlobStorageConnectionString = blobStorageConnectionString;
            BlobStorageContainerName = blobStorageContainerName;
        }

        /// <param name="serviceBusReadWriteConnectionString">The service bus connection string to use for the simulation. The connection string must have read and write access rights.</param>
        /// <param name="dataAvailableQueueName">The 'sbq-dataavailable' queue name.</param>
        /// <param name="domainQueueName">The 'sbq-[domain]' queue name.</param>
        /// <param name="domainReplyQueueName">The 'sbq-[domain]-reply' queue name.</param>
        /// <param name="blobStorageConnectionString">The connection string to the Blob Storage used to store the generated bundles.</param>
        /// <param name="blobStorageContainerName">The container name of the Blob Storage used to store the generated bundles.</param>
        public MessageHubSimulationConfig(
            string serviceBusReadWriteConnectionString,
            string dataAvailableQueueName,
            string domainQueueName,
            string domainReplyQueueName,
            string blobStorageConnectionString,
            string blobStorageContainerName)
        {
            ServiceBusReadWriteConnectionString = serviceBusReadWriteConnectionString;
            DataAvailableQueueName = dataAvailableQueueName;
            DomainQueueName = domainQueueName;
            DomainReplyQueueName = domainReplyQueueName;
            BlobStorageConnectionString = blobStorageConnectionString;
            BlobStorageContainerName = blobStorageContainerName;
            DomainDequeueQueueName = null;
        }

        /// <summary>
        /// The service bus connection string to use for the simulation. The connection string must have read and write access rights.
        /// </summary>
        public string ServiceBusReadWriteConnectionString { get; }

        /// <summary>
        /// The 'sbq-dataavailable' queue name.
        /// </summary>
        public string DataAvailableQueueName { get; }

        /// <summary>
        /// The 'sbq-[domain]' queue name.
        /// </summary>
        public string DomainQueueName { get; }

        /// <summary>
        /// The 'sbq-[domain]-reply' queue name.
        /// </summary>
        public string DomainReplyQueueName { get; }

        /// <summary>
        /// The 'sbq-[domain]-dequeue' queue name.
        /// </summary>
        public string? DomainDequeueQueueName { get; }

        /// <summary>
        /// The connection string to the Blob Storage used to store the generated bundles.
        /// </summary>
        public string BlobStorageConnectionString { get; }

        /// <summary>
        /// The container name of the Blob Storage used to store the generated bundles.
        /// </summary>
        public string BlobStorageContainerName { get; }

        /// <summary>
        /// Specifies the timeout to use with WaitForNotificationsInDataAvailableQueueAsync.
        /// Default is 15 seconds.
        /// </summary>
        public TimeSpan WaitTimeout { get; set; } = TimeSpan.FromSeconds(15);

        /// <summary>
        /// Specifies the timeout to use with PeekAsync.
        /// Default is 15 seconds.
        /// </summary>
        public TimeSpan PeekTimeout { get; set; } = TimeSpan.FromSeconds(15);

        internal PeekRequestConfig CreateSimulatedPeekRequestConfig()
        {
            return new PeekRequestConfig(
                DomainQueueName,
                DomainReplyQueueName,
                DomainQueueName,
                DomainReplyQueueName,
                DomainQueueName,
                DomainReplyQueueName,
                DomainQueueName,
                DomainReplyQueueName,
                DomainQueueName,
                DomainReplyQueueName,
                PeekTimeout);
        }

        internal DequeueConfig CreateSimulatedDequeueConfig()
        {
            return new DequeueConfig(
                DomainDequeueQueueName!,
                DomainDequeueQueueName!,
                DomainDequeueQueueName!,
                DomainDequeueQueueName!,
                DomainDequeueQueueName!);
        }
    }
}
