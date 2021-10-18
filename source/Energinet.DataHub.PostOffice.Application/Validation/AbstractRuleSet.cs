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
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using FluentValidation.Results;
using FluentValidation.Validators;

namespace Energinet.DataHub.PostOffice.Application.Validation
{
    public abstract class AbstractRuleSet<T> : AbstractValidator<T>
    {
        public override ValidationResult Validate(ValidationContext<T> context)
        {
            SetErrorCodes();
            return base.Validate(context);
        }

        public override Task<ValidationResult> ValidateAsync(ValidationContext<T> context, CancellationToken cancellation = default)
        {
            SetErrorCodes();
            return base.ValidateAsync(context, cancellation);
        }

        private void SetErrorCodes()
        {
            foreach (var validator in this.SelectMany(x => x.Validators).Where(x => x.Options.ErrorCode == null))
            {
                validator.Options.ErrorCode = validator switch
                {
                    NotEmptyValidator => "value_not_specified",
                    StringEnumValidator => "invalid_enum_value",
                    NotEqualValidator => "value_not_equal_to",
                    GreaterThanValidator => "value_not_greater_than",
                    _ => "validation_error"
                };
            }
        }
    }
}
