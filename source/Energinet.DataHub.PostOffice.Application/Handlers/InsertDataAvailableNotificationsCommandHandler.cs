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
using System.Threading;
using System.Threading.Tasks;
using Energinet.DataHub.PostOffice.Application.Commands;
using Energinet.DataHub.PostOffice.Domain.Model;
using Energinet.DataHub.PostOffice.Domain.Repositories;
using Energinet.DataHub.PostOffice.Utilities;
using MediatR;

namespace Energinet.DataHub.PostOffice.Application.Handlers
{
    public sealed class InsertDataAvailableNotificationsCommandHandler : IRequestHandler<InsertDataAvailableNotificationsCommand>
    {
        private readonly IDataAvailableNotificationRepository _dataAvailableNotificationRepository;

        public InsertDataAvailableNotificationsCommandHandler(IDataAvailableNotificationRepository dataAvailableNotificationRepository)
        {
            _dataAvailableNotificationRepository = dataAvailableNotificationRepository;
        }

        public async Task<Unit> Handle(InsertDataAvailableNotificationsCommand request, CancellationToken cancellationToken)
        {
            Guard.ThrowIfNull(request, nameof(request));

            var groupedByKey = request
                .Notifications
                .Select(Map)
                .GroupBy(notification => new CabinetKey(notification));

            await Task.WhenAll(groupedByKey.Select(HandleGroupAsync)).ConfigureAwait(false);

            return Unit.Value;
        }

        private static DataAvailableNotification Map(DataAvailableNotificationDto notificationDto)
        {
            return new DataAvailableNotification(
                new Uuid(notificationDto.Uuid),
                new MarketOperator(new GlobalLocationNumber(notificationDto.Recipient)),
                new ContentType(notificationDto.ContentType),
                Enum.Parse<DomainOrigin>(notificationDto.Origin, true),
                new SupportsBundling(notificationDto.SupportsBundling),
                new Weight(notificationDto.Weight),
                new SequenceNumber(notificationDto.SequenceNumber),
                new DocumentType(notificationDto.DocumentType));
        }

        private Task HandleGroupAsync(IGrouping<CabinetKey, DataAvailableNotification> group)
        {
            return _dataAvailableNotificationRepository.SaveAsync(group.Key, group.ToList());
        }
    }
}
