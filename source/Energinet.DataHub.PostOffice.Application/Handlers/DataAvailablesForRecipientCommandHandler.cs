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
using System.Collections.Generic;
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
    // TODO: Name mismatch.
    public class DataAvailablesForRecipientCommandHandler
        : IRequestHandler<DataAvailableNotificationsForRecipientCommand, DataAvailableNotificationResponse>
    {
        private readonly IDataAvailableNotificationRepository _dataAvailableNotificationRepository;

        public DataAvailablesForRecipientCommandHandler(IDataAvailableNotificationRepository dataAvailableNotificationRepository)
        {
            _dataAvailableNotificationRepository = dataAvailableNotificationRepository;
        }

        public async Task<DataAvailableNotificationResponse> Handle(DataAvailableNotificationsForRecipientCommand request, CancellationToken cancellationToken)
        {
            Guard.ThrowIfNull(request, nameof(request));

            var groupedByKey = request.Notifications.GroupBy(x => new CabinetKey(
                new MarketOperator(new GlobalLocationNumber(x.Recipient)),
                Enum.Parse<DomainOrigin>(x.Origin),
                new ContentType(x.ContentType)));

            var tasks = new List<Task>();

            foreach (var group in groupedByKey)
            {
                tasks.Add(ProcessGroupAsync(group, group.Key));
            }

            await Task.WhenAll(tasks).ConfigureAwait(false);
            return new DataAvailableNotificationResponse();
        }

        private static DataAvailableNotification Map(DataAvailableNotificationCommand notificationCommand)
        {
            return new(
                new Uuid(notificationCommand.Uuid),
                new MarketOperator(new GlobalLocationNumber(notificationCommand.Recipient)),
                new ContentType(notificationCommand.ContentType),
                Enum.Parse<DomainOrigin>(notificationCommand.Origin),
                new SupportsBundling(notificationCommand.SupportsBundling),
                new Weight(notificationCommand.Weight),
                new SequenceNumber(notificationCommand.SequenceNumber));
        }

        private Task ProcessGroupAsync(IEnumerable<DataAvailableNotificationCommand> group, CabinetKey key)
        {
            var dataAvailableNotifications = group.Select(Map);
            return _dataAvailableNotificationRepository.SaveAsync(dataAvailableNotifications, key);
        }
    }
}
