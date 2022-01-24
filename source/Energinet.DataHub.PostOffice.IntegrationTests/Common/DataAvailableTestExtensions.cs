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
using Azure.Storage.Blobs;
using Energinet.DataHub.MessageHub.Core;
using Energinet.DataHub.MessageHub.Core.Factories;
using Energinet.DataHub.MessageHub.Model.Model;
using Energinet.DataHub.PostOffice.Domain.Model;
using Energinet.DataHub.PostOffice.Domain.Repositories;
using SimpleInjector;

namespace Energinet.DataHub.PostOffice.IntegrationTests.Common
{
    internal static class DataAvailableTestExtensions
    {
        public static async Task<IReadOnlyList<DataAvailableNotification>> ReadToEndAsync(this ICabinetReader reader)
        {
            var items = new List<DataAvailableNotification>();

            while (reader.CanPeek)
            {
                items.Add(await reader.TakeAsync().ConfigureAwait(false));
            }

            return items;
        }

        public static Task<IReadOnlyList<Guid>> GetDataAvailableIdsAsync(this DataBundleRequestDto request, Scope scope)
        {
            var currentConfig = scope.GetInstance<StorageConfig>();
            var currentFactory = scope.GetInstance<IStorageServiceClientFactory>();

            var storageFromClient = new MessageHub.Client.Storage.StorageHandler(
                new TestClientFactoryWrapper(currentFactory),
                new MessageHub.Client.StorageConfig(currentConfig.AzureBlobStorageContainerName));

            return storageFromClient.GetDataAvailableNotificationIdsAsync(request);
        }

        private sealed class TestClientFactoryWrapper : MessageHub.Client.Factories.IStorageServiceClientFactory
        {
            private readonly IStorageServiceClientFactory _coreFactory;

            public TestClientFactoryWrapper(IStorageServiceClientFactory coreFactory)
            {
                _coreFactory = coreFactory;
            }

            public BlobServiceClient Create()
            {
                return _coreFactory.Create();
            }
        }
    }
}
