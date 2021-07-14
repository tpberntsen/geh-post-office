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
        private readonly IDataAvailableController _dataAvailableController;
        private readonly IStorageService _storageService;

        public GetMessageHandler(
            IDataAvailableController dataAvailableController,
            IStorageService storageService)
        {
            _dataAvailableController = dataAvailableController;
            _storageService = storageService;
        }

        public async Task<string> Handle(GetMessageQuery getMessagesQuery, CancellationToken cancellationToken)
        {
            if (getMessagesQuery is null)
            {
                throw new ArgumentNullException(nameof(getMessagesQuery));
            }

            var dataAvailableForRecipient = await _dataAvailableController.GetCurrentDataAvailableRequestSetAsync(getMessagesQuery).ConfigureAwait(false);
            var availableForRecipient = dataAvailableForRecipient.ToList();

            var contentPath = await GetContentPathAsync(availableForRecipient).ConfigureAwait(false);

            var data = await GetMarketOperatorDataAsync().ConfigureAwait(false);

            await AddMessageResponseToStorageAsync(availableForRecipient, contentPath).ConfigureAwait(false);

            return data;
        }

        private async Task AddMessageResponseToStorageAsync(RequestData requestData, string? contentPath)
        {
            await _dataAvailableController
                .AddToMessageResponseStorageAsync(requestData, new Uri(contentPath!))
                .ConfigureAwait(false);
        }

        /*
        private async Task RequestPathToMarketOperatorDataAsync(RequestData requestData)
        {
            if (requestData == null) throw new ArgumentNullException(nameof(requestData));

            await _sendMessageToServiceBus.RequestDataAsync(
                requestData,
                _sessionId.ToString()).ConfigureAwait(false);
        }*/

        private async Task<string?> GetContentPathAsync(IReadOnlyCollection<Domain.DataAvailable> availableForRecipient)
        {
            var contentPathStrategy = await _dataAvailableController
                .GetStrategyForContentPathAsync(availableForRecipient)
                .ConfigureAwait(false);

            var contentPath = await contentPathStrategy
                .GetContentPathAsync(availableForRecipient)
                .ConfigureAwait(false);

            return contentPath;
        }

        /*private async Task<string?> ReadPathToMarketOperatorDataAsync()
        {
            var replyData = await _getMessageReplyDataFromServiceBus.GetPathAsync(
                _returnQueueName ?? "default",
                _sessionId.ToString()).ConfigureAwait(false);

            return replyData.DataPath;
        }*/

        private async Task<string> GetMarketOperatorDataAsync()
        {
            // Todo: change '_blobStorageFileName' to the path provided from 'ReadPathToMarketOperatorDataAsync()' when the method actually returns a path.
            return await _storageService.GetStorageContentAsync(
                _blobStorageContainerName,
                _blobStorageFileName).ConfigureAwait(false);
        }
    }
}
