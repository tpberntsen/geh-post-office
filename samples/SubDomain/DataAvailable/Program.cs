// Copyright 2020 Energinet DataHub A/S
//
// Licensed under the Apache License, Version 2.0 (the "License2");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//     http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using System;
using System.Threading;
using System.Threading.Tasks;
using Energinet.DataHub.MessageHub.Client;
using Energinet.DataHub.MessageHub.Client.DataAvailable;
using Energinet.DataHub.MessageHub.Client.Factories;
using Energinet.DataHub.MessageHub.Model.Model;
using Microsoft.Extensions.Configuration;

namespace DataAvailableNotification
{
    public static class Program
    {
        public static async Task Main(string[] args)
        {
            var configuration = BuildConfiguration(args);

            var configurationSection = configuration.GetSection("Values");
            var connectionString = configurationSection["ServiceBusConnectionString"];
            var dataAvailableQueueName = configurationSection["DATAAVAILABLE_QUEUE_NAME"];
            var domainReplyQueueName = configurationSection["DOMAIN_REPLY_QUEUE_NAME"];

            var recipient = configurationSection["recipient"];
            var origin = configurationSection["origin"];
            var messageType = configurationSection["type"];
            var interval = int.TryParse(configurationSection["interval"], out var intervalParsed) ? intervalParsed : 1;
            var domainOrigin = origin != null ? Enum.Parse<DomainOrigin>(origin, true) : DomainOrigin.TimeSeries;

            var serviceBusClientFactory = new ServiceBusClientFactory(connectionString);
            await using var azureServiceFactory = new AzureServiceBusFactory(serviceBusClientFactory);
            var messageHubConfig = new MessageHubConfig(dataAvailableQueueName, domainReplyQueueName);

            var dataAvailableNotificationSender = new DataAvailableNotificationSender(azureServiceFactory, messageHubConfig);

            for (var i = 0; i < interval; i++)
            {
                var msgDto = CreateDto(domainOrigin, messageType, recipient);
                var correlationId = Guid.NewGuid().ToString();

                Console.WriteLine($"Sending message number: {i + 1}.");

                await dataAvailableNotificationSender.SendAsync(correlationId, msgDto).ConfigureAwait(false);

                if (i + 1 < interval)
                    Thread.Sleep(100);
            }

            Console.WriteLine("Message sender completed.");
        }

        private static DataAvailableNotificationDto CreateDto(DomainOrigin origin, string messageType, string recipient)
        {
            var msgDto = DataAvailableNotificationFactory.CreateOriginDto(origin, messageType, recipient);
            return msgDto;
        }

        private static IConfiguration BuildConfiguration(string[] args)
        {
            var builder = new ConfigurationBuilder()
                .AddJsonFile("local.settings.json", false, true)
                .AddEnvironmentVariables()
                .AddCommandLine(args);
            var configuration = builder.Build();
            return configuration;
        }
    }
}
