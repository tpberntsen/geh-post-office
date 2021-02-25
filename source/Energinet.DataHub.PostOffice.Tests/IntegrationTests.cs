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
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Azure.Messaging.ServiceBus;
using Bogus;
using Energinet.DataHub.PostOffice.Contracts;
using Energinet.DataHub.PostOffice.Tests.Tooling;
using FluentAssertions;
using Google.Protobuf;
using Google.Protobuf.WellKnownTypes;
using Newtonsoft.Json;
using Xunit;

namespace Energinet.DataHub.PostOffice.Tests
{
    [Trait("Category", "Integration")]
    public class IntegrationTests
    {
        public IntegrationTests()
        {
            LocalSettings.SetupEnvironment();
        }

        [RunnableInDebugOnly]
        public async Task ValidateDocumentFlow()
        {
            using var httpClient = CreateHttpClient();

            // No documents should exists at the beginning.
            (await PeekFiveDocumentsFromMarketData(httpClient).ConfigureAwait(false))
                .StatusCode.Should().Be(HttpStatusCode.NoContent);

            // Create 10 documents.
            await CreateMarketDataDocuments().ConfigureAwait(false);

            // First peek, should be OK and contain 5 documents.
            var firstPeek = await PeekFiveDocumentsFromMarketData(httpClient).ConfigureAwait(false);
            firstPeek.StatusCode.Should().Be(HttpStatusCode.OK);
            var firstContent = await firstPeek.Content.ReadAsJsonAsync<Domain.Document[]>().ConfigureAwait(false);
            var firstBundle = firstContent[0].Bundle;
            firstBundle.Should().NotBeNull().And.NotBeEmpty();
            firstContent.Should().HaveCount(5);

            // Dequeue peeked items, should be OK.
            (await DequeueMarketData(httpClient, firstBundle!).ConfigureAwait(false))
                .StatusCode.Should().Be(HttpStatusCode.OK);

            // Dequeue peeked items again, should not do anything.
            (await DequeueMarketData(httpClient, firstBundle!).ConfigureAwait(false))
                .StatusCode.Should().Be(HttpStatusCode.NotFound);

            // Second peek, should be OK and not contain the same as the first peek.
            var secondPeek = await PeekFiveDocumentsFromMarketData(httpClient).ConfigureAwait(false);
            secondPeek.StatusCode.Should().Be(HttpStatusCode.OK);
            var secondContent = await secondPeek.Content.ReadAsJsonAsync<Domain.Document[]>().ConfigureAwait(false);
            var secondBundle = secondContent[0].Bundle;
            secondBundle.Should().NotBeSameAs(firstBundle);

            // Third peek, should be OK but contain the same as the second peek, since we didn't dequeue.
            var thirdPeek = await PeekFiveDocumentsFromMarketData(httpClient).ConfigureAwait(false);
            thirdPeek.StatusCode.Should().Be(HttpStatusCode.OK);
            var thirdContent = await thirdPeek.Content.ReadAsJsonAsync<Domain.Document[]>().ConfigureAwait(false);
            var thirdBundle = thirdContent[0].Bundle;
            thirdBundle.Should().BeEquivalentTo(secondBundle);
            thirdContent[0].Content.Should().BeEquivalentTo(secondContent[0].Content);

            // Final dequeue
            (await DequeueMarketData(httpClient, secondBundle!).ConfigureAwait(false))
                .StatusCode.Should().Be(HttpStatusCode.OK);

            // No documents remaining
            (await PeekFiveDocumentsFromMarketData(httpClient).ConfigureAwait(false))
                .StatusCode.Should().Be(HttpStatusCode.NoContent);
        }

        private static async Task CreateMarketDataDocuments()
        {
            var connectionString = Environment.GetEnvironmentVariable("SERVICEBUS_CONNECTION_STRING");
            const int numberOfDocuments = 10;
            const string topicName = "marketdata";
            var changeOfSupplerDocuments = CreateDocuments("me", "changeofsupplier", numberOfDocuments);
            await using ServiceBusClient client = new ServiceBusClient(connectionString);
            var sender = client.CreateSender(topicName);
            foreach (var document in changeOfSupplerDocuments)
            {
                await sender.SendMessageAsync(new ServiceBusMessage(document.ToByteArray())).ConfigureAwait(false);
            }

            await Task.Delay(5000).ConfigureAwait(false);
        }

        private static HttpClient CreateHttpClient()
        {
            var httpClient = HttpClientFactory.Create();
            var endpointUrl = Environment.GetEnvironmentVariable("OUTBOUND_URL");
            httpClient.BaseAddress = new Uri(endpointUrl!);
            return httpClient;
        }

        private static async Task<HttpResponseMessage> PeekFiveDocumentsFromMarketData(HttpClient httpClient)
        {
            var response = await httpClient
                .GetAsync(new Uri("Peek?recipient=me&type=marketdata&pageSize=5", UriKind.Relative))
                .ConfigureAwait(false);
            return response;
        }

        private static async Task<HttpResponseMessage> DequeueMarketData(HttpClient httpClient, string bundle)
        {
            var response = await httpClient
                .DeleteAsync(new Uri($"Dequeue?recipient=me&bundle={bundle}", UriKind.Relative))
                .ConfigureAwait(false);
            return response;
        }

        private static List<Contracts.Document> CreateDocuments(string recipient, string type, int numberOfDocuments)
        {
            var documents = new List<Contracts.Document>();
            for (var index = 0; index < numberOfDocuments; index++)
            {
                documents.Add(new Document
                {
                    EffectuationDate = Timestamp.FromDateTimeOffset(DateTimeOffset.Now.AddMinutes(index)),
                    Recipient = recipient,
                    Type = type,
                    Content = "{\"document\":\"" + index + "\"}",
                    Version = "1.0.0",
                });
            }

            return documents;
        }
    }
}
