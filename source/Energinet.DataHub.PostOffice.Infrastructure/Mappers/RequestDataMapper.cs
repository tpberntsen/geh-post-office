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
using System.Linq;
using Energinet.DataHub.PostOffice.Application;
using Energinet.DataHub.PostOffice.Domain;

namespace Energinet.DataHub.PostOffice.Infrastructure.Mappers
{
    public sealed class RequestDataMapper : IMapper<Contracts.RequestDataset, RequestData>
    {
        public RequestData Map(Contracts.RequestDataset obj)
        {
            if (obj == null) throw new ArgumentNullException(nameof(obj));

            var requestData = new RequestData() { Uuids = obj.UUID.ToList() };

            return requestData;
        }
    }
}
