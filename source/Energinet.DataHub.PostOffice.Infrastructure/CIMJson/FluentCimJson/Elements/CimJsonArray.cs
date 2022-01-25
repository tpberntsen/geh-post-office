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
            var shouldWriteStartArrayElement = true;
            using MemoryStream jsonStream = new();
            using Utf8JsonWriter jsonWriter = new(jsonStream, new JsonWriterOptions { Indented = false, SkipValidation = true });
            while (!reader.EOF)
            {
                var singleArrayElements = new List<ICimJsonElement>(_elementsDescriptors.Count);
                if (reader.IsStartElement() && reader.LocalName == Name)
                {
                    if (shouldWriteStartArrayElement)
                    {
                        jsonWriter.WriteStartArray();
                        shouldWriteStartArrayElement = false;
                        _arrayElementFound = true;
                    }

                    singleArrayElements.Clear();
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
                else if (reader.NodeType is XmlNodeType.EndElement or XmlNodeType.Element && reader.LocalName != Name)
                {
                    if (IsOptional && !_arrayElementFound)
                        break;

                    jsonWriter.WriteEndArray();
                    jsonWriter.Flush();
                    if (jsonStream.TryGetBuffer(out var buffer))
                    {
                        _arrayFragment = buffer;
                    }

                    break;
                }

                if (!reader.IsStartElement())
                {
                    reader.Read();
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
        /// Reads to the next element, if the element is optional it will stop reading after the next found element
        /// </summary>
        /// <param name="reader"></param>
        /// <param name="elementName"></param>
        /// <param name="isOptional"></param>
        /// <returns>Whether the element was found</returns>
        private static bool ReadToElement(XmlReader reader, string elementName, bool isOptional)
        {
            if (reader.IsStartElement() && reader.LocalName == elementName)
            {
                return true;
            }

            if (isOptional && reader.IsStartElement() && reader.LocalName != elementName)
                return false;

            while (reader.Read())
            {
                if (reader.NodeType == XmlNodeType.Element && reader.LocalName == elementName)
                {
                    return true;
                }

                if (isOptional && (reader.IsStartElement() || reader.NodeType is XmlNodeType.EndElement) && reader.LocalName != elementName)
                    return false;
            }

            return false;
        }
    }
}
