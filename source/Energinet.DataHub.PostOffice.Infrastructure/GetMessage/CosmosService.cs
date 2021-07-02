using System.Collections.Generic;
using System.Threading.Tasks;
using Energinet.DataHub.PostOffice.Application;
using Energinet.DataHub.PostOffice.Application.GetMessage;
using Energinet.DataHub.PostOffice.Contracts;

namespace Energinet.DataHub.PostOffice.Infrastructure.GetMessage
{
    public class CosmosService : ICosmosService
    {
        private readonly IDocumentStore<DataAvailable> _cosmosDocumentStore;
        private readonly IList<string> _collection;

        public CosmosService(IDocumentStore<DataAvailable> cosmosDocumentStore)
        {
            _cosmosDocumentStore = cosmosDocumentStore;
            _collection = new List<string>();
        }

        public async Task<IList<string>> GetUuidsFromCosmosDatabaseAsync(string recipient, string containerName)
        {
            var documents = await _cosmosDocumentStore.GetDocumentsAsync(new DocumentQuery(recipient, containerName))
                .ConfigureAwait(false);

            foreach (var document in documents)
            {
                if (document.UUID != null)
                {
                    _collection.Add(document.UUID);
                }
            }

            return _collection;
        }
    }
}
