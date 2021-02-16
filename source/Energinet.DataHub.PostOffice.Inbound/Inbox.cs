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
using System.IO;
using System.Threading.Tasks;
using Energinet.DataHub.PostOffice.Application;
using Energinet.DataHub.PostOffice.Domain;
using Google.Protobuf;
using GreenEnergyHub.Messaging.Transport;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using NodaTime;
using NodaTime.Serialization.Protobuf;

namespace Energinet.DataHub.PostOffice.Inbound
{
    public class Inbox
    {
        private readonly MessageExtractor _messageExtractor;
        private readonly IDocumentStore _documentStore;

        public Inbox(
            MessageExtractor messageExtractor,
            IDocumentStore documentStore)
        {
            _messageExtractor = messageExtractor;
            _documentStore = documentStore;
        }

        [FunctionName("Inbox")]
        public async Task<IActionResult> RunAsync(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = null)] HttpRequest request,
            ILogger logger)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));

            var document = new Contracts.Document
            {
                Content = "{ \"hello\":\"world\" }",
                EffectuationDate = Instant.FromUtc(2021, 2, 17, 8, 0).ToTimestamp(),
                Recipient = "me",
                Type = "Market",
            };

            logger.LogInformation("C# HTTP trigger function processed a request.");

            var bytes = document.ToByteArray();
            var message = (Document)await _messageExtractor.ExtractAsync(bytes).ConfigureAwait(false);
            await _documentStore.SaveDocumentAsync(message).ConfigureAwait(false);

            logger.LogInformation("Got message: {message}", message);

            return new OkObjectResult(message);
        }
    }
}
