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

using Energinet.DataHub.MessageHub.Core.Dequeue;
using Energinet.DataHub.MessageHub.Core.Peek;
using Energinet.DataHub.MessageHub.Core.Storage;
using Energinet.DataHub.MessageHub.Model.DataAvailable;
using Energinet.DataHub.MessageHub.Model.Peek;
using Energinet.DataHub.PostOffice.Application.Commands;
using Energinet.DataHub.PostOffice.Application.Validation;
using FluentValidation;
using SimpleInjector;

namespace Energinet.DataHub.PostOffice.Common
{
    internal static class ApplicationServiceRegistration
    {
        public static void AddApplicationServices(this Container container)
        {
            container.Register<IValidator<DataAvailableNotificationCommand>, DataAvailableNotificationCommandRuleSet>(Lifestyle.Scoped);
            container.Register<IValidator<DataAvailableNotificationListCommand>, DataAvailableNotificationListCommandRuleSet>(Lifestyle.Scoped);
            container.Register<IValidator<PeekCommand>, PeekCommandRuleSet>(Lifestyle.Scoped);
            container.Register<IValidator<PeekTimeSeriesCommand>, PeekTimeSeriesCommandRuleSet>(Lifestyle.Scoped);
            container.Register<IValidator<PeekMasterDataCommand>, PeekMasterDataCommandRuleSet>(Lifestyle.Scoped);
            container.Register<IValidator<PeekAggregationsCommand>, PeekAggregationsCommandRuleSet>(Lifestyle.Scoped);
            container.Register<IValidator<DequeueCommand>, DequeueCommandRuleSet>(Lifestyle.Scoped);

            container.Register<IDataAvailableNotificationParser, DataAvailableNotificationParser>(Lifestyle.Singleton);
            container.Register<IRequestBundleParser, RequestBundleParser>(Lifestyle.Singleton);
            container.Register<IResponseBundleParser, ResponseBundleParser>(Lifestyle.Singleton);
            container.Register<IDataBundleRequestSender, DataBundleRequestSender>(Lifestyle.Singleton);
            container.Register<IDequeueNotificationSender, DequeueNotificationSender>(Lifestyle.Singleton);
            container.Register<IStorageHandler, StorageHandler>(Lifestyle.Singleton);
        }
    }
}
