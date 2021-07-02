using System;
using System.Net;
using System.Threading.Tasks;
using Azure.Messaging.ServiceBus;
using Energinet.DataHub.PostOffice.Application.GetMessage;
using Microsoft.Azure.Functions.Worker.Http;

namespace Energinet.DataHub.PostOffice.Infrastructure.GetMessage
{
    public class GetPathToDataFromServiceBus : IGetPathToDataFromServiceBus
    {
        private ServiceBusClient _serviceBusClient;

        public GetPathToDataFromServiceBus(ServiceBusClient serviceBusClient)
        {
            _serviceBusClient = serviceBusClient;
        }

        public async Task<string> GetPathAsync(string containerName, string sessionId)
        {
            var receiver = await _serviceBusClient.AcceptSessionAsync(containerName, sessionId).ConfigureAwait(false);

            var received = await receiver.ReceiveMessageAsync(TimeSpan.FromSeconds(3)).ConfigureAwait(false);

            return received.ToString();
        }
    }
}
