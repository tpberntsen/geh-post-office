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

namespace Energinet.DataHub.PostOffice.Application
{
    /// <summary>
    /// Query for document store.
    /// </summary>
    public class DocumentQuery
    {
        public DocumentQuery(string recipient, string type, int pageSize = 1)
        {
            Recipient = recipient;
            Type = type;
            PageSize = pageSize;
        }

        /// <summary>
        /// Recipient.
        /// </summary>
        public string Recipient { get; set; }

        /// <summary>
        /// Type of the documents to fetch.
        /// </summary>
        public string Type { get; set; }

        /// <summary>
        /// Maximum number of items to return.
        /// </summary>
        public int PageSize { get; set; }
    }
}
