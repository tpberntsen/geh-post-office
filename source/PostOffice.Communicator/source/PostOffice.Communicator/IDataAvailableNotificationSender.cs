using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GreenEnergyHub.PostOffice.Communicator.Model;

namespace GreenEnergyHub.PostOffice.Communicator
{
    /// <summary>
    /// Singleton, thread-safe
    /// </summary>
    public interface IDataAvailableNotificationSender
    {
        /// <summary>
        /// bla
        /// </summary>
        /// <param name="dataAvailableNotificationDto"></param>
        /// <returns>1</returns>
        Task SendAsync(DataAvailableNotificationDto dataAvailableNotificationDto);
    }
}
