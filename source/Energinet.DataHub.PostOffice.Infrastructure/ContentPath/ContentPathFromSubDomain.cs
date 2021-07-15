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
using Energinet.DataHub.PostOffice.Domain;

namespace Energinet.DataHub.PostOffice.Infrastructure.ContentPath
{
    public class ContentPathFromSubDomain : IGetContentPathStrategy
    {
        private readonly string? _returnQueueName = Environment.GetEnvironmentVariable("ServiceBus_DataRequest_Return_Queue");
        private readonly ISendMessageToServiceBus _sendMessageToServiceBus;
        private readonly IGetPathToDataFromServiceBus _getPathToDataFromServiceBus;
        private Guid _sessionId;

        public ContentPathFromSubDomain(
            ISendMessageToServiceBus sendMessageToServiceBus,
            IGetPathToDataFromServiceBus getPathToDataFromServiceBus)
        {
            _sendMessageToServiceBus = sendMessageToServiceBus;
            _getPathToDataFromServiceBus = getPathToDataFromServiceBus;
            _sessionId = Guid.NewGuid();
        }

        public string StrategyName { get; } = nameof(ContentPathFromSubDomain);

        public string? SavedContentPath { get; set; }

        public async Task<MessageReply> GetContentPathAsync(RequestData requestData)
        {
            if (requestData == null) throw new ArgumentNullException(nameof(requestData));

            await RequestPathToMarketOperatorDataAsync(requestData).ConfigureAwait(false);
            var messageReply = await ReadPathToMarketOperatorDataAsync().ConfigureAwait(false);

            return messageReply;
        }

        private async Task RequestPathToMarketOperatorDataAsync(RequestData requestData)
        {
            if (requestData is null) throw new ArgumentNullException(nameof(requestData));

            await _sendMessageToServiceBus.RequestDataAsync(
                requestData,
                _sessionId.ToString()).ConfigureAwait(false);
        }

        private async Task<MessageReply> ReadPathToMarketOperatorDataAsync()
        {
            return await _getPathToDataFromServiceBus.GetPathAsync(
                _returnQueueName ?? "default",
                _sessionId.ToString()).ConfigureAwait(false);
        }
    }
}
