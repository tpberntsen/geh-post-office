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

namespace Energinet.DataHub.PostOffice.Inbound
{
    /// <summary>
    /// Parse and map from dto to domain document.
    /// </summary>
    public class InputParser
    {
        private readonly IMapper<Contracts.Document, Domain.Document> _mapper;

        public InputParser(
            IMapper<Contracts.Document, Domain.Document> mapper)
        {
            _mapper = mapper;
        }

        public Domain.Document Parse(byte[] bytes)
        {
            if (bytes == null) throw new ArgumentNullException(nameof(bytes));

            var document = Contracts.Document.Parser.ParseFrom(bytes);
            if (document == null) throw new InvalidOperationException("Cannot parse bytes to document.");

            return _mapper.Map(document);
        }
    }
}
