using System.Threading.Tasks;
using GreenEnergyHub.PostOffice.Communicator.Model;

namespace GreenEnergyHub.PostOffice.Communicator
{
    // Singleton, thread-safe
    public interface IRequestDataBundleReceiver
    {
        Task<DataBundleRequestDto> ReceiveAsync(byte[] dataBundleRequestContract);
    }
}
