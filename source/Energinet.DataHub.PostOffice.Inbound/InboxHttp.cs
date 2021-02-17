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
using Google.Protobuf;
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
        private readonly InputParser _inputParser;
        private readonly IDocumentStore _documentStore;

        public Inbox(
            InputParser inputParser,
            IDocumentStore documentStore)
        {
            _inputParser = inputParser;
            _documentStore = documentStore;
        }

        [FunctionName("Inbox")]
        public async Task<IActionResult> RunAsync(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = null)] HttpRequest request,
            ILogger logger)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));

            using var reader = new StreamReader(request.Body);
            var content = await reader.ReadToEndAsync().ConfigureAwait(false);

            var input = new Contracts.Document
            {
                Content = content,
                EffectuationDate = Instant.FromUtc(2021, 2, 17, 8, 0).ToTimestamp(),
                Recipient = "me",
                Type = "Market",
            };
            var bytes = input.ToByteArray();

            logger.LogInformation("C# HTTP trigger function processed a request.");

            var document = _inputParser.Parse(bytes);
            await _documentStore.SaveDocumentAsync(document).ConfigureAwait(false);

            logger.LogInformation("Got document: {document}", document);

            return new OkObjectResult(document);
        }
    }
}
