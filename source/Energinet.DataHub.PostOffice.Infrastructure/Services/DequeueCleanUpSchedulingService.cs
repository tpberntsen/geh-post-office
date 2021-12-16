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

using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using System.Threading.Tasks;
using Azure.Messaging.ServiceBus;
using Energinet.DataHub.MessageHub.Core.Factories;
using Energinet.DataHub.PostOffice.Application.Commands;
using Energinet.DataHub.PostOffice.Domain.Model;
using Energinet.DataHub.PostOffice.Domain.Services;
using Energinet.DataHub.PostOffice.Infrastructure.Configs;

namespace Energinet.DataHub.PostOffice.Infrastructure.Services
{
    public class DequeueCleanUpSchedulingService : IDequeueCleanUpSchedulingService
    {
        private readonly IMessageBusFactory _messageBusFactory;
        private readonly DequeueCleanUpConfig _dequeueCleanUpConfig;

        public DequeueCleanUpSchedulingService(
            IMessageBusFactory messageBusFactory,
            DequeueCleanUpConfig dequeueCleanUpConfig)
        {
            _messageBusFactory = messageBusFactory;
            _dequeueCleanUpConfig = dequeueCleanUpConfig;
        }

        public Task TriggerDequeueCleanUpOperationAsync([NotNull] Bundle bundle)
        {
            var jsonSerializedDequeueCommand = JsonSerializer.Serialize(new DequeueCleanUpCommand(bundle.Recipient.Gln.Value, bundle.BundleId.ToString()));
            var sender = _messageBusFactory.GetSenderClient(_dequeueCleanUpConfig.DequeueCleanUpQueueName);
            var message = new ServiceBusMessage(jsonSerializedDequeueCommand);
            return sender.PublishMessageAsync<ServiceBusMessage>(message);
        }
    }
}
