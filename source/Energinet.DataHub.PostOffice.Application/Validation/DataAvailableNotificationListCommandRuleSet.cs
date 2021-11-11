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

using Energinet.DataHub.PostOffice.Application.Commands;
using Energinet.DataHub.PostOffice.Application.Validation.Rules;
using Energinet.DataHub.PostOffice.Domain.Model;
using FluentValidation;

namespace Energinet.DataHub.PostOffice.Application.Validation
{
    public sealed class DataAvailableNotificationListCommandRuleSet : AbstractRuleSet<DataAvailableNotificationListCommand>
    {
        public DataAvailableNotificationListCommandRuleSet()
        {
            RuleForEach(command => command.DataAvailableNotifications)
                .NotEmpty()
                .ChildRules(m => m.RuleFor(
                    x => x.Uuid).SetValidator(new UuidValidationRule()));

            RuleForEach(command => command.DataAvailableNotifications)
                .ChildRules(m => m.RuleFor(
                    x => x.Uuid).SetValidator(new GlobalLocationNumberValidationRule()));

            RuleForEach(command => command.DataAvailableNotifications)
                .ChildRules(m => m.RuleFor(
                    x => x.ContentType).NotEmpty());

            RuleForEach(command => command.DataAvailableNotifications)
                .ChildRules(m => m.RuleFor(
                    x => x.Origin).IsEnumName(typeof(DomainOrigin), false).NotEqual(_ => DomainOrigin.Unknown.ToString()));

            RuleForEach(command => command.DataAvailableNotifications)
                .ChildRules(m => m.RuleFor(x => x.Weight).GreaterThan(0));
        }
    }
}
