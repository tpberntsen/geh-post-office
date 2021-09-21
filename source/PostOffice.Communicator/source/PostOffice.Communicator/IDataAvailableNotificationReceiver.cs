using System.Threading.Tasks;
using GreenEnergyHub.PostOffice.Communicator.Model;

namespace GreenEnergyHub.PostOffice.Communicator
{
    /// <summary>
    /// Singleton, thread-safe
    /// </summary>
    public interface IDataAvailableNotificationReceiver
    {
        /// <summary>
        /// bla
        /// </summary>
        /// <param name="dataAvailableContract"></param>
        /// <returns>1</returns>
        Task<DataAvailableNotificationDto> ReceiveAsync(byte[] dataAvailableContract);
    }
}
