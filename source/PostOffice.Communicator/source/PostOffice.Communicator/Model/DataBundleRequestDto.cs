using System.Collections.Generic;

namespace GreenEnergyHub.PostOffice.Communicator.Model
{
    public sealed record DataBundleRequestDto(string IdempotencyId, IEnumerable<string> DataAvailableNotificationIds);
}
