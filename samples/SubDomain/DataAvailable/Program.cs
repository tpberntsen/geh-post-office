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
using System.Diagnostics;
using System.Linq;
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

            var recipient = configuration["recipient"];
            var origin = configuration["origin"];
            var messageType = configuration["type"];
            var noOfMessagesToSend = int.TryParse(configuration["noOfMessagesToSend"], out var intervalParsed) ? intervalParsed : 1;
            var workers = int.TryParse(configuration["workers"], out var workersParsed) ? workersParsed : 1;
            var domainOrigin = origin != null ? Enum.Parse<DomainOrigin>(origin, true) : DomainOrigin.TimeSeries;

            var sw = Stopwatch.StartNew();
            await Task.WhenAll(Enumerable.Range(0, workers).Select(async w =>
            {
                    var serviceBusClientFactory = new ServiceBusClientFactory(connectionString);
                    var azureServiceFactory = new AzureServiceBusFactory(serviceBusClientFactory);
                    var messageHubConfig = new MessageHubConfig(dataAvailableQueueName, domainReplyQueueName);

                    var dataAvailableNotificationSender = new DataAvailableNotificationSender(azureServiceFactory, messageHubConfig);

                    for (var i = 0; i < noOfMessagesToSend / workers; i++)
                    {
                        Console.WriteLine($"[{w}]: Sending message number: {i + 1}.");
                        await dataAvailableNotificationSender.SendAsync(CreateDto(domainOrigin, messageType, recipient)).ConfigureAwait(false);
                    }
            })).ConfigureAwait(false);

            Console.WriteLine($"Message sender completed. {noOfMessagesToSend} sent in {sw.ElapsedMilliseconds} ms. with {workers} workes");
        }

        private static DataAvailableNotificationDto CreateDto(DomainOrigin origin, string messageType, string recipient)
        {
            return DataAvailableNotificationFactory.CreateOriginDto(origin, messageType, recipient);
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
