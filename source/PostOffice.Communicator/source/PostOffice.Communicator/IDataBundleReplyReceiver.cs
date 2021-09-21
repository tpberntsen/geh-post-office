using System.Threading.Tasks;
using GreenEnergyHub.PostOffice.Communicator.Model;

namespace GreenEnergyHub.PostOffice.Communicator
{
    internal interface IDataBundleReplyReceiver
    {
        Task<DataBundleReplyDto> ReceiveAsync(byte[] dataBundleReplyContract);
    }
}
