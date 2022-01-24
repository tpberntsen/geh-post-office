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

using System.Linq;
using Energinet.DataHub.PostOffice.Application.Commands;
using Energinet.DataHub.PostOffice.Application.Validation.Rules;
using Energinet.DataHub.PostOffice.Domain.Model;
using FluentValidation;

namespace Energinet.DataHub.PostOffice.Application.Validation
{
    public sealed class InsertDataAvailableNotificationsCommandRuleSet : AbstractRuleSet<InsertDataAvailableNotificationsCommand>
    {
        public InsertDataAvailableNotificationsCommandRuleSet()
        {
            RuleFor(command => command.Notifications)
                .NotEmpty()
                .Must(x => x?.Select(dto => dto.Recipient).Distinct().Count() == 1);

            RuleForEach(command => command.Notifications)
                .NotNull()
                .ChildRules(dto =>
                {
                    dto.RuleFor(x => x.Uuid)
                        .NotEmpty()
                        .SetValidator(new UuidValidationRule());

                    dto.RuleFor(x => x.Recipient)
                        .NotEmpty()
                        .SetValidator(new GlobalLocationNumberValidationRule());

                    dto.RuleFor(x => x.ContentType)
                        .NotEmpty();

                    dto.RuleFor(x => x.Origin)
                        .NotEmpty()
                        .IsEnumName(typeof(DomainOrigin), false)
                        .NotEqual(_ => DomainOrigin.Unknown.ToString());

                    dto.RuleFor(x => x.Weight)
                        .GreaterThan(0);

                    dto.RuleFor(x => x.SequenceNumber)
                        .GreaterThan(0);
                });
        }
    }
}
