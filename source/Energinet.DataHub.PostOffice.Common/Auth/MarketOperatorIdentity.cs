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

namespace Energinet.DataHub.PostOffice.Common.Auth
{
    internal sealed class MarketOperatorIdentity : IMarketOperatorIdentity
    {
        private string? _gln;

        public bool HasIdentity => _gln != null;

        public string Gln => _gln ?? throw new InvalidOperationException("The identity of the market operator is not known.");

        void IMarketOperatorIdentity.AssignGln(string gln)
        {
            if (string.IsNullOrWhiteSpace(gln))
                throw new ArgumentException("Cannot assign an empty value as GLN.", nameof(gln));

            if (HasIdentity)
                throw new InvalidOperationException("An identity has already been assigned.");

            _gln = gln;
        }
    }
}
