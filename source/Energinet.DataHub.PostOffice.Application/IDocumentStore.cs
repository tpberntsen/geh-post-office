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

using System.Collections.Generic;
using System.Threading.Tasks;
using Energinet.DataHub.PostOffice.Domain;

namespace Energinet.DataHub.PostOffice.Application
{
    /// <summary>
    /// Crud operations for the document data store.
    /// </summary>
    public interface IDocumentStore
    {
        /// <summary>
        /// Get documents.
        /// </summary>
        /// <param name="documentQuery">The documentQuery to get documents by.</param>
        /// <returns>A list of documents matching the documentQuery parameters.</returns>
        Task<IList<Document>> GetDocumentsAsync(DocumentQuery documentQuery);

        /// <summary>
        /// Get documents.
        /// </summary>
        /// <param name="documentQuery">The documentQuery to get documents by.</param>
        /// <returns>A list of documents matching the documentQuery parameters.</returns>
        Task<IList<Document>> GetDocumentBundleAsync(DocumentQuery documentQuery);

        /// <summary>
        /// Save a document.
        /// </summary>
        /// <param name="document">The document to save.</param>
        Task SaveDocumentAsync(Document document);

        /// <summary>
        /// Delete documents
        /// </summary>
        /// <param name="bundle">Id of bundle to delete</param>
        /// <param name="recipient">TODO: Should this be here or via some other auth lookup?</param>
        Task<bool> DeleteDocumentsAsync(string bundle, string recipient);
    }
}
