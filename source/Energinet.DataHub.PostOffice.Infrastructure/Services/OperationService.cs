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
using System.Threading.Tasks;
using Azure.Messaging.ServiceBus;
using Energinet.DataHub.MessageHub.Core.Factories;
using Energinet.DataHub.PostOffice.Domain.Model;
using Energinet.DataHub.PostOffice.Domain.Services;

namespace Energinet.DataHub.PostOffice.Infrastructure.Services
{
    public class OperationService : IOperationService
    {
        private readonly IMessageBusFactory _messageBusFactory;

        public OperationService(IMessageBusFactory messageBusFactory)
        {
            _messageBusFactory = messageBusFactory;
        }

        public async Task TriggerDequeueCleanUpOperationAsync([NotNull] Uuid bundleId)
        {
            var sender = _messageBusFactory.GetSenderClient("sbq-dequeue-cleanup");
            var message = new ServiceBusMessage(bundleId.ToString());
            await sender.PublishMessageAsync<ServiceBusMessage>(message).ConfigureAwait(false);
        }
    }
}
