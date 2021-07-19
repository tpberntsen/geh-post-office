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
        private readonly string? _blobStorageContainerName = Environment.GetEnvironmentVariable("BlobStorageContainerName");
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
            if (getMessagesQuery is null) { throw new ArgumentNullException(nameof(getMessagesQuery)); }

            var requestData = await _dataAvailableController.GetCurrentDataAvailableRequestSetAsync(getMessagesQuery).ConfigureAwait(false);

            if (requestData.Uuids.Any() is false)
            {
                return string.Empty;
            }

            var messageReply = await GetContentPathAsync(requestData).ConfigureAwait(false);

            var data = await GetMarketOperatorDataAsync(messageReply.DataPath ?? string.Empty).ConfigureAwait(false);

            await _dataAvailableController.AddToMessageReplyStorageAsync(messageReply).ConfigureAwait(false);

            return data;
        }

        private async Task<MessageReply> GetContentPathAsync(RequestData requestData)
        {
            var contentPathStrategy = await _dataAvailableController
                .GetStrategyForContentPathAsync(requestData)
                .ConfigureAwait(false);

            var messageReply = await contentPathStrategy
                .GetContentPathAsync(requestData)
                .ConfigureAwait(false);

            return messageReply;
        }

        private async Task<string> GetMarketOperatorDataAsync(string contentPath)
        {
            var contentPathUri = new Uri(contentPath);
            var filename = contentPathUri.Segments[^1];

            return await _storageService.GetStorageContentAsync(
                _blobStorageContainerName,
                filename).ConfigureAwait(false);
        }
    }
}
