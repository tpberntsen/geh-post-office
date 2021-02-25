using System;
using System.Collections.Generic;

namespace Energinet.DataHub.PostOffice.Infrastructure
{
    public class CosmosContainerConfig
    {
        public CosmosContainerConfig(string[] containers)
        {
            Containers = containers ?? throw new ArgumentNullException(nameof(containers));
        }

        public IReadOnlyList<string> Containers { get; }
    }
}
