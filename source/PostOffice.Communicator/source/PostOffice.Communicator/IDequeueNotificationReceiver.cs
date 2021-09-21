using System.Threading.Tasks;
using GreenEnergyHub.PostOffice.Communicator.Model;

namespace GreenEnergyHub.PostOffice.Communicator
{
    /// <summary>
    /// bla
    /// </summary>
    public interface IDequeueNotificationReceiver
    {
        /// <summary>
        /// bla
        /// </summary>
        /// <param name="dequeueNotificationContract"></param>
        /// <returns>1</returns>
        Task<DequeueNotificationDto> ReceiveAsync(byte[] dequeueNotificationContract);
    }
}
