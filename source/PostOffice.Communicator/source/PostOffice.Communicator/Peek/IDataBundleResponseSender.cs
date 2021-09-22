using System.Threading.Tasks;
using GreenEnergyHub.PostOffice.Communicator.Model;

namespace GreenEnergyHub.PostOffice.Communicator.Peek
{
    /// <summary>
    /// bla
    /// </summary>
    public interface IDataBundleResponseSender
    {
        /// <summary>
        /// bla
        /// </summary>
        /// <param name="requestDataBundleResponseDto"></param>
        /// <returns>1</returns>
        Task SendAsync(RequestDataBundleResponseDto requestDataBundleResponseDto);
    }
}
