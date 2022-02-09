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

namespace Energinet.DataHub.PostOffice.EntryPoint.Operations.HealthCheck
{
    public sealed class Health : IHealth
    {
        private readonly ICosmosDatabaseVerifier _cosmosDatabaseVerifier;
        private readonly ISqlDatabaseVerifier _sqlDatabaseVerifier;
        private readonly IServiceBusQueueVerifier _serviceBusVerifier;

        public Health(ICosmosDatabaseVerifier cosmosDatabaseVerifier, ISqlDatabaseVerifier sqlDatabaseVerifier, IServiceBusQueueVerifier serviceBusVerifier)
        {
            _cosmosDatabaseVerifier = cosmosDatabaseVerifier;
            _sqlDatabaseVerifier = sqlDatabaseVerifier;
            _serviceBusVerifier = serviceBusVerifier;
        }

        public IFluentHealth FluentValidator => new FluentHealth(_cosmosDatabaseVerifier, _sqlDatabaseVerifier, _serviceBusVerifier);
    }
}
