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
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Energinet.DataHub.PostOffice.Application.Commands;
using Energinet.DataHub.PostOffice.Domain.Model;
using Energinet.DataHub.PostOffice.Domain.Repositories;
using MediatR;

namespace Energinet.DataHub.PostOffice.Application.Handlers
{
    public class DataAvailableNotificationHandler
        : IRequestHandler<DataAvailableNotificationCommand, DataAvailableNotificationResponse>,
            IRequestHandler<DataAvailableNotificationListCommand, DataAvailableNotificationResponse>,
            IRequestHandler<GetDuplicatedDataAvailablesFromArchiveCommand, GetDuplicatedDataAvailablesFromArchiveResponse>
    {
        private readonly IDataAvailableNotificationRepository _dataAvailableNotificationRepository;

        public DataAvailableNotificationHandler(IDataAvailableNotificationRepository dataAvailableNotificationRepository)
        {
            _dataAvailableNotificationRepository = dataAvailableNotificationRepository;
        }

        public async Task<DataAvailableNotificationResponse> Handle(DataAvailableNotificationCommand request, CancellationToken cancellationToken)
        {
            if (request is null)
                throw new ArgumentNullException(nameof(request));

            var dataAvailableNotification = MapToDataAvailableNotification(request);
            await _dataAvailableNotificationRepository.SaveAsync(dataAvailableNotification).ConfigureAwait(false);
            return new DataAvailableNotificationResponse();
        }

        public async Task<DataAvailableNotificationResponse> Handle(DataAvailableNotificationListCommand request, CancellationToken cancellationToken)
        {
            if (request is null)
                throw new ArgumentNullException(nameof(request));

            var mappedDataAvailable = request
                .DataAvailableNotifications
                .Select(MapToDataAvailableNotification);

            await _dataAvailableNotificationRepository.SaveAsync(mappedDataAvailable).ConfigureAwait(false);
            return new DataAvailableNotificationResponse();
        }

        public async Task<GetDuplicatedDataAvailablesFromArchiveResponse> Handle(GetDuplicatedDataAvailablesFromArchiveCommand request, CancellationToken cancellationToken)
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request));

            var mapped = request.DataAvailableNotificationCommands.Select(x =>
            {
                var couldBeMapped = TryMapToDataAvailableNotification(x, out var mapped);
                return (couldBeMapped, mapped, x);
            }).Where(x => x.couldBeMapped).ToDictionary(x => x.mapped!, x => x.x);

            var result = _dataAvailableNotificationRepository.ValidateAgainstArchiveAsync(mapped.Select(x => x.Key));
            var mappedResult = new List<(DataAvailableNotificationCommand Command, bool IsIdempotent)>();
            await foreach (var entry in result)
            {
                mappedResult.Add((mapped[entry.Command], entry.IsIdempotent));
            }

            return new GetDuplicatedDataAvailablesFromArchiveResponse(mappedResult);

            static bool TryMapToDataAvailableNotification(DataAvailableNotificationCommand request, [NotNullWhen(true)] out DataAvailableNotification? command)
            {
                try
                {
                    command = MapToDataAvailableNotification(request);
                }
    #pragma warning disable CA1031
                catch (Exception)
    #pragma warning restore CA1031
                {
                    command = null;
                }

                return command != null;
            }
        }

        private static DataAvailableNotification MapToDataAvailableNotification(DataAvailableNotificationCommand request)
        {
            return new DataAvailableNotification(
                new Uuid(request.Uuid),
                new MarketOperator(new GlobalLocationNumber(request.Recipient)),
                new ContentType(request.ContentType),
                Enum.Parse<DomainOrigin>(request.Origin, true),
                new SupportsBundling(request.SupportsBundling),
                new Weight(request.Weight));
        }
    }
}
