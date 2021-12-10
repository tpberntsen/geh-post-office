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

namespace Energinet.DataHub.PostOffice.Infrastructure.CIMJson.FluentCimJson.Interfaces.Attributes
{
    /// <summary>
    /// The interface that defines an XML attribute conversion to JSON
    /// </summary>
    public interface ICimJsonAttributeFromXml
    {
        /// <summary>
        /// The name of the Attribute in XML and JSON
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Parse the string value from the XML into the attributes type
        /// </summary>
        /// <param name="value"></param>
        void ParseAttribute(string value);

        /// <summary>
        /// Write the JSON representation into the JSON writer
        /// </summary>
        /// <param name="writer"></param>
        void WriteJson(Utf8JsonWriter writer);
    }
}
