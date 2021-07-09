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
using Energinet.DataHub.PostOffice.Domain;
using MediatR;

namespace Energinet.DataHub.PostOffice.Application.GetMessage.Handlers
{
    public class GetMessageHandler : IRequestHandler<GetMessageQuery, string>
    {
        private readonly string _blobStorageFileName = "Test.txt";
        private readonly string? _blobStorageContainerName = Environment.GetEnvironmentVariable("BlobStorageContainerName");
        private readonly string? _returnQueueName = Environment.GetEnvironmentVariable("ServiceBus_DataRequest_Return_Queue");
        private readonly IDataAvailableStorageService _dataAvailableStorageService;
        private readonly ISendMessageToServiceBus _sendMessageToServiceBus;
        private readonly IGetPathToDataFromServiceBus _getMessageReplyDataFromServiceBus;
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
            _getMessageReplyDataFromServiceBus = getPathToDataFromServiceBus;
            _storageService = storageService;
            _sessionId = Guid.NewGuid();
        }

        public async Task<string> Handle(GetMessageQuery request, CancellationToken cancellationToken)
        {
            if (request is null)
            {
                throw new ArgumentNullException(nameof(request));
            }

            var requestData = await _dataAvailableStorageService.GetDataAvailableUuidsAsync(request.Recipient).ConfigureAwait(false);

            if (request.Recipient is not { Length: > 0 }) return string.Empty;

            await RequestPathToMarketOperatorDataAsync(requestData).ConfigureAwait(false);

            var path = await ReadPathToMarketOperatorDataAsync().ConfigureAwait(false);

            var data = await GetMarketOperatorDataAsync(path).ConfigureAwait(false);

            return data;
        }

        private async Task RequestPathToMarketOperatorDataAsync(RequestData requestData)
        {
            if (requestData == null) throw new ArgumentNullException(nameof(requestData));

            await _sendMessageToServiceBus.RequestDataAsync(
                requestData,
                _sessionId.ToString()).ConfigureAwait(false);
        }

        private async Task<string?> ReadPathToMarketOperatorDataAsync()
        {
            var replyData = await _getMessageReplyDataFromServiceBus.GetPathAsync(
                _returnQueueName ?? "default",
                _sessionId.ToString()).ConfigureAwait(false);

            return replyData.DataPath;
        }

        private async Task<string> GetMarketOperatorDataAsync(string? path)
        {
            if (path is null) throw new ArgumentNullException(nameof(path));

            // Todo: change '_blobStorageFileName' to 'path' when 'ReadPathToMarketOperatorDataAsync()' actually returns a path.
            return await _storageService.GetStorageContentAsync(
                _blobStorageContainerName,
                _blobStorageFileName).ConfigureAwait(false);
        }
    }
}
