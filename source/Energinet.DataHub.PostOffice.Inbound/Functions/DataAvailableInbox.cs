using System;
using System.Threading.Tasks;
using Energinet.DataHub.PostOffice.Application;
using Energinet.DataHub.PostOffice.Inbound.Parsing;
using Microsoft.Azure.ServiceBus;
using Microsoft.Azure.ServiceBus.Management;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;

namespace Energinet.DataHub.PostOffice.Inbound.Functions
{
    public class DataAvailableInbox
    {
        private const string FunctionName = "DataAvailableInbox";

        private readonly InputParserDataAvailable _inputParser;
        private readonly ICosmosStore<Domain.DataAvailable> _documentStore;

        public DataAvailableInbox(
            InputParserDataAvailable inputParser,
            ICosmosStore<Domain.DataAvailable> documentStore)
        {
            _inputParser = inputParser;
            _documentStore = documentStore;
        }

        [FunctionName(FunctionName)]
        public async Task RunAsync(
            [ServiceBusTrigger(
                "%INBOUND_QUEUE_DATAAVAILABLE_TOPIC_NAME%",
                "%INBOUND_QUEUE_DATAAVAILABLE_SUBSCRIPTION_NAME%",
                Connection = "INBOUND_QUEUE_CONNECTION_STRING")]
            Message message,
            ILogger logger)
        {
            if (message == null) throw new ArgumentNullException(nameof(message));
            logger.LogInformation($"C# ServiceBus topic trigger function processed message in {FunctionName} {message.Label}.");

            try
            {
                var topicName = Environment.GetEnvironmentVariable("INBOUND_QUEUE_DATAAVAILABLE_TOPIC_NAME");
                if (string.IsNullOrEmpty(topicName)) throw new InvalidOperationException("TopicName is null");

                var document = await _inputParser.ParseAsync(message.Body).ConfigureAwait(false);
                await _documentStore.SaveDocumentAsync(document).ConfigureAwait(false);
                logger.LogInformation("Document saved to cosmos: {document}", document);
            }
            catch (Exception exception)
            {
                logger.LogError(exception, $"Error in {FunctionName}");
                throw;
            }
        }
    }
}
