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
using Energinet.DataHub.PostOffice.Application.GetMessage.Queries;
using Energinet.DataHub.PostOffice.Domain;

namespace Energinet.DataHub.PostOffice.Application.GetMessage.Interfaces
{
    /// <summary>
    /// DataAvailable Controller
    /// </summary>
    public interface IDataAvailableController
    {
        /// <summary>
        /// Finds current request set from recipient calculated from created time and priority
        /// </summary>
        /// <param name="getMessageQuery"></param>
        /// <returns>list of dataAvailables where data should be collected</returns>
        Task<RequestData> GetCurrentDataAvailableRequestSetAsync(GetMessageQuery getMessageQuery);

        /// <summary>
        /// Finds data message uuids for recipient.
        /// </summary>
        /// <param name="dataAvailables"></param>
        /// <returns>list of dataAvailables where data should be collected</returns>
        Task<IGetContentPathStrategy> GetStrategyForContentPathAsync(RequestData dataAvailables);

        /// <summary>
        /// Saves message reply to storage
        /// </summary>
        /// <param name="messageReply"></param>
        /// <returns>void task</returns>
        Task AddToMessageReplyStorageAsync(MessageReply messageReply);
    }
}
