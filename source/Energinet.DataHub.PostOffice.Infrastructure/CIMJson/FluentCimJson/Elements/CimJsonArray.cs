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
        public CimJsonArray(string name, List<ICimJsonElementDescriptor> elementDescriptors)
        {
            Name = name;
            _elementsDescriptors = elementDescriptors;
        }

        public string Name { get; }

        public void ReadData(XmlReader reader)
        {
            using MemoryStream jsonStream = new();
            using Utf8JsonWriter jsonWriter = new(jsonStream, new JsonWriterOptions { Indented = false, SkipValidation = true });
            jsonWriter.WriteStartArray();
            while (!reader.EOF)
            {
                var singleArrayElement = new List<ICimJsonElement>(_elementsDescriptors.Count);
                if (reader.IsStartElement() && reader.LocalName == Name)
                {
                    singleArrayElement.Clear();
                    foreach (var elementDescriptor in _elementsDescriptors)
                    {
                        var element = elementDescriptor.CreateElement();
                        ReadToElement(reader, element.Name);
                        element.ReadData(reader);
                        singleArrayElement.Add(element);
                    }

                    jsonWriter.WriteStartObject();
                    singleArrayElement.Sort((x, y) => string.Compare(x.Name, y.Name, StringComparison.OrdinalIgnoreCase));
                    foreach (var jsonElement in singleArrayElement)
                    {
                        jsonElement.WriteJson(jsonWriter);
                        jsonElement.ReturnElementToPool();
                    }

                    jsonWriter.WriteEndObject();
                }
                else if (reader.NodeType == XmlNodeType.Element && reader.LocalName != Name)
                {
                    jsonWriter.WriteEndArray();
                    jsonWriter.Flush();
                    if (jsonStream.TryGetBuffer(out var buffer))
                    {
                        _arrayFragment = buffer;
                    }

                    _elementsDescriptors.Clear();
                    break;
                }

                reader.Read();
            }
        }

        public void WriteJson(Utf8JsonWriter writer)
        {
            writer.WritePropertyName(Name);
            writer.WriteRawValue(_arrayFragment);
        }

        public void ReturnElementToPool()
        {
        }

        private static void ReadToElement(XmlReader reader, string elementName)
        {
            if (reader.IsStartElement() && reader.LocalName == elementName)
            {
                return;
            }

            while (reader.Read())
            {
                if (reader.NodeType == XmlNodeType.Element && reader.LocalName == elementName)
                {
                    return;
                }
            }
        }
    }
}
