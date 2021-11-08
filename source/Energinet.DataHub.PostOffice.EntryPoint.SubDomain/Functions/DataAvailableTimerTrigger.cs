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
using System.Linq;
using System.Threading.Tasks;
using Energinet.DataHub.MessageHub.Model.DataAvailable;
using Energinet.DataHub.MessageHub.Model.Model;
using Energinet.DataHub.PostOffice.Application;
using Energinet.DataHub.PostOffice.Application.Commands;
using MediatR;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.ServiceBus;
using Microsoft.Extensions.Logging;

namespace Energinet.DataHub.PostOffice.EntryPoint.SubDomain.Functions
{
    public sealed class DataAvailableTimerTrigger
    {
        private const string FunctionName = nameof(DataAvailableTimerTrigger);

        private readonly IMediator _mediator;
        private readonly IDataAvailableMessageReceiver _messageReceiver;
        private readonly IDataAvailableNotificationParser _dataAvailableNotificationParser;
        private readonly IMapper<DataAvailableNotificationDto, DataAvailableNotificationCommand> _dataAvailableNotificationMapper;

        public DataAvailableTimerTrigger(
            IMediator mediator,
            IDataAvailableMessageReceiver messageReceiver,
            IDataAvailableNotificationParser dataAvailableNotificationParser,
            IMapper<DataAvailableNotificationDto, DataAvailableNotificationCommand> dataAvailableNotificationMapper)
        {
            _mediator = mediator;
            _messageReceiver = messageReceiver;
            _dataAvailableNotificationParser = dataAvailableNotificationParser;
            _dataAvailableNotificationMapper = dataAvailableNotificationMapper;
        }

        [Function(FunctionName)]
        public async Task RunAsync([TimerTrigger("* */1 * * * *")] FunctionContext context)
        {
            var logger = context.GetLogger<DataAvailableInbox>();
            logger.LogInformation("Begins processing DataAvailableNotifications in timer.");

            var messages = await _messageReceiver.ReceiveAsync().ConfigureAwait(false);
            logger.LogInformation("Received a DataAvailableNotification batch of size {0}.", messages.Count);

            await ProcessMessagesAsync(messages).ConfigureAwait(false);
        }

        private async Task ProcessMessagesAsync(IEnumerable<Message> messages)
        {
            var list = messages
                .Select(m => new { Message = m, Task = ProcessMessageAsync(m) })
                .ToList();

            try
            {
                await Task.WhenAll(list.Select(x => x.Task)).ConfigureAwait(false);
            }
            catch (Exception)
            {
                // TODO: Which exceptions to catch?
                var allFaultedMessages = list
                    .Where(x => !x.Task.IsCompletedSuccessfully)
                    .Select(x => x.Message);

                await _messageReceiver.DeadLetterAsync(allFaultedMessages).ConfigureAwait(false);
            }

            var allCompletedMessages = list
                .Where(x => x.Task.IsCompletedSuccessfully)
                .Select(x => x.Message);

            await _messageReceiver.CompleteAsync(allCompletedMessages).ConfigureAwait(false);
        }

        private Task ProcessMessageAsync(Message message)
        {
            try
            {
                var dataAvailableNotification = _dataAvailableNotificationParser.Parse(message.Body);
                var dataAvailableCommand = _dataAvailableNotificationMapper.Map(dataAvailableNotification);
                return _mediator.Send(dataAvailableCommand);
            }
            catch (Exception ex)
            {
                // TODO: Which exceptions to catch?
                return Task.FromException(ex);
            }
        }
    }
}
