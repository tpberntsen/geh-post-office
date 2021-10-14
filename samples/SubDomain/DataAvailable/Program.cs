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
using Energinet.DataHub.MessageHub.Client.Model;
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

            var recipient = configuration["recipient"];
            var origin = configuration["origin"];
            var messageType = configuration["type"];
            var interval = int.TryParse(configuration["interval"], out var intervalParsed) ? intervalParsed : 1;
            var domainOrigin = origin != null ? Enum.Parse<DomainOrigin>(origin, true) : DomainOrigin.TimeSeries;

            var serviceBusClientFactory = new ServiceBusClientFactory(connectionString);
            var messageHubConfig = new MessageHubConfig(dataAvailableQueueName, domainReplyQueueName);

            await using var dataAvailableNotificationSender = new DataAvailableNotificationSender(serviceBusClientFactory, messageHubConfig);

            for (var i = 0; i < interval; i++)
            {
                var msgDto = CreateDto(domainOrigin, messageType, recipient);

                await dataAvailableNotificationSender.SendAsync(msgDto).ConfigureAwait(false);

                if (i + 1 < interval)
                    Thread.Sleep(5000);
            }

            Console.WriteLine("A batch of messages has been published to the queue.");
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
