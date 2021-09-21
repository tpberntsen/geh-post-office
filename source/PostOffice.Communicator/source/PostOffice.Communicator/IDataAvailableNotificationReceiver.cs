using System.Threading.Tasks;
using GreenEnergyHub.PostOffice.Communicator.Model;

namespace GreenEnergyHub.PostOffice.Communicator
{
    // Singleton, thread-safe
    public interface IDataAvailableNotificationReceiver
    {
        Task<DataAvailableNotificationDto> ReceiveAsync(byte[] dataAvailableContract);
    }
}
