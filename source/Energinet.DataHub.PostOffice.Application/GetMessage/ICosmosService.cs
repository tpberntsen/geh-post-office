using System.Collections.Generic;
using System.Threading.Tasks;

namespace Energinet.DataHub.PostOffice.Application.GetMessage
{
    /// <summary>
    /// Service to connect and retrieve data from Cosmos database
    /// </summary>
    public interface ICosmosService
    {
        /// <summary>
        /// Get UUIDs from Cosmos database
        /// </summary>
        /// <param name="recipient"></param>
        /// <param name="containerName"></param>
        /// <returns>A collection with all UUIDs for the specified recipient</returns>
        public Task<IList<string>> GetUuidsFromCosmosDatabaseAsync(string recipient, string containerName);
    }
}
