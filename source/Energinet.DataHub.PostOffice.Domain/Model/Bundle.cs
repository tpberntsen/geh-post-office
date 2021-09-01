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

namespace Energinet.DataHub.PostOffice.Domain.Model
{
    public class Bundle : IBundle
    {
        private readonly Func<Task<Stream>> _getStream;
        public Bundle(Uuid bundleId, IEnumerable<Uuid> notificationIds, Func<Task<Stream>> getStream)
        {
            BundleId = bundleId;
            NotificationIds = notificationIds;
            _getStream = getStream;
        }

        public Uuid BundleId { get; }
        public IEnumerable<Uuid> NotificationIds { get; }

        public async Task<Stream> OpenAsync()
        {
            return await _getStream();
        }
    }
}
