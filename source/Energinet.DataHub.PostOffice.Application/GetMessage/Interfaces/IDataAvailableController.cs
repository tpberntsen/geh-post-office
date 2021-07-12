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

namespace Energinet.DataHub.PostOffice.Application.GetMessage.Interfaces
{
    /// <summary>
    /// DataAvailable Controller
    /// </summary>
    public interface IDataAvailableController
    {
        /// <summary>
        /// Find current request set from recipient calculated from created time and priority
        /// </summary>
        /// <param name="getMessageQuery"></param>
        /// <returns>list of dataAvailables where data should be collected</returns>
        Task<IEnumerable<Domain.DataAvailable>> GetCurrentDataAvailableRequestSetAsync(GetMessageQuery getMessageQuery);

        /// <summary>
        /// Finds data message uuids for recipient.
        /// </summary>
        /// <param name="dataAvailables"></param>
        /// <returns>list of dataAvailables where data should be collected</returns>
        Task<IGetContentPathStrategy> GetStrategyForContentPathAsync(IEnumerable<Domain.DataAvailable> dataAvailables);

        /// <summary>
        /// Saves message Response
        /// </summary>
        /// <param name="dataAvailables"></param>
        /// <param name="contentPath"></param>
        /// <returns>void task</returns>
        Task AddToMessageResponseStorageAsync(IEnumerable<Domain.DataAvailable> dataAvailables, Uri contentPath);
    }
}
