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

using Energinet.DataHub.PostOffice.Application.GetMessage.Interfaces;
using Energinet.DataHub.PostOffice.Domain.Services;
using Energinet.DataHub.PostOffice.Infrastructure.GetMessage;
using SimpleInjector;

namespace Energinet.DataHub.PostOffice.Common
{
    internal static class DomainServiceRegistration
    {
        public static void AddDomainServices(this Container container)
        {
            container.Register<IWeightCalculatorDomainService, WeightCalculatorDomainService>(Lifestyle.Scoped);
            container.Register<IMarketOperatorDataDomainService, MarketOperatorDataDomainService>(Lifestyle.Scoped);
            container.Register<IDataAvailableController, DataAvailableController>(Lifestyle.Scoped);
        }
    }
}
