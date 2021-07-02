using System.Collections.Generic;
using System.Threading.Tasks;

namespace Energinet.DataHub.PostOffice.Application.GetMessage
{
    /// <summary>
    /// Send message to service bus container
    /// </summary>
    public interface ISendMessageToServiceBus
    {
        /// <summary>
        /// Send message to service bus container
        /// </summary>
        /// <param name="uuids"></param>
        /// <param name="containerName"></param>
        /// <param name="sessionId"></param>
        public Task SendMessageAsync(IList<string> uuids, string containerName, string sessionId);
    }
}
