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
using Energinet.DataHub.PostOffice.Application;
using Energinet.DataHub.PostOffice.Inbound.Parsing;
using Microsoft.Azure.ServiceBus;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;

namespace Energinet.DataHub.PostOffice.Inbound.Functions
{
    public class MarketInbox
    {
        private const string FunctionName = "MarketInbox";

        private readonly InputParser _inputParser;
        private readonly IDocumentStore _documentStore;

        public MarketInbox(
            InputParser inputParser,
            IDocumentStore documentStore)
        {
            _inputParser = inputParser;
            _documentStore = documentStore;
        }

        [FunctionName(FunctionName)]
        public async Task RunAsync(
            [ServiceBusTrigger(
                "%INBOUND_QUEUE_MARKETDATA_TOPIC_NAME%",
                "%INBOUND_QUEUE_MARKETDATA_SUBSCRIPTION_NAME%",
                Connection = "INBOUND_QUEUE_CONNECTION_STRING")] Message message,
            ILogger logger)
        {
            if (message == null) throw new ArgumentNullException(nameof(message));
            logger.LogInformation($"C# ServiceBus topic trigger function processed message in {FunctionName}.");

            try
            {
                var topicName = Environment.GetEnvironmentVariable("INBOUND_QUEUE_MARKETDATA_TOPIC_NAME");
                if (string.IsNullOrEmpty(topicName)) throw new InvalidOperationException("TopicName is null");

                var document = await _inputParser.ParseAsync(message.Body).ConfigureAwait(false);
                await _documentStore.SaveDocumentAsync(document, topicName).ConfigureAwait(false);

                logger.LogInformation("Got document: {document}", document);
            }
            catch (Exception exception)
            {
                logger.LogError(exception, $"Error in {FunctionName}");
                throw;
            }
        }
    }
}
