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
using Energinet.DataHub.PostOffice.Application.GetMessage.Interfaces;
using Energinet.DataHub.PostOffice.Application.GetMessage.Queries;
using MediatR;

namespace Energinet.DataHub.PostOffice.Application.GetMessage.Handlers
{
    public class GetMessageHandler : IRequestHandler<GetMessageQuery, string>
    {
        private readonly string _queueName = "charges";
        private readonly string _blobStorageFileName = "Test.txt";
        private readonly string? _blobStorageContainerName = Environment.GetEnvironmentVariable("BlobStorageContainerName");
        private readonly IDataAvailableStorageService _dataAvailableStorageService;
        private readonly ISendMessageToServiceBus _sendMessageToServiceBus;
        private readonly IGetPathToDataFromServiceBus _getPathToDataFromServiceBus;
        private readonly IStorageService _storageService;
        private Guid _sessionId;

        public GetMessageHandler(
            IDataAvailableStorageService dataAvailableStorageService,
            ISendMessageToServiceBus sendMessageToServiceBus,
            IGetPathToDataFromServiceBus getPathToDataFromServiceBus,
            IStorageService storageService)
        {
            _dataAvailableStorageService = dataAvailableStorageService;
            _sendMessageToServiceBus = sendMessageToServiceBus;
            _getPathToDataFromServiceBus = getPathToDataFromServiceBus;
            _storageService = storageService;
            _sessionId = Guid.NewGuid();
        }

        public async Task<string> Handle(GetMessageQuery request, CancellationToken cancellationToken)
        {
            if (request is null)
            {
                throw new ArgumentNullException(nameof(request));
            }

            var uuids = await _dataAvailableStorageService.GetDataAvailableUuidsAsync(request).ConfigureAwait(false);

            await RequestPathToMarketOperatorDataAsync(uuids).ConfigureAwait(false);

            var path = await ReadPathToMarketOperatorDataAsync().ConfigureAwait(false);

            var data = await GetMarketOperatorDataAsync().ConfigureAwait(false);

            return data;
        }

        private async Task RequestPathToMarketOperatorDataAsync(IList<string> uuids)
        {
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

        private async Task<string> GetMarketOperatorDataAsync()
        {
            // Todo: change '_blobStorageFileName' to the path provided from 'ReadPathToMarketOperatorDataAsync()' when the method actually returns a path.
            return await _storageService.GetStorageContentAsync(
                _blobStorageContainerName,
                _blobStorageFileName).ConfigureAwait(false);
        }
    }
}
