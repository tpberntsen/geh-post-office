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
using Energinet.DataHub.PostOffice.Domain.Model;
using Energinet.DataHub.PostOffice.Domain.Repositories;
using Energinet.DataHub.PostOffice.Infrastructure.Documents;
using Energinet.DataHub.PostOffice.Infrastructure.Mappers;

namespace Energinet.DataHub.PostOffice.Infrastructure.Model
{
    internal sealed class AsyncCabinetReader : ICabinetReader
    {
        private readonly IReadOnlyList<CosmosCabinetDrawer> _drawers;
        private readonly IReadOnlyList<Task<IEnumerable<CosmosDataAvailable>>> _items;

        private readonly List<CosmosCabinetDrawerChanges?> _changes = new();
        private readonly Queue<CosmosDataAvailable> _drawerQueue = new();

        private DataAvailableNotification? _mappedDataAvailableNotification;
        private int _index;

        public AsyncCabinetReader(
            CabinetKey cabinetKey,
            IReadOnlyList<CosmosCabinetDrawer> drawers,
            IReadOnlyList<Task<IEnumerable<CosmosDataAvailable>>> items)
        {
            Key = cabinetKey;
            _drawers = drawers;
            _items = items;

            for (var i = 0; i < drawers.Count; i++)
            {
                _changes.Add(null);
            }
        }

        public CabinetKey Key { get; }

        public bool CanPeek => _drawerQueue.Count > 0;

        public Task InitializeAsync()
        {
            return _index < _items.Count
                ? FillQueueFromCurrentIndexAsync()
                : Task.CompletedTask;
        }

        public DataAvailableNotification Peek()
        {
            ThrowIfNoMoreItems();

            if (_mappedDataAvailableNotification != null)
                return _mappedDataAvailableNotification;

            var dataAvailableNotification = _drawerQueue.Peek();
            return _mappedDataAvailableNotification = CosmosDataAvailableMapper.Map(dataAvailableNotification);
        }

        public async Task<DataAvailableNotification> TakeAsync()
        {
            ThrowIfNoMoreItems();

            var document = _drawerQueue.Dequeue();
            var drawerChanges = GetCurrentDrawerChanges(document);

            if (_drawerQueue.Count == 0)
            {
                UpdatePositionInDrawer(drawerChanges);
                UpdateCatalogEntry(drawerChanges, null);

                if (++_index < _drawers.Count)
                {
                    await FillQueueFromCurrentIndexAsync().ConfigureAwait(false);
                }
            }
            else
            {
                var nextItemInDrawer = _drawerQueue.Peek();
                UpdatePositionInDrawer(drawerChanges);
                UpdateCatalogEntry(drawerChanges, nextItemInDrawer);
            }

            var mappedNotification = _mappedDataAvailableNotification;
            if (mappedNotification != null)
            {
                _mappedDataAvailableNotification = null;
                return mappedNotification;
            }

            return CosmosDataAvailableMapper.Map(document);
        }

        public IEnumerable<CosmosCabinetDrawerChanges> GetChanges()
        {
            foreach (var drawerChange in _changes)
            {
                if (drawerChange != null)
                    yield return drawerChange;
            }
        }

        private static void UpdatePositionInDrawer(CosmosCabinetDrawerChanges drawerChanges)
        {
            drawerChanges.UpdatedDrawer = drawerChanges.UpdatedDrawer with
            {
                Position = drawerChanges.UpdatedDrawer.Position + 1
            };
        }

        private void UpdateCatalogEntry(CosmosCabinetDrawerChanges drawerChanges, CosmosDataAvailable? nextItemInDrawer)
        {
            if (nextItemInDrawer == null)
            {
                drawerChanges.UpdatedCatalogEntry = null;
                return;
            }

            var currentEntry = drawerChanges.UpdatedCatalogEntry ?? new CosmosCatalogEntry
            {
                Id = Guid.NewGuid().ToString(),
                ContentType = Key.ContentType.Value,
                PartitionKey = string.Join('_', Key.Recipient.Gln.Value, Key.Origin)
            };

            drawerChanges.UpdatedCatalogEntry = currentEntry with
            {
                NextSequenceNumber = nextItemInDrawer.SequenceNumber
            };
        }

        private CosmosCabinetDrawerChanges GetCurrentDrawerChanges(CosmosDataAvailable initialItem)
        {
            var currentDrawer = _changes[_index];
            if (currentDrawer != null)
            {
                return currentDrawer;
            }

            return _changes[_index] = new CosmosCabinetDrawerChanges
            {
                UpdatedDrawer = _drawers[_index],
                InitialCatalogEntrySequenceNumber = initialItem.SequenceNumber
            };
        }

        private async Task FillQueueFromCurrentIndexAsync()
        {
            foreach (var dataAvailable in await _items[_index].ConfigureAwait(false))
            {
                _drawerQueue.Enqueue(dataAvailable);
            }
        }

        private void ThrowIfNoMoreItems()
        {
            if (!CanPeek)
            {
                throw new InvalidOperationException("There are no more items.");
            }
        }
    }
}
