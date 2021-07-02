using System;
using System.Threading.Tasks;
using Energinet.DataHub.PostOffice.Application.GetMessage;
using Microsoft.WindowsAzure.Storage;

namespace Energinet.DataHub.PostOffice.Infrastructure.GetMessage
{
    public class BlobStorageService : IBlobStorageService
    {
        public async Task<string> GetBlobAsync(string containerName, string fileName)
        {
            var connectionString = Environment.GetEnvironmentVariable("BlobStorageConnectionString");

            var storageAccount = CloudStorageAccount.Parse(connectionString);

            var serviceClient = storageAccount.CreateCloudBlobClient();

            var container = serviceClient.GetContainerReference(containerName);

            var blob = container.GetBlockBlobReference(fileName);

            var contents = await blob.DownloadTextAsync().ConfigureAwait(false);

            return contents;
        }
    }
}
