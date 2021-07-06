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
using System.Threading;
using System.Threading.Tasks;
using MediatR;

namespace Energinet.DataHub.PostOffice.Application.GetMessage
{
    public class GetMessageHandler : IRequestHandler<GetMessageQuery, string>
    {
        private readonly string _queueName = "charges";
        private readonly string _blobStorageFileName = "Test.txt";
        private readonly string? _blobStorageContainerName = Environment.GetEnvironmentVariable("BlobStorageContainerName");
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

            var uuids = await _cosmosService.GetDataAvailableUuidsAsync(request.Recipient).ConfigureAwait(false);

            await RequestForPathToMarketOperatorDataAsync(uuids).ConfigureAwait(false);

            var path = await ReadPathToMarketOperatorDataAsync().ConfigureAwait(false);

            var data = await GetMarketOperatorDataAsync(path).ConfigureAwait(false);

            return data;
        }

        private async Task RequestForPathToMarketOperatorDataAsync(IList<string>? uuids)
        {
            if (uuids is null) throw new ArgumentNullException(nameof(uuids));

            await _sendMessageToServiceBus.SendMessageAsync(
                uuids,
                _queueName,
                _sessionId.ToString()).ConfigureAwait(false);
        }

        private async Task<string> ReadPathToMarketOperatorDataAsync()
        {
            return await _getPathToDataFromServiceBus.GetPathAsync(
                _queueName,
                _sessionId.ToString()).ConfigureAwait(false);
        }

        private async Task<string> GetMarketOperatorDataAsync(string? path)
        {
            if (path is null) throw new ArgumentNullException(nameof(path));

            // Todo: change '_blobStorageFileName' to 'path' when 'ReadPathToMarketOperatorDataAsync()' actually returns a path.
            return await _blobStorageService.GetBlobAsync(
                _blobStorageContainerName,
                _blobStorageFileName).ConfigureAwait(false);
        }
    }
}
