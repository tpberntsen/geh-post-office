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
using Energinet.DataHub.PostOffice.Application;
using Energinet.DataHub.PostOffice.Application.DataAvailable;
using Energinet.DataHub.PostOffice.Inbound.Parsing;
using MediatR;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.ServiceBus;
using Microsoft.Extensions.Logging;

namespace Energinet.DataHub.PostOffice.Inbound.Functions
{
    public class DataAvailableInbox
    {
        private const string FunctionName = "DataAvailableInbox";

        private readonly IMediator _mediator;
        private readonly DataAvailableContractParser _dataAvailableContractParser;

        public DataAvailableInbox(
            IMediator mediator,
            DataAvailableContractParser dataAvailableContractParser)
        {
            _mediator = mediator;
            _dataAvailableContractParser = dataAvailableContractParser;
        }

        [Function(FunctionName)]
        public async Task RunAsync(
            [ServiceBusTrigger(
                "%INBOUND_QUEUE_DATAAVAILABLE_TOPIC_NAME%",
                "%INBOUND_QUEUE_DATAAVAILABLE_SUBSCRIPTION_NAME%",
                Connection = "INBOUND_QUEUE_CONNECTION_STRING")]
            byte[] message,
            FunctionContext context)
        {
            if (message is null) throw new ArgumentNullException(nameof(message));

            var topicName = Environment.GetEnvironmentVariable("INBOUND_QUEUE_DATAAVAILABLE_TOPIC_NAME");
            if (string.IsNullOrEmpty(topicName)) throw new InvalidOperationException("TopicName is null");

            var logger = context.GetLogger(nameof(DataAvailableInbox));
            logger.LogInformation(message: "C# ServiceBus topic trigger function processed message in {FunctionName}", FunctionName);

            try
            {
                var dataAvailableCommand = _dataAvailableContractParser.Parse(message);
                await _mediator.Send(dataAvailableCommand).ConfigureAwait(false);
            }
            catch (Exception exception)
            {
                logger.LogError(exception, "Error in {FunctionName}", FunctionName);
                throw;
            }
        }
    }
}
