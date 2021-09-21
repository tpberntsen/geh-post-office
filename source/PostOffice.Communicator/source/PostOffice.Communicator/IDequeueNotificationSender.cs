using System.Threading.Tasks;
using GreenEnergyHub.PostOffice.Communicator.Model;

namespace GreenEnergyHub.PostOffice.Communicator
{
    public interface IDequeueNotificationSender
    {
        Task SendAsync(DequeueNotificationDto dequeueNotificationDto);
    }
}
