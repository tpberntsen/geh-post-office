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
using System.Web.Http;
using Energinet.DataHub.PostOffice.Application;
using Energinet.DataHub.PostOffice.Outbound.Extensions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;

namespace Energinet.DataHub.PostOffice.Outbound.Functions
{
    public class Peek
    {
        private readonly IDocumentStore _documentStore;

        public Peek(
            IDocumentStore documentStore)
        {
            _documentStore = documentStore;
        }

        [FunctionName("Peek")]
        public async Task<IActionResult> RunAsync(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = null)] HttpRequest request,
            ILogger logger)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));

            var documentQuery = request.GetDocumentQuery();
            if (string.IsNullOrEmpty(documentQuery.Recipient)) return new BadRequestErrorMessageResult("Specify recipient");
            if (string.IsNullOrEmpty(documentQuery.Type)) return new BadRequestErrorMessageResult("Specify type of document");

            logger.LogInformation("processing document query: {documentQuery}", documentQuery);

            var documents = await _documentStore
                .GetDocumentBundleAsync(documentQuery)
                .ConfigureAwait(false);

            if (documents.Count == 0) return new NoContentResult();

            return new OkObjectResult(documents);
        }
    }
}
