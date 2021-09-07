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
using Energinet.DataHub.PostOffice.EntryPoint.SubDomain.Parsing;
using Energinet.DataHub.PostOffice.Infrastructure;
using MediatR;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace Energinet.DataHub.PostOffice.EntryPoint.SubDomain.Functions
{
    public class DataAvailableInbox
    {
        private const string FunctionName = "DataAvailableInbox";
        private readonly DataAvailableContractParser _dataAvailableContractParser;
        private readonly IMediator _mediator;

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
                "%" + ServiceBusConfig.InboundQueueDataAvailableTopicNameKey + "%", // TODO: Rename configs
                "%" + ServiceBusConfig.InboundQueueDataAvailableSubscriptionNameKey + "%",
                Connection = ServiceBusConfig.InboundQueueConnectionStringKey)]
            byte[] message,
            FunctionContext context)
        {
            if (message is null)
                throw new ArgumentNullException(nameof(message));

            var logger = context.GetLogger(nameof(DataAvailableInbox));
            logger.LogInformation(
                "C# ServiceBus topic trigger function processed message in {FunctionName}",
                FunctionName);

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
