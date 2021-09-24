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
using Energinet.DataHub.PostOffice.Domain.Model;

namespace Energinet.DataHub.PostOffice.Domain.Services
{
    public class WeightCalculatorDomainService : IWeightCalculatorDomainService
    {
        public Weight CalculateMaxWeight(DomainOrigin domainOrigin)
        {
            switch (domainOrigin)
            {
                case DomainOrigin.Aggregations:
                case DomainOrigin.TimeSeries:
                case DomainOrigin.Charges:
                    return new Weight(1);
                case DomainOrigin.Unknown:
                    throw new InvalidOperationException($"Mapping of enum {nameof(DomainOrigin)}.{nameof(DomainOrigin.Unknown)} to type {nameof(Weight)} is undefined.");
                default:
                    throw new ArgumentOutOfRangeException(nameof(domainOrigin), domainOrigin, null);
            }
        }
    }
}
