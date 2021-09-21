using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GreenEnergyHub.PostOffice.Communicator.Model
{
    public sealed record DataAvailableNotificationDto(
        string UUID, // Unique dataset identification
        string Recipient, // Dataset recipient
        string MessageType, // Dataset message type
        string Origin, // Identification for where the dataset can be queried
        bool SupportsBundling, // Is the message capable of being bundled with similar messages
        int RelativeWeight);
}
