using System.Collections.Generic;

namespace GreenEnergyHub.PostOffice.Communicator.Model
{
    public sealed record DequeueNotificationDto(ICollection<string> DatasAvailableIds, string Recipient);
}
