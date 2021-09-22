using System.Threading.Tasks;
using GreenEnergyHub.PostOffice.Communicator.Model;

namespace GreenEnergyHub.PostOffice.Communicator.Peek
{
    /// <summary>
    /// Singleton, thread-safe
    /// </summary>
    public interface IDataBundleRequestReceiver
    {
        /// <summary>
        /// bla
        /// </summary>
        /// <param name="dataBundleRequestContract"></param>
        /// <returns>1</returns>
        Task<DataBundleRequestDto> ReceiveAsync(byte[] dataBundleRequestContract);
    }
}
