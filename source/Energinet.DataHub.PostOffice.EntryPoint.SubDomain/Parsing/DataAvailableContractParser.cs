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
using Energinet.DataHub.PostOffice.Application;
using Energinet.DataHub.PostOffice.Application.Commands;
using Energinet.DataHub.PostOffice.Contracts;

namespace Energinet.DataHub.PostOffice.EntryPoint.SubDomain.Parsing
{
    public class DataAvailableContractParser
    {
        private readonly IMapper<DataAvailable, DataAvailableNotificationCommand> _mapper;

        public DataAvailableContractParser(IMapper<DataAvailable, DataAvailableNotificationCommand> mapper)
        {
            _mapper = mapper;
        }

        public DataAvailableNotificationCommand Parse(byte[] bytes)
        {
            if (bytes is null) throw new ArgumentNullException(nameof(bytes));

            var dataAvailableContract = DataAvailable.Parser.ParseFrom(bytes);

            return _mapper.Map(dataAvailableContract);
        }
    }
}
