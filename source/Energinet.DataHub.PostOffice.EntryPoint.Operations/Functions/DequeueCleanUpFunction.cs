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
using System.Text.Json;
using System.Threading.Tasks;
using Energinet.DataHub.PostOffice.Application.Commands;
using Energinet.DataHub.PostOffice.Infrastructure;
using Energinet.DataHub.PostOffice.Infrastructure.Correlation;
using MediatR;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace Energinet.DataHub.PostOffice.EntryPoint.Operations.Functions
{
    public class DequeueCleanUpFunction
    {
        private const string FunctionName = "DequeueCleanUp";

        private readonly IMediator _mediator;
        private readonly ILogCallback _logCallback;

        public DequeueCleanUpFunction(IMediator mediator, ILogCallback logCallback)
        {
            _mediator = mediator;
            _logCallback = logCallback;
        }

        [Function(FunctionName)]
        public async Task RunAsync(
            [ServiceBusTrigger(
                "%" + ServiceBusConfig.DequeueCleanUpQueueNameKey + "%",
                Connection = "ServiceBusConnectionString")]
            string message,
            FunctionContext context)
        {
            if (message is null)
                throw new ArgumentNullException(nameof(message));

            var logger = context.GetLogger<DequeueCleanUpFunction>();
            logger.LogInformation($"C# ServiceBus queue trigger function processed message in {FunctionName}");

            _logCallback.SetCallback(x => logger.LogWarning(x));

            try
            {
                var command = JsonSerializer.Deserialize<DequeueCleanUpCommand>(message);
                var operationResponse = await _mediator.Send(command!).ConfigureAwait(false);

                if (!operationResponse.Completed)
                    logger.LogError("Dequeue cleanup operation dit not complete successfully");
            }
            catch (Exception exception)
            {
                logger.LogError(exception, "Error in {FunctionName}, message: {message}", FunctionName, message);
                throw;
            }
        }
    }
}
