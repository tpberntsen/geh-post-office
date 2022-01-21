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
using Energinet.DataHub.PostOffice.Application.Commands;
using Energinet.DataHub.PostOffice.Domain.Model;
using Energinet.DataHub.PostOffice.Utilities;
using MediatR;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.ServiceBus;
using Microsoft.Extensions.Logging;

namespace Energinet.DataHub.PostOffice.EntryPoint.SubDomain.Functions
{
    public class DataAvailableTimerTrigger
    {
        private readonly IMediator _mediator;
        private readonly IDataAvailableMessageReceiver _messageReceiver;
        private readonly IDataAvailableNotificationParser _dataAvailableNotificationParser;

        public DataAvailableTimerTrigger(
            IMediator mediator,
            IDataAvailableMessageReceiver messageReceiver,
            IDataAvailableNotificationParser dataAvailableNotificationParser)
        {
            _mediator = mediator;
            _messageReceiver = messageReceiver;
            _dataAvailableNotificationParser = dataAvailableNotificationParser;
        }

        [Function(nameof(DataAvailableTimerTrigger))]
        public async Task RunAsync([TimerTrigger("0 */1 * * * *")] FunctionContext context)
        {
            var logger = context.GetLogger<DataAvailableTimerTrigger>();
            logger.LogInformation("Begins processing DataAvailableTimerTrigger.");

            var protobufMessages = await _messageReceiver
                .ReceiveAsync()
                .ConfigureAwait(false);

            var notifications = protobufMessages
                .Select(TryParse)
                .ToList();

            var commandTasks = notifications
                .Where(x => x.CouldBeParsed)
                .GroupBy(x => x.Value!.Recipient)
                .Select(async grouping =>
                {
                    try
                    {
                        var items = grouping.Select(x => x.Value!);
                        var request = new InsertDataAvailableNotificationsCommand(items);

                        await _mediator.Send(request).ConfigureAwait(false);
                        return new { grouping, deadletter = false };
                    }
#pragma warning disable CA1031 // Do not catch general exception types
                    catch
#pragma warning restore CA1031 // Do not catch general exception types
                    {
                        return new { grouping, deadletter = true };
                    }
                });

            var complete = new List<Message>();
            var deadletter = new List<Message>();

            foreach (var commandTask in commandTasks)
            {
                var result = await commandTask.ConfigureAwait(false);
                if (result.deadletter)
                {
                    deadletter.AddRange(result.grouping.Select(x => x.Message));
                }
                else
                {
                    complete.AddRange(result.grouping.Select(x => x.Message));
                }
            }

            deadletter.AddRange(notifications.Where(x => !x.CouldBeParsed).Select(x => x.Message));

            await _messageReceiver.CompleteAsync(complete).ConfigureAwait(false);
            await _messageReceiver.DeadLetterAsync(deadletter).ConfigureAwait(false);

            var newMaximumSequenceNumber = protobufMessages.Max(GetSequenceNumber);
            await _mediator
                .Send(new UpdateMaximumSequenceNumberCommand(new SequenceNumber(newMaximumSequenceNumber)))
                .ConfigureAwait(false);
        }

        protected virtual long GetSequenceNumber(Message message)
        {
            Guard.ThrowIfNull(message, nameof(message));
            return message.SystemProperties.SequenceNumber;
        }

        private (Message Message, bool CouldBeParsed, DataAvailableNotificationDto? Value) TryParse(Message message)
        {
            try
            {
                var parsedValue = _dataAvailableNotificationParser.Parse(message.Body);
                return (message, true, new DataAvailableNotificationDto(
                    parsedValue.Uuid.ToString(),
                    parsedValue.Recipient.Value,
                    parsedValue.MessageType.Value,
                    parsedValue.Origin.ToString(),
                    parsedValue.SupportsBundling,
                    parsedValue.RelativeWeight,
                    GetSequenceNumber(message)));
            }
#pragma warning disable CA1031 // Do not catch general exception types
            catch
#pragma warning restore CA1031 // Do not catch general exception types
            {
                return (message, false, null);
            }
        }
    }
}
