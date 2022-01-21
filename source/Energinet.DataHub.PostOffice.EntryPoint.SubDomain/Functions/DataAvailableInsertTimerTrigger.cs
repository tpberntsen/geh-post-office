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

using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Energinet.DataHub.MessageHub.Model.DataAvailable;
using Energinet.DataHub.MessageHub.Model.Exceptions;
using Energinet.DataHub.PostOffice.Application.Commands;
using Energinet.DataHub.PostOffice.Domain.Model;
using MediatR;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.ServiceBus;
using Microsoft.Extensions.Logging;

namespace Energinet.DataHub.PostOffice.EntryPoint.SubDomain.Functions
{
    // TODO: Name
    public class InsertTimerTrigger
    {
        private readonly IMediator _mediator;
        private readonly IDataAvailableMessageReceiver _messageReceiver;
        private readonly IDataAvailableNotificationParser _dataAvailableNotificationParser;

        public InsertTimerTrigger(
            IMediator mediator,
            IDataAvailableMessageReceiver messageReceiver,
            IDataAvailableNotificationParser dataAvailableNotificationParser)
        {
            _mediator = mediator;
            _messageReceiver = messageReceiver;
            _dataAvailableNotificationParser = dataAvailableNotificationParser;
        }

        [Function("InsertTimerTrigger")]
        public async Task RunAsync([TimerTrigger("0 */1 * * * *")] FunctionContext context)
        {
            var logger = context.GetLogger("InsertTimerTrigger");
            logger.LogInformation("Begins processing InsertTimerTrigger.");

            var protobufMessages = await _messageReceiver
                .ReceiveAsync()
                .ConfigureAwait(false);

            var notifications = protobufMessages
                .Select(TryParse)
                .ToList();

            var notificationsPrRecipient = notifications
                .Where(x => x.CouldBeParsed)
                .GroupBy(x => x.Command!.Recipient);

            var tasks = new List<Task<DataAvailableNotificationResponse>>();

            foreach (var group in notificationsPrRecipient)
            {
                var recipientCommand = new DataAvailableNotificationsForRecipientCommand(group.Select(x => x.Command!));
                tasks.Add(_mediator.Send(recipientCommand));
            }

            // TODO: What happens if one task is in error.
            await Task.WhenAll(tasks).ConfigureAwait(false);

            // TODO: Make proper deadletter.
            await _messageReceiver
                .CompleteAsync(protobufMessages)
                .ConfigureAwait(false);

            var newMaximumSequenceNumber = protobufMessages.Max(x => x.SystemProperties.SequenceNumber);
            await _mediator
                .Send(new UpdateMaximumSequenceNumberCommand(new SequenceNumber(newMaximumSequenceNumber)))
                .ConfigureAwait(false);
        }

        private (bool CouldBeParsed, DataAvailableNotificationCommand? Command) TryParse(Message message)
        {
            try
            {
                var parsedValue = _dataAvailableNotificationParser.Parse(message.Body);

                // TODO: Check om Parse kontrollerer obligatoriske felter.
                return (true, new DataAvailableNotificationCommand(
                    parsedValue.Uuid.ToString(),
                    parsedValue.Recipient.Value,
                    parsedValue.MessageType.Value,
                    parsedValue.Origin.ToString(),
                    parsedValue.SupportsBundling,
                    parsedValue.RelativeWeight,
                    message.SystemProperties.SequenceNumber));
            }
            catch (MessageHubException)
            {
                return (false, null);
            }
        }
    }
}
