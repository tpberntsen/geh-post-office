using System.Threading.Tasks;
using GreenEnergyHub.PostOffice.Communicator.Model;

namespace GreenEnergyHub.PostOffice.Communicator.Dequeue
{
    /// <summary>
    /// bla
    /// </summary>
    public interface IDequeueNotificationParser
    {
        /// <summary>
        /// bla
        /// </summary>
        /// <param name="dequeueNotificationContract"></param>
        /// <returns>1</returns>
        DequeueNotificationDto Receive(byte[] dequeueNotificationContract);
    }
}
