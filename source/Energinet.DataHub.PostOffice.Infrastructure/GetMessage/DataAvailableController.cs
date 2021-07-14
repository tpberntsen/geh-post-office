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
using System.Threading.Tasks;
using Energinet.DataHub.PostOffice.Application.GetMessage.Interfaces;
using Energinet.DataHub.PostOffice.Application.GetMessage.Queries;
using Energinet.DataHub.PostOffice.Domain;

namespace Energinet.DataHub.PostOffice.Infrastructure.GetMessage
{
    public class DataAvailableController : IDataAvailableController
    {
        private readonly IDataAvailableStorageService _dataAvailableStorageService;
        private readonly IMessageResponseStorage _messageResponseStorage;
        private readonly IGetContentPathStrategyFactory _contentPathStrategyFactory;

        public DataAvailableController(
            IDataAvailableStorageService dataAvailableStorageService,
            IMessageResponseStorage messageResponseStorage,
            IGetContentPathStrategyFactory contentPathStrategyFactory)
        {
            _dataAvailableStorageService = dataAvailableStorageService;
            _messageResponseStorage = messageResponseStorage;
            _contentPathStrategyFactory = contentPathStrategyFactory;
        }

        public async Task<RequestData> GetCurrentDataAvailableRequestSetAsync(GetMessageQuery getMessageQuery)
        {
            var dataAvailablesByRecipient = await _dataAvailableStorageService.GetDataAvailableUuidsAsync(getMessageQuery).ConfigureAwait(false);
            return dataAvailablesByRecipient;
        }

        public async Task<IGetContentPathStrategy> GetStrategyForContentPathAsync(RequestData requestData)
        {
            if (requestData == null) throw new ArgumentNullException(nameof(requestData));

            var dataAvailableContentKey = GetContentKeyFromUuids(requestData);
            var savedContentPath = await _messageResponseStorage.GetMessageResponseAsync(dataAvailableContentKey).ConfigureAwait(false);

            return _contentPathStrategyFactory.Create(savedContentPath ?? string.Empty);
        }

        public async Task AddToMessageResponseStorageAsync(RequestData requestData, Uri contentPath)
        {
            if (requestData == null) throw new ArgumentNullException(nameof(requestData));

            var messageContentKey = GetContentKeyFromUuids(requestData);

            await _messageResponseStorage.SaveMessageResponseAsync(messageContentKey, contentPath).ConfigureAwait(false);
        }

        private static string GetContentKeyFromUuids(RequestData requestData)
        {
            return string.Join(";", requestData.Uuids);
        }
    }
}
