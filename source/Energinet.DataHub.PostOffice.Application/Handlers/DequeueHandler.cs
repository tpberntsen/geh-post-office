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
using Energinet.DataHub.PostOffice.Application.Commands;
using Energinet.DataHub.PostOffice.Domain.Model;
using Energinet.DataHub.PostOffice.Domain.Services;
using MediatR;

namespace Energinet.DataHub.PostOffice.Application.Handlers
{
    public class DequeueHandler : IRequestHandler<DequeueCommand, DequeueResponse>
    {
        private readonly IMarketOperatorDataDomainService _marketOperatorDataDomainService;

        public DequeueHandler(IMarketOperatorDataDomainService marketOperatorDataDomainService)
        {
            _marketOperatorDataDomainService = marketOperatorDataDomainService;
        }

        public async Task<DequeueResponse> Handle(DequeueCommand request, CancellationToken cancellationToken)
        {
            if (request is null)
                throw new ArgumentNullException(nameof(request));

            var isDequeued = await _marketOperatorDataDomainService
                .TryAcknowledgeAsync(
                    new MarketOperator(new GlobalLocationNumber(request.Recipient)),
                    new Uuid(request.BundleUuid))
                .ConfigureAwait(false);

            return new DequeueResponse(isDequeued);
        }
    }
}
