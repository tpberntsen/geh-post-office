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
using Energinet.DataHub.PostOffice.Utilities;
using Microsoft.Extensions.Configuration;

namespace Energinet.DataHub.PostOffice.Common
{
    public sealed class FeatureFlags : IFeatureFlags
    {
        private readonly Feature _features;

        public FeatureFlags(IConfiguration configuration)
        {
            Feature flags = 0;

            foreach (var feature in Enum.GetValues<Feature>())
            {
                var enabled = configuration.GetValue(MapFeatureToKey(feature), false);
                if (enabled)
                {
                    flags |= feature;
                }
            }

            _features = flags;
        }

        public bool IsFeatureActive(Feature feature)
        {
            return _features.HasFlag(feature);
        }

        private static string MapFeatureToKey(Feature feature)
        {
            return $"FEATURE_{feature.ToString().ToUpperInvariant()}";
        }
    }
}
