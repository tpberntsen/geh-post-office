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
using System.Threading.Tasks;
using Azure.Messaging.ServiceBus;
using Google.Protobuf;
using Microsoft.Extensions.Configuration;

namespace DataAvailableNotification
{
    public static class Program
    {
        public static async Task Main()
        {
            var configuration = BuildConfiguration();
            var connectionString = configuration.GetSection("Values")["ServiceBusConnectionString"];
            var queueName = configuration.GetSection("Values")["DataAvailableQueueName"];

            await using var client = new ServiceBusClient(connectionString);
            await using var sender = client.CreateSender(queueName);
            using var messageBatch = await sender.CreateMessageBatchAsync().ConfigureAwait(false);

            var msg = DataAvailableModel.CreateProtoContract(Guid.NewGuid().ToString(), Guid.NewGuid().ToString(), SubDomainOrigin.TimeSeries);
            var bytearray = msg.ToByteArray();

            if (!messageBatch.TryAddMessage(new ServiceBusMessage(new BinaryData(bytearray))))
            {
                throw new Exception("The message is too large to fit in the batch.");
            }
            else
            {
                Console.WriteLine($"Message added to batch, uuid: {msg.UUID}, recipient: {msg.Recipient} ");
            }

            await sender.SendMessagesAsync(messageBatch).ConfigureAwait(false);
            Console.WriteLine($"A batch of messages has been published to the queue.");

            Console.WriteLine("Press any key to end the application");

            Console.ReadKey();
        }

        private static IConfiguration BuildConfiguration()
        {
            var builder = new ConfigurationBuilder()
                .AddJsonFile("local.settings.json", false, true)
                .AddEnvironmentVariables();
            var configuration = builder.Build();
            return configuration;
        }
    }
}
