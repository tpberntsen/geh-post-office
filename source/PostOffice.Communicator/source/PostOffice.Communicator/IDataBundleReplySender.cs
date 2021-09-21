using System.Threading.Tasks;
using GreenEnergyHub.PostOffice.Communicator.Model;

namespace GreenEnergyHub.PostOffice.Communicator
{
    /// <summary>
    /// bla
    /// </summary>
    public interface IDataBundleReplySender
    {
        /// <summary>
        /// bla
        /// </summary>
        /// <param name="dataBundleReplyDto"></param>
        /// <returns>1</returns>
        Task SendAsync(DataBundleReplyDto dataBundleReplyDto);
    }
}
