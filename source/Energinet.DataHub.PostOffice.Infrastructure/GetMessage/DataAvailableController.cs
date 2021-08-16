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
using System.Threading.Tasks;
using Energinet.DataHub.PostOffice.Application.GetMessage.Interfaces;
using Energinet.DataHub.PostOffice.Application.GetMessage.Queries;
using Energinet.DataHub.PostOffice.Domain;
using Energinet.DataHub.PostOffice.Domain.Repositories;

namespace Energinet.DataHub.PostOffice.Infrastructure.GetMessage
{
    public class DataAvailableController : IDataAvailableController
    {
        private readonly IDataAvailableRepository _dataAvailableRepository;
        private readonly IMessageReplyRepository _messageReplyRepository;
        private readonly IGetContentPathStrategyFactory _contentPathStrategyFactory;

        public DataAvailableController(
            IDataAvailableRepository dataAvailableRepository,
            IMessageReplyRepository messageReplyRepository,
            IGetContentPathStrategyFactory contentPathStrategyFactory)
        {
            _dataAvailableRepository = dataAvailableRepository;
            _messageReplyRepository = messageReplyRepository;
            _contentPathStrategyFactory = contentPathStrategyFactory;
        }

        public async Task<RequestData> GetCurrentDataAvailableRequestSetAsync(GetMessageQuery getMessageQuery)
        {
            if (getMessageQuery is null)
                throw new ArgumentNullException(nameof(getMessageQuery));

            var dataAvailablesByRecipient = await _dataAvailableRepository.GetDataAvailableUuidsAsync(getMessageQuery.Recipient).ConfigureAwait(false);
            return dataAvailablesByRecipient;
        }

        public async Task<IGetContentPathStrategy> GetStrategyForContentPathAsync(RequestData requestData)
        {
            if (requestData is null) throw new ArgumentNullException(nameof(requestData));

            var dataAvailableContentKey = GetContentKeyFromUuids(requestData.Uuids);
            var savedContentPath = await _messageReplyRepository.GetMessageReplyAsync(dataAvailableContentKey).ConfigureAwait(false);

            return _contentPathStrategyFactory.Create(savedContentPath ?? string.Empty);
        }

        public async Task AddToMessageReplyStorageAsync(MessageReply messageReply)
        {
            if (messageReply is null) throw new ArgumentNullException(nameof(messageReply));

            var messageContentKey = GetContentKeyFromUuids(messageReply.Uuids);

            await _messageReplyRepository.SaveMessageReplyAsync(messageContentKey, new Uri(messageReply.DataPath!)).ConfigureAwait(false);
        }

        private static string GetContentKeyFromUuids(IEnumerable<string> uuids)
        {
            return string.Join(";", uuids);
        }
    }
}
