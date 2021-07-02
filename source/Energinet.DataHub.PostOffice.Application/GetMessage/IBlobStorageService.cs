using System.Threading.Tasks;

namespace Energinet.DataHub.PostOffice.Application.GetMessage
{
    /// <summary>
    /// Service to query Azure blob storage containing data to external actors
    /// </summary>
    public interface IBlobStorageService
    {
        /// <summary>
        /// Gets a document from the specified container with the specified filename
        /// </summary>
        /// <param name="containerName"></param>
        /// <param name="fileName"></param>
        /// <returns>A string expressing data interesting to external actor</returns>
        public Task<string> GetBlobAsync(string containerName, string fileName);
    }
}
