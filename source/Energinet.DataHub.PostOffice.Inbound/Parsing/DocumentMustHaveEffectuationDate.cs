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
using Google.Protobuf.WellKnownTypes;
using GreenEnergyHub.Messaging.Validation;

namespace Energinet.DataHub.PostOffice.Inbound.Parsing
{
    public class DocumentMustHaveEffectuationDate : PropertyRule<Timestamp>
    {
        protected override string Code => "Json Serializable Content";

        protected override bool IsValid(Timestamp propertyValue, PropertyValidatorContext context)
        {
            return propertyValue != null
                   && propertyValue.ToDateTimeOffset() > DateTimeOffset.MinValue;
        }
    }
}
