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
using Energinet.DataHub.MessageHub.Client.Storage;
using Energinet.DataHub.MessageHub.Model.Dequeue;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace GetMessage.Functions
{
    public class ReplyToDequeueFromPostOffice
    {
        private readonly IDequeueNotificationParser _dequeueNotificationParser;
        private readonly IStorageHandler _storageHandler;

        public ReplyToDequeueFromPostOffice(
            IDequeueNotificationParser dequeueNotificationParser,
            IStorageHandler storageHandler)
        {
            _dequeueNotificationParser = dequeueNotificationParser;
            _storageHandler = storageHandler;
        }

        [Function("ReplyToDequeueFromPostOffice")]
        public async Task Run(
            [ServiceBusTrigger(
            "%QueueListenerNameForDequeue%",
            Connection = "ServiceBusConnectionString")]
            byte[] message,
            FunctionContext context)
        {
            var logger = context.GetLogger("ReplyToRequestFromPostOffice");
            logger.LogInformation("C# ServiceBus ReplyToDequeueFromPostOffice triggered");

            try
            {
                var dequeueNotification = _dequeueNotificationParser.Parse(message);
                var dataAvailableNotificationIds = await _storageHandler
                    .GetDataAvailableNotificationIdsAsync(dequeueNotification)
                    .ConfigureAwait(false);

                logger.LogInformation($"Dequeue received for {dequeueNotification.MarketOperator.Value} with notification ids: {string.Join(",", dataAvailableNotificationIds)}");
            }
            catch (Exception e)
            {
                throw new InvalidOperationException("Could not process message.", e);
            }
        }
    }
}
