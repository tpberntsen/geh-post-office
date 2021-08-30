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
using Energinet.DataHub.PostOffice.Application.DataAvailable;
using Energinet.DataHub.PostOffice.Domain.Model;
using Energinet.DataHub.PostOffice.Domain.Repositories;
using MediatR;

namespace Energinet.DataHub.PostOffice.Application.Handlers
{
    public class DataAvailableNotificationHandler : IRequestHandler<DataAvailableCommand, DataAvailableNotificationResponse>
    {
        private readonly IDataAvailableNotificationRepository _dataAvailableNotificationRepository;

        public DataAvailableNotificationHandler(IDataAvailableNotificationRepository dataAvailableNotificationRepository)
        {
            _dataAvailableNotificationRepository = dataAvailableNotificationRepository;
        }

        public async Task<DataAvailableNotificationResponse> Handle(DataAvailableCommand request, CancellationToken cancellationToken)
        {
            if (request is null) throw new ArgumentNullException(nameof(request));

            var dataAvailableNotification = new DataAvailableNotification(
                new Uuid(request.UUID),
                new Recipient(request.Recipient),
                new MessageType(request.RelativeWeight, request.MessageType),
                Enum.Parse<Origin>(request.Origin, true),
                new Weight(request.RelativeWeight));

            await _dataAvailableNotificationRepository.CreateAsync(dataAvailableNotification).ConfigureAwait(false);

            return new DataAvailableNotificationResponse();
        }
    }
}
