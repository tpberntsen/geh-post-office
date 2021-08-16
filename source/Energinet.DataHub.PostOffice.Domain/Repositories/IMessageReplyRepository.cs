using System;
using System.Threading.Tasks;

namespace Energinet.DataHub.PostOffice.Domain.Repositories
{
    /// <summary>
    /// Repository for Message replies domain value objects.
    /// </summary>
    public interface IMessageReplyRepository
    {
        /// <summary>
        /// Getting path to saved data.
        /// </summary>
        /// <param name="messageKey"></param>
        /// <returns>path to saved data</returns>
        Task<string?> GetMessageReplyAsync(string messageKey);

        /// <summary>
        /// Saves message response to storage
        /// </summary>
        /// <param name="messageKey"></param>
        /// <param name="contentUri"></param>
        /// <returns>void task</returns>
        Task<bool> SaveMessageReplyAsync(string messageKey, Uri contentUri);
    }
}
