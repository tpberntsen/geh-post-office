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
        private readonly string _queueName = "charges";
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

        public async Task<string> GetContentPathAsync(IEnumerable<DataAvailable> dataAvailables)
        {
            await RequestPathToMarketOperatorDataAsync(dataAvailables.Select(d => d.uuid)).ConfigureAwait(false);
            var contentPath = await ReadPathToMarketOperatorDataAsync().ConfigureAwait(false);
            return contentPath;
        }

        private async Task RequestPathToMarketOperatorDataAsync(IEnumerable<string?> uuids)
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
    }
}
