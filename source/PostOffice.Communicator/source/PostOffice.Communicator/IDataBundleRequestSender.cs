using System.Threading.Tasks;
using GreenEnergyHub.PostOffice.Communicator.Model;

namespace GreenEnergyHub.PostOffice.Communicator
{
    /// <summary>
    /// Singleton, thread-safe
    /// </summary>
    public interface IDataBundleRequestSender
    {
        /// <summary>
        /// bla
        /// </summary>
        /// <param name="dataBundleRequestDto"></param>
        /// <returns>1</returns>
        Task<DataBundleReplyDto?> SendAsync(DataBundleRequestDto dataBundleRequestDto);
    }
}
