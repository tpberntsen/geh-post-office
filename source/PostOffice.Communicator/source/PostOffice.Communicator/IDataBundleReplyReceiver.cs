using System.Threading.Tasks;
using GreenEnergyHub.PostOffice.Communicator.Model;

namespace GreenEnergyHub.PostOffice.Communicator
{
    /// <summary>
    /// bla
    /// </summary>
    internal interface IDataBundleReplyReceiver
    {
        /// <summary>
        /// bla
        /// </summary>
        /// <param name="dataBundleReplyContract"></param>
        /// <returns>1</returns>
        Task<DataBundleReplyDto> ReceiveAsync(byte[] dataBundleReplyContract);
    }
}
