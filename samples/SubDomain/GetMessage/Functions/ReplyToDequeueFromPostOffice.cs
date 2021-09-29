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
using GreenEnergyHub.PostOffice.Communicator.Dequeue;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace GetMessage.Functions
{
    public class ReplyToDequeueFromPostOffice
    {
        private readonly IDequeueNotificationParser _dequeueNotificationParser;

        public ReplyToDequeueFromPostOffice(IDequeueNotificationParser dequeueNotificationParser)
        {
            _dequeueNotificationParser = dequeueNotificationParser;
        }

        [Function("ReplyToDequeueFromPostOffice")]
        public async Task RunAsync(
            [ServiceBusTrigger(
            "%QueueListenerNameForDequeue%",
            Connection = "ServiceBusConnectionString")]
            byte[] dequeueNotification,
            FunctionContext context)
        {
            var logger = context.GetLogger("ReplyToRequestFromPostOffice");
            logger.LogInformation("C# ServiceBus ReplyToDequeueFromPostOffice triggered");

            try
            {
                var isParsed = _dequeueNotificationParser.TryParse(dequeueNotification, out var dequeueNotificationDto);
                logger.LogInformation($"Dequeue received for {dequeueNotificationDto.Recipient} with notification Ids: {string.Join(",", dequeueNotificationDto.DataAvailableNotificationIds)}");
            }
            catch (Exception e)
            {
                throw new Exception("Could not process message.", e);
            }
        }
    }
}
