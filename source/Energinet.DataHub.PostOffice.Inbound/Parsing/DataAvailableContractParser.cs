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
using Energinet.DataHub.PostOffice.Application.DataAvailable;
using GreenEnergyHub.Messaging;

namespace Energinet.DataHub.PostOffice.Inbound.Parsing
{
    public class DataAvailableContractParser
    {
        private readonly IMapper<Contracts.DataAvailable, DataAvailableCommand> _mapper;

        public DataAvailableContractParser(IMapper<Contracts.DataAvailable, DataAvailableCommand> mapper)
        {
            _mapper = mapper;
        }

        public DataAvailableCommand Parse(byte[] bytes)
        {
            if (bytes == null) throw new ArgumentNullException(nameof(bytes));

            var dataAvailableContract = Contracts.DataAvailable.Parser.ParseFrom(bytes);
            if (dataAvailableContract == null) throw new InvalidOperationException("Cannot parse bytes to document.");

            return _mapper.Map(dataAvailableContract);
        }
    }
}
