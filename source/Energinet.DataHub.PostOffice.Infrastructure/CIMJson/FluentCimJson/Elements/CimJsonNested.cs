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
using System.Text.Json;
using System.Xml;
using Energinet.DataHub.PostOffice.Infrastructure.CIMJson.FluentCimJson.Interfaces.Descriptor;

namespace Energinet.DataHub.PostOffice.Infrastructure.CIMJson.FluentCimJson.Elements
{
    internal class CimJsonNested : ICimJsonElement
    {
        private readonly List<ICimJsonElement> _elements;
        private readonly List<ICimJsonElementDescriptor> _elementsDescriptors;

        public CimJsonNested(
            string name,
            List<ICimJsonElementDescriptor> elementsDescriptors)
        {
            _elements = new List<ICimJsonElement>(elementsDescriptors.Count);
            _elementsDescriptors = elementsDescriptors;
            Name = name;
        }

        public string Name { get; }
        public void ReadData(XmlReader reader)
        {
            ReadToElement(reader, Name);
            foreach (var elementDescriptor in _elementsDescriptors)
            {
                var element = elementDescriptor.CreateElement();
                ReadToElement(reader, element.Name);
                element.ReadData(reader);
                _elements.Add(element);
            }
        }

        public void WriteJson(Utf8JsonWriter writer)
        {
            writer.WriteStartObject(Name);
            _elements.Sort((x, y) => string.Compare(x.Name, y.Name, StringComparison.OrdinalIgnoreCase));
            foreach (var element in _elements)
            {
                element.WriteJson(writer);
                element.ReturnElementToPool();
            }

            writer.WriteEndObject();
            _elements.Clear();
        }

        public void ReturnElementToPool()
        {
            foreach (var element in _elements)
            {
                element.ReturnElementToPool();
            }
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
