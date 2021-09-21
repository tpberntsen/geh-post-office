using System.Threading.Tasks;
using GreenEnergyHub.PostOffice.Communicator.Model;

namespace GreenEnergyHub.PostOffice.Communicator.Dequeue
{
    /// <summary>
    /// bla
    /// </summary>
    public interface IDequeueNotificationSender
    {
        /// <summary>
        /// bla
        /// </summary>
        /// <param name="dequeueNotificationDto"></param>
        /// <returns>1</returns>
        Task SendAsync(DequeueNotificationDto dequeueNotificationDto);
    }
}
