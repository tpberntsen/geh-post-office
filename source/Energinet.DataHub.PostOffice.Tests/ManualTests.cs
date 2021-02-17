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
using Google.Protobuf;
using Google.Protobuf.WellKnownTypes;
using Xunit;

namespace Energinet.DataHub.PostOffice.Tests
{
    public class ManualTests
    {
        [Fact]
        public async Task Foo()
        {
            const string topicName = "marketdata";
            var faker = new Faker();
            await using (ServiceBusClient client = new ServiceBusClient("Endpoint=sb://sbn-inbound-postoffice-endk-d.servicebus.windows.net/;SharedAccessKeyName=sbtaur-inbound-sender;SharedAccessKey=G+l7o/v0TExTuPmOqkpca8pE0TcKnkCVdI/6Yn/qMr8=;EntityPath=marketdata"))
            {
                var document = new Energinet.DataHub.PostOffice.Contracts.Document
                {
                    EffectuationDate = Timestamp.FromDateTimeOffset(DateTimeOffset.UtcNow.AddDays(1)),
                    Recipient = "me",
                    Type = "changeofsupplier",
                    Content = "{\"document\":\"" + faker.Rant.Review() + "\"}",
                };
                ServiceBusSender sender = client.CreateSender(topicName);
                await sender.SendMessageAsync(new ServiceBusMessage(document.ToByteArray())).ConfigureAwait(false);
            }
        }
    }
}
