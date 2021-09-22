using System.Collections.Generic;

namespace GreenEnergyHub.PostOffice.Communicator.Model
{
    public sealed record DequeueNotificationDto(ICollection<string> DatasetIds, string Recipient);
}
