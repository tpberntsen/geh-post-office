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
using System.Threading;
using System.Threading.Tasks;
using MediatR;

namespace Energinet.DataHub.PostOffice.Application.GetMessage
{
    public class GetMessageHandler : IRequestHandler<GetMessageQuery, string>
    {
        private readonly ICosmosService _cosmosService;
        private readonly ISendMessageToServiceBus _sendMessageToServiceBus;
        private readonly IGetPathToDataFromServiceBus _getPathToDataFromServiceBus;
        private readonly IBlobStorageService _blobStorageService;
        private Guid _sessionId;

        public GetMessageHandler(
            ICosmosService cosmosService,
            ISendMessageToServiceBus sendMessageToServiceBus,
            IGetPathToDataFromServiceBus getPathToDataFromServiceBus,
            IBlobStorageService blobStorageService)
        {
            _cosmosService = cosmosService;
            _sendMessageToServiceBus = sendMessageToServiceBus;
            _getPathToDataFromServiceBus = getPathToDataFromServiceBus;
            _blobStorageService = blobStorageService;
            _sessionId = Guid.NewGuid();
        }

        public async Task<string> Handle(GetMessageQuery request, CancellationToken cancellationToken)
        {
            if (request is null)
            {
                throw new ArgumentNullException(nameof(request));
            }

            var uuids = await _cosmosService.GetUuidsFromCosmosDatabaseAsync(request.Recipient).ConfigureAwait(false);

            var queueName = "charges";

            await _sendMessageToServiceBus.SendMessageAsync(
                uuids,
                queueName,
                _sessionId.ToString()).ConfigureAwait(false);

            var path = await _getPathToDataFromServiceBus.GetPathAsync(
                queueName,
                _sessionId.ToString()).ConfigureAwait(false);

            var data =
                await _blobStorageService.GetBlobAsync(
                    "test-blobstorage",
                    "Test.txt").ConfigureAwait(false);

            return data;
        }
    }
}
