using System.Threading.Tasks;

namespace Energinet.DataHub.PostOffice.Application.GetMessage
{
    /// <summary>
    /// Get path to data from service bus container
    /// </summary>
    public interface IGetPathToDataFromServiceBus
    {
        /// <summary>
        /// Get path to data from service bus container
        /// </summary>
        /// <param name="containerName"></param>
        /// <param name="sessionId"></param>
        /// <returns>String containing path to data in document store</returns>
        public Task<string> GetPathAsync(string containerName, string sessionId);
    }
}
