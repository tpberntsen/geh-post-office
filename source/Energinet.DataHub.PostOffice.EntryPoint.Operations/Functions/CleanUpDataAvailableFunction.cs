﻿// Copyright 2020 Energinet DataHub A/S
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
using System.ServiceModel.Channels;
using System.Threading.Tasks;
using Energinet.DataHub.PostOffice.Application.Commands;
using Energinet.DataHub.PostOffice.Domain.Model;
using Energinet.DataHub.PostOffice.Infrastructure;
using MediatR;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace Energinet.DataHub.PostOffice.EntryPoint.Operations.Functions
{
    public class CleanUpDataAvailableFunction
    {
        private const string FunctionName = "CleanUpDataAvailable";

        private readonly IMediator _mediator;

        public CleanUpDataAvailableFunction(IMediator mediator)
        {
            _mediator = mediator;
        }

        [Function(FunctionName)]
        public async Task RunAsync(
            [ServiceBusTrigger(
                "%" + ServiceBusConfig.DataAvailableCleanUpQueueNameKey + "%",
                Connection = ServiceBusConfig.DataAvailableQueueConnectionStringKey)]
            Message message,
            FunctionContext context)
        {
            if (message is null)
                throw new ArgumentNullException(nameof(message));

            var logger = context.GetLogger<CleanUpDataAvailableFunction>();
            logger.LogInformation($"C# ServiceBus queue trigger function processed message in {FunctionName}");

            try
            {
                var command = new DataAvailableCleanUpCommand(new Uuid(message.GetBody<string>()));
                await _mediator.Send(command).ConfigureAwait(false);
            }
            catch (Exception exception)
            {
                logger.LogError(exception, "Error in {FunctionName}", FunctionName);
                throw;
            }
        }
    }
}