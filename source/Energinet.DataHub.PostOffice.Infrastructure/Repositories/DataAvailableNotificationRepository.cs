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

using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Energinet.DataHub.PostOffice.Domain.Model;
using Energinet.DataHub.PostOffice.Domain.Repositories;

namespace Energinet.DataHub.PostOffice.Infrastructure.Repositories
{
    // todo : correct implementation #137
    public class DataAvailableNotificationRepository : IDataAvailableNotificationRepository
    {
        public Task CreateAsync(DataAvailableNotification dataAvailableNotification)
        {
            return Task.CompletedTask;
        }

        public Task<IEnumerable<DataAvailableNotification>> PeekAsync(Recipient recipient, MessageType messageType)
        {
            return Task.FromResult(Enumerable.Empty<DataAvailableNotification>());
        }

        public Task<DataAvailableNotification?> PeekAsync(Recipient recipient)
        {
            return Task.FromResult<DataAvailableNotification?>(null);
        }

        public Task DequeueAsync(IEnumerable<Uuid> ids)
        {
            return Task.CompletedTask;
        }
    }
}
