using System.Threading.Tasks;
using GreenEnergyHub.PostOffice.Communicator.Model;

namespace GreenEnergyHub.PostOffice.Communicator
{
    public interface IDequeueNotificationReceiver
    {
        Task<DequeueNotificationDto> ReceiveAsync(byte[] dequeueNotificationContract);
    }
}
