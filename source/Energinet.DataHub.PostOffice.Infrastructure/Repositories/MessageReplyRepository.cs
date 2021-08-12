using System;
using System.Threading.Tasks;
using Energinet.DataHub.PostOffice.Domain.Repositories;
using Microsoft.Azure.Cosmos;

namespace Energinet.DataHub.PostOffice.Infrastructure.Repositories
{
    public class MessageReplyRepository : IMessageReplyRepository
    {
       // private const string ContainerName = "messagereplies";
        private readonly CosmosClient _cosmosClient;
        private readonly CosmosDatabaseConfig _cosmosConfig;

        public MessageReplyRepository(CosmosClient cosmosClient, CosmosDatabaseConfig cosmosConfig)
        {
            _cosmosClient = cosmosClient;
            _cosmosConfig = cosmosConfig;
        }

        public async Task<string?> GetMessageReplyAsync(string messageKey)
        {
            if (messageKey is null) throw new ArgumentNullException(nameof(messageKey));

            return await Task.FromResult(string.Empty).ConfigureAwait(false);
        }

        public Task SaveMessageReplyAsync(string messageKey, Uri contentUri)
        {
            throw new NotImplementedException();
        }
    }
}
