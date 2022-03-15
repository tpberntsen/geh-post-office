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
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Energinet.DataHub.MessageHub.Model.DataAvailable;
using Energinet.DataHub.PostOffice.Application.Commands;
using Energinet.DataHub.PostOffice.Utilities;
using MediatR;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.ServiceBus;
using Microsoft.Extensions.Logging;

namespace Energinet.DataHub.PostOffice.EntryPoint.SubDomain.Functions
{
    public class DataAvailableTimerTrigger
    {
        private const string FunctionName = nameof(DataAvailableTimerTrigger);

        private readonly ILogger<DataAvailableTimerTrigger> _logger;
        private readonly IMediator _mediator;
        private readonly IDataAvailableMessageReceiver _messageReceiver;
        private readonly IDataAvailableNotificationParser _dataAvailableNotificationParser;

        public DataAvailableTimerTrigger(
            ILogger<DataAvailableTimerTrigger> logger,
            IMediator mediator,
            IDataAvailableMessageReceiver messageReceiver,
            IDataAvailableNotificationParser dataAvailableNotificationParser)
        {
            _logger = logger;
            _mediator = mediator;
            _messageReceiver = messageReceiver;
            _dataAvailableNotificationParser = dataAvailableNotificationParser;
        }

        [Function(FunctionName)]
#pragma warning disable CA1801
        public async Task RunAsync([TimerTrigger("*/5 * * * * *")] FunctionContext context)
#pragma warning restore CA1801
        {
            _logger.LogInformation("Begins processing DataAvailableTimerTrigger.");

            var messages = await _messageReceiver
                .ReceiveAsync()
                .ConfigureAwait(false);

            _logger.LogInformation("Received a batch of size {0}.", messages.Count);

            if (messages.Count == 0)
                return;

            var sw = Stopwatch.StartNew();

            var internalSequenceNumber = await _mediator
                .Send(new GetMaximumSequenceNumberCommand())
                .ConfigureAwait(false);

            var complete = new List<Message>();
            var deadletter = new List<Message>();

            await ProcessMessagesAsync(
                messages,
                internalSequenceNumber + 1,
                complete,
                deadletter).ConfigureAwait(false);

            var newMaximumSequenceNumber = internalSequenceNumber + messages.Count;
            await _mediator
                .Send(new UpdateMaximumSequenceNumberCommand(newMaximumSequenceNumber))
                .ConfigureAwait(false);

            _logger.LogInformation("Ready to complete messages after {0} ms.", sw.ElapsedMilliseconds);

            await _messageReceiver.CompleteAsync(complete).ConfigureAwait(false);
            await _messageReceiver.DeadLetterAsync(deadletter).ConfigureAwait(false);
        }

        protected virtual long GetSequenceNumber(Message message)
        {
            Guard.ThrowIfNull(message, nameof(message));
            return message.SystemProperties.SequenceNumber;
        }

        private async Task ProcessMessagesAsync(
            IEnumerable<Message> messages,
            long sequenceNumberOffset,
            List<Message> complete,
            List<Message> deadletter)
        {
            var notifications = messages
                .Select((m, i) => TryParse(m, sequenceNumberOffset, i))
                .ToList();

            var commandTasks = notifications
                .Where(x => x.CouldBeParsed)
                .GroupBy(x => x.Value!.Recipient)
                .Select(async grouping =>
                {
                    try
                    {
                        await ProcessGroupAsync(grouping.Key, grouping.Select(x => x.Value!)).ConfigureAwait(false);
                        return new { grouping, deadletter = false };
                    }
#pragma warning disable CA1031 // Do not catch general exception types
                    catch (Exception ex)
#pragma warning restore CA1031 // Do not catch general exception types
                    {
                        _logger.LogWarning(
                            "{0} will be deadletted ({1} messages).\nReason:\n{2}",
                            grouping.Key,
                            grouping.Count(),
                            ex);

                        Log(error: true, grouping.Select(x => (x.Message, x.Value)));

                        return new { grouping, deadletter = true };
                    }
                });

            foreach (var commandTask in commandTasks.ToList())
            {
                var result = await commandTask.ConfigureAwait(false);
                if (result.deadletter)
                {
                    deadletter.AddRange(result.grouping.Select(x => x.Message));
                }
                else
                {
                    complete.AddRange(result.grouping.Select(x => x.Message));
                    Log(error: false, result.grouping.Select(x => (x.Message, x.Value)));
                }
            }

            deadletter.AddRange(notifications.Where(x => !x.CouldBeParsed).Select(x => x.Message));

            void Log(bool error, IEnumerable<(Message message, DataAvailableNotificationDto? da)> das)
            {
                foreach (var (message, da) in das)
                {
                    _logger.LogInformation(
                        "EntryPoint=DataAvailableTimerTrigger;Status={0};CorrelationID={1};DataAvailableId={2};Domain={3};Gln={4}",
                        error ? "Failed" : "Success",
                        message.CorrelationId,
                        da?.Uuid,
                        da?.Origin,
                        da?.Recipient);
                }
            }
        }

        private async Task ProcessGroupAsync(string key, IEnumerable<DataAvailableNotificationDto> group)
        {
            var items = group.ToList();
            var retry = 0;

            while (true)
            {
                try
                {
                    var request = new InsertDataAvailableNotificationsCommand(items);
                    await _mediator.Send(request).ConfigureAwait(false);
                    return;
                }
                catch (CosmosException ex) when (ex.StatusCode == HttpStatusCode.TooManyRequests)
                {
                    if (++retry == 3)
                    {
                        throw;
                    }

                    _logger.LogWarning("{0} will be retried ({1} messages).\nReason:\nTMR", key, items.Count);
                    await Task.Delay(1500).ConfigureAwait(false);
                }
            }
        }

        private (Message Message, bool CouldBeParsed, DataAvailableNotificationDto? Value) TryParse(
            Message message,
            long initialSequenceNumber,
            long sequenceNumberOffset)
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
                    initialSequenceNumber + sequenceNumberOffset,
                    string.IsNullOrWhiteSpace(parsedValue.DocumentType) ? parsedValue.MessageType.Value : parsedValue.DocumentType));
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
