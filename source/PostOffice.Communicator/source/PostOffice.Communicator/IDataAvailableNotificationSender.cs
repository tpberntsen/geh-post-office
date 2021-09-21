using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GreenEnergyHub.PostOffice.Communicator.Model;

namespace GreenEnergyHub.PostOffice.Communicator
{
    // Singleton, thread-safe
    public interface IDataAvailableNotificationSender
    {
        Task SendAsync(DataAvailableNotificationDto dataAvailableNotificationDto);
    }
}
