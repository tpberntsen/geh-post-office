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
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;

namespace Energinet.DataHub.PostOffice.Outbound.Functions
{
    public class Dequeue
    {
        private readonly IDocumentStore _documentStore;

        public Dequeue(
            IDocumentStore documentStore)
        {
            _documentStore = documentStore;
        }

        [FunctionName("Dequeue")]
        public async Task<IActionResult> RunAsync(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = null)] HttpRequest request,
            ILogger logger)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));
            if (!request.Query.ContainsKey("id")) throw new InvalidOperationException("Request must include 'id'");
            if (!request.Query.ContainsKey("recipient")) throw new InvalidOperationException("Request must include 'recipient'");

            var bundle = request.Query["id"].ToString();
            var recipient = request.Query["recipient"].ToString();

            logger.LogInformation("processing document dequeue: {id}", bundle);

            var didDeleteDocuments = await _documentStore
                .DeleteDocumentsAsync(bundle, recipient)
                .ConfigureAwait(false);

            return didDeleteDocuments
                ? (IActionResult)new OkResult()
                : (IActionResult)new NotFoundResult();
        }
    }
}
