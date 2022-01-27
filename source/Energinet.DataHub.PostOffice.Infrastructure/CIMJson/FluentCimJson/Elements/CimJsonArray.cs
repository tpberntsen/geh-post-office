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

using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Xml;
using Energinet.DataHub.PostOffice.Infrastructure.CIMJson.FluentCimJson.Interfaces.Descriptor;

namespace Energinet.DataHub.PostOffice.Infrastructure.CIMJson.FluentCimJson.Elements
{
    internal class CimJsonArray : ICimJsonElement
    {
        private readonly List<ICimJsonElementDescriptor> _elementsDescriptors;
        private ArraySegment<byte> _arrayFragment;
        private bool _arrayElementFound;
        public CimJsonArray(string name, bool isOptional, List<ICimJsonElementDescriptor> elementDescriptors)
        {
            Name = name;
            IsOptional = isOptional;
            _elementsDescriptors = elementDescriptors;
            _arrayElementFound = false;
        }

        public string Name { get; }
        public bool IsOptional { get; }

        public void ReadData(XmlReader reader)
        {
            using MemoryStream jsonStream = new();
            using Utf8JsonWriter jsonWriter = new(jsonStream, new JsonWriterOptions { Indented = false, SkipValidation = true });
            if (ReadToElement(reader, Name, IsOptional, true))
            {
                jsonWriter.WriteStartArray();
                _arrayElementFound = true;
                // Parse all array elements
                while (ReadToElement(reader, Name, IsOptional, true))
                {
                    var singleArrayElements = new List<ICimJsonElement>(_elementsDescriptors.Count);
                    foreach (var elementDescriptor in _elementsDescriptors)
                    {
                        var element = elementDescriptor.CreateElement();
                        var elementFound = ReadToElement(reader, element.Name, element.IsOptional);

                        if (element.IsOptional && !elementFound)
                            continue;

                        element.ReadData(reader);
                        singleArrayElements.Add(element);
                    }

                    jsonWriter.WriteStartObject();
                    singleArrayElements.Sort((x, y) => string.Compare(x.Name, y.Name, StringComparison.OrdinalIgnoreCase));
                    foreach (var jsonElement in singleArrayElements)
                    {
                        jsonElement.WriteJson(jsonWriter);
                        jsonElement.ReturnElementToPool();
                    }

                    jsonWriter.WriteEndObject();
                }

                jsonWriter.WriteEndArray();
                jsonWriter.Flush();
                if (jsonStream.TryGetBuffer(out var buffer))
                {
                    _arrayFragment = buffer;
                }
            }
        }

        public void WriteJson(Utf8JsonWriter writer)
        {
            if (!_arrayElementFound) return;
            writer.WritePropertyName(Name);
            writer.WriteRawValue(_arrayFragment);
        }

        public void ReturnElementToPool()
        {
        }

        /// <summary>
        /// Reads to the next element, if the element is optional it will stop reading after the next found element,
        /// even if it is not an element with requested name.
        /// The reader will be positioned at the found elements, or the next start element if not found
        /// </summary>
        /// <param name="reader"></param>
        /// <param name="elementName"></param>
        /// <param name="isOptional"></param>
        /// <param name="stopAtNewElement">Is used to set whether it will stop at the next new element not matching the requested name
        /// If not set it will read until an element of the requested name is found or EOF is reached</param>
        /// <returns>Whether the element was found</returns>
        private static bool ReadToElement(XmlReader reader, string elementName, bool isOptional, bool stopAtNewElement = false)
        {
            if (reader.IsStartElement() && reader.LocalName == elementName)
            {
                return true;
            }

            while (reader.Read())
            {
                reader.MoveToContent();
                if (reader.NodeType == XmlNodeType.Element && reader.LocalName == elementName)
                {
                    return true;
                }

                if ((isOptional || stopAtNewElement) && (reader.IsStartElement() || reader.NodeType is XmlNodeType.EndElement) && reader.LocalName != elementName)
                    return false;
            }

            return false;
        }
    }
}
