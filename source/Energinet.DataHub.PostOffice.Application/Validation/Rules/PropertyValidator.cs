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
using FluentValidation.Validators;

namespace Energinet.DataHub.PostOffice.Application.Validation.Rules
{
    public abstract class PropertyValidator<T> : PropertyValidator
    {
        protected PropertyValidator(string errorCode)
        {
            ErrorCode = errorCode;
        }

        protected override bool IsValid(PropertyValidatorContext context) =>
            context == null
                ? throw new ArgumentNullException(nameof(context))
                : context.PropertyValue is T value && IsValid(value);

        protected abstract bool IsValid(T value);
    }
}
