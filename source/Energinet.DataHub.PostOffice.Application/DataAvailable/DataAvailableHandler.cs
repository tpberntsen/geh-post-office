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
using System.Threading;
using System.Threading.Tasks;
using Energinet.DataHub.PostOffice.Domain.Repositories;
using MediatR;

namespace Energinet.DataHub.PostOffice.Application.DataAvailable
{
    public class DataAvailableHandler : IRequestHandler<DataAvailableCommand, bool>
    {
        private readonly IDataAvailableRepository _dataAvailableRepository;

        public DataAvailableHandler(IDataAvailableRepository dataAvailableRepository)
        {
            _dataAvailableRepository = dataAvailableRepository;
        }

        public async Task<bool> Handle(DataAvailableCommand request, CancellationToken cancellationToken)
        {
            if (request is null) throw new ArgumentNullException(nameof(request));

            var dataAvailableDomain = new Domain.DataAvailable(request.UUID, request.Recipient, request.MessageType, request.Origin, request.SupportsBundling, request.RelativeWeight, 1M);
            var saveResult = await _dataAvailableRepository.SaveDocumentAsync(dataAvailableDomain).ConfigureAwait(false);

            return saveResult;
        }
    }
}
