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
using Bogus;
using Energinet.DataHub.PostOffice.Tests.Tooling;
using Google.Protobuf;
using Google.Protobuf.WellKnownTypes;

namespace Energinet.DataHub.PostOffice.Tests
{
    public class ManualTests
    {
        private readonly Faker _faker;

        public ManualTests()
        {
            LocalSettings.SetupEnvironment();
            _faker = new Faker();
        }

        [RunnableInDebugOnly]
        public async Task SendDocument()
        {
            const int numberOfMessages = 1;
            const string topicName = "marketdata";
            var connectionString = Environment.GetEnvironmentVariable("SERVICEBUS_CONNECTION_STRING");

            await using ServiceBusClient client = new ServiceBusClient(connectionString);
            var sender = client.CreateSender(topicName);
            for (int i = 0; i < numberOfMessages; i++)
            {
                await SendMessagesAsync(sender).ConfigureAwait(false);
            }
        }

        private async Task SendMessagesAsync(ServiceBusSender sender)
        {
            var document = new Contracts.Document
            {
                EffectuationDate = Timestamp.FromDateTimeOffset(_faker.Date.Soon()),
                Recipient = _faker.PickRandom("greenenergy", "vELkommen"),
                Type = _faker.PickRandom("changeofsupplier"), //, "movein", "moveout"),
                Content = "{\"document\":\"" + _faker.Rant.Review() + "\"}",
                Version = "1.0.0",
            };
            await sender.SendMessageAsync(new ServiceBusMessage(document.ToByteArray())).ConfigureAwait(false);
        }
    }
}
