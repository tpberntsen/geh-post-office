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
using System.Net;
using System.Threading.Tasks;
using Azure.Messaging.ServiceBus;
using Energinet.DataHub.PostOffice.Application.GetMessage;
using Microsoft.Azure.Functions.Worker.Http;

namespace Energinet.DataHub.PostOffice.Infrastructure.GetMessage
{
    public class GetPathToDataFromServiceBus : IGetPathToDataFromServiceBus
    {
        private ServiceBusClient _serviceBusClient;

        public GetPathToDataFromServiceBus(ServiceBusClient serviceBusClient)
        {
            _serviceBusClient = serviceBusClient;
        }

        public async Task<string> GetPathAsync(string queueName, string sessionId)
        {
            var receiver = await _serviceBusClient.AcceptSessionAsync(queueName, sessionId).ConfigureAwait(false);

            var received = await receiver.ReceiveMessageAsync(TimeSpan.FromSeconds(3)).ConfigureAwait(false);

            return received.ToString();
        }
    }
}
