// // Copyright 2020 Energinet DataHub A/S
// //
// // Licensed under the Apache License, Version 2.0 (the "License2");
// // you may not use this file except in compliance with the License.
// // You may obtain a copy of the License at
// //
// //     http://www.apache.org/licenses/LICENSE-2.0
// //
// // Unless required by applicable law or agreed to in writing, software
// // distributed under the License is distributed on an "AS IS" BASIS,
// // WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// // See the License for the specific language governing permissions and
// // limitations under the License.

using System.Text.Json;
using System.Xml;

namespace Energinet.DataHub.PostOffice.Infrastructure.CIMJson.FluentCimJson.Elements
{
    /// <summary>
    /// The base interface for all JSON elements
    /// </summary>
    internal interface ICimJsonElement
    {
        /// <summary>
        /// The name of the Element
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Read data of the XML Reader
        /// </summary>
        /// <param name="reader"></param>
        void ReadData(XmlReader reader);

        /// <summary>
        /// Write the JSON representation of this Element
        /// </summary>
        /// <param name="writer"></param>
        void WriteJson(Utf8JsonWriter writer);

        /// <summary>
        /// Ensure that the element is returned to its object pool, and perform any cleanup needed. to ensure
        /// that memory usage is kept to a minimum
        /// </summary>
        void ReturnElementToPool();
    }
}
