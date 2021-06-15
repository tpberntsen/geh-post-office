using System;
using Microsoft.Azure.ServiceBus;
using Microsoft.Azure.ServiceBus.Management;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;

namespace Energinet.DataHub.PostOffice.Inbound.Functions
{
    public static class DataAvailableInbox
    {
        private const string FunctionName = "DataAvailableInbox";

        [FunctionName(FunctionName)]
        public static void Run(
            [ServiceBusTrigger(
                "%INBOUND_QUEUE_DATAAVAILABLE_TOPIC_NAME%",
                "%INBOUND_QUEUE_DATAAVAILABLE_SUBSCRIPTION_NAME%",
                Connection = "INBOUND_QUEUE_CONNECTION_STRING")]
            Message message,
            ILogger logger)
        {
            if (message == null) throw new ArgumentNullException(nameof(message));
            logger.LogInformation($"C# ServiceBus topic trigger function processed message in {FunctionName} {message.Label}.");
        }
    }
}
