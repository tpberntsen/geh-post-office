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
using System.Threading.Tasks;
using Energinet.DataHub.PostOffice.Domain.Model;
using Energinet.DataHub.PostOffice.Domain.Repositories;

namespace Energinet.DataHub.PostOffice.Tests.Common
{
    internal sealed class MockedCabinetReader : ICabinetReader
    {
        private readonly Queue<DataAvailableNotification> _items;

        public MockedCabinetReader(IEnumerable<DataAvailableNotification> notifications)
        {
            _items = new Queue<DataAvailableNotification>(notifications);
            Key = new CabinetKey(
                _items.Peek().Recipient,
                _items.Peek().Origin,
                _items.Peek().ContentType);
        }

        public CabinetKey Key { get; }

        public bool CanPeek => _items.Count > 0;

        public DataAvailableNotification Peek()
        {
            return _items.Peek();
        }

        public Task<DataAvailableNotification> TakeAsync()
        {
            return Task.FromResult(_items.Dequeue());
        }
    }
}
