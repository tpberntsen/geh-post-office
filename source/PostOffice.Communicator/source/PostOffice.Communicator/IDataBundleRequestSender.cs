using System.Threading.Tasks;
using GreenEnergyHub.PostOffice.Communicator.Model;

namespace GreenEnergyHub.PostOffice.Communicator
{
    // Singleton, thread-safe
    public interface IDataBundleRequestSender
    {
        Task<DataBundleReplyDto?> SendAsync(DataBundleRequestDto dataBundleRequestDto);
    }
}
