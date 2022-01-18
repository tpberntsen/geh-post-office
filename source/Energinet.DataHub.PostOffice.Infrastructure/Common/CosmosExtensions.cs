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
using Microsoft.Azure.Cosmos.Linq;

namespace Energinet.DataHub.PostOffice.Infrastructure.Common
{
    public static class CosmosExtensions
    {
        public static async IAsyncEnumerable<T> AsCosmosIteratorAsync<T>(this IQueryable<T> query)
        {
            using var iterator = query.ToFeedIterator();

            while (iterator.HasMoreResults)
            {
                foreach (var item in await iterator.ReadNextAsync().ConfigureAwait(false))
                {
                    yield return item;
                }
            }
        }

        // This method exists, because we have to put ConfigureAwait(false) on IAsyncEnumerable.
        public static async Task<T?> FirstOrDefaultAsync<T>(this IAsyncEnumerable<T> enumerable)
        {
            await foreach (var item in enumerable.ConfigureAwait(false))
            {
                return item;
            }

            return default;
        }

        // This method exists, because we have to put ConfigureAwait(false) on IAsyncEnumerable.
        public static async Task<T> SingleAsync<T>(this IAsyncEnumerable<T> enumerable)
        {
            await using var enumerator = enumerable.ConfigureAwait(false).GetAsyncEnumerator();

            if (!await enumerator.MoveNextAsync())
            {
                throw new InvalidOperationException("The collection is empty.");
            }

            var result = enumerator.Current;

            if (await enumerator.MoveNextAsync())
            {
                throw new InvalidOperationException("The collection has more than one element.");
            }

            return result;
        }

        // This method exists, because we have to put ConfigureAwait(false) on IAsyncEnumerable.
        public static async Task<T?> SingleOrDefaultAsync<T>(this IAsyncEnumerable<T> enumerable)
        {
            await using var enumerator = enumerable.ConfigureAwait(false).GetAsyncEnumerator();

            if (!await enumerator.MoveNextAsync())
            {
                return default;
            }

            var result = enumerator.Current;

            if (await enumerator.MoveNextAsync())
            {
                throw new InvalidOperationException("The collection has more than one element.");
            }

            return result;
        }

        // This method exists, because we have to put ConfigureAwait(false) on IAsyncEnumerable.
        public static async Task<IReadOnlyList<T>> ToListAsync<T>(this IAsyncEnumerable<T> enumerable)
        {
            var items = new List<T>();

            await foreach (var item in enumerable.ConfigureAwait(false))
            {
                items.Add(item);
            }

            return items;
        }
    }
}
