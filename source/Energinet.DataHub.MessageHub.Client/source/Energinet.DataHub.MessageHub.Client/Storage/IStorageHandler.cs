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
using System.IO;
using System.Threading.Tasks;
using Energinet.DataHub.MessageHub.Model.Model;

namespace Energinet.DataHub.MessageHub.Client.Storage
{
    /// <summary>
    /// Provides access to shared blob storage between MessageHub and sub-domains.
    /// </summary>
    public interface IStorageHandler
    {
        /// <summary>
        /// Stores a filestream in the MessageHub storage, and returns a path to the stored file,
        /// that is to be used when sending a response to the MessageHub.
        /// </summary>
        /// <param name="stream">A stream containing the contents that should be stored</param>
        /// <param name="requestDto">The domain that is sending the data</param>
        /// <returns>A string containing the path of the stored file</returns>
        Task<Uri> AddStreamToStorageAsync(Stream stream, DataBundleRequestDto requestDto);

        /// <summary>
        /// Gets a list of DataAvailableNotification ids for the bundle request from MessageHub.
        /// </summary>
        /// <param name="bundleRequest">The bundle request to get the ids from.</param>
        /// <returns>A list of DataAvailableNotification ids.</returns>
        Task<IReadOnlyList<Guid>> GetDataAvailableNotificationIdsAsync(DataBundleRequestDto bundleRequest);

        /// <summary>
        /// Gets a list of DataAvailableNotification ids for the dequeue notification from MessageHub.
        /// </summary>
        /// <param name="dequeueNotification">The dequeue notification to get the ids from.</param>
        /// <returns>A list of DataAvailableNotification ids.</returns>
        Task<IReadOnlyList<Guid>> GetDataAvailableNotificationIdsAsync(DequeueNotificationDto dequeueNotification);
    }
}
