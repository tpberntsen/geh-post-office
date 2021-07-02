using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Azure.Messaging.ServiceBus;
using Energinet.DataHub.PostOffice.Application;
using Energinet.DataHub.PostOffice.Application.GetMessage;
using Energinet.DataHub.PostOffice.Contracts;

namespace Energinet.DataHub.PostOffice.Infrastructure.GetMessage
{
    public class SendMessageToServiceBus : ISendMessageToServiceBus
    {
        // private readonly string _serviceBusConnectionString = "Endpoint=sb://sbn-inbound-postoffice-endk-u.servicebus.windows.net/;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=YMwhKlYdf2hXZ+ufhk/EZ42kYh6RyJzeHxTPt+Stwc0=";
        private ServiceBusClient? _serviceBusClient;
        private ServiceBusSender? _sender;

        public SendMessageToServiceBus(ServiceBusClient serviceBusClient)
        {
            _serviceBusClient = serviceBusClient;
        }

        public async Task SendMessageAsync(IList<string> collection, string containerName, string sessionId)
        {
            if (collection == null) throw new ArgumentNullException(nameof(collection));
            if (_serviceBusClient != null) _sender = _serviceBusClient.CreateSender(containerName);

            var message = new ServiceBusMessage(collection.ToString()) { SessionId = sessionId };

            message.ReplyToSessionId = message.SessionId;
            message.ReplyTo = containerName;

            if (_sender != null) await _sender.SendMessageAsync(message).ConfigureAwait(false);
        }
    }
}
