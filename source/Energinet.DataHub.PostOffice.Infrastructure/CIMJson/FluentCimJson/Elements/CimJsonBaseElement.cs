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
using System.Linq;
using System.Text.Json;
using System.Xml;
using Energinet.DataHub.PostOffice.Infrastructure.CIMJson.FluentCimJson.Interfaces.Attributes;

namespace Energinet.DataHub.PostOffice.Infrastructure.CIMJson.FluentCimJson.Elements
{
    internal abstract class CimJsonBaseElement : ICimJsonElement
    {
        protected CimJsonBaseElement()
        {
            Attributes = new List<ICimJsonAttributeFromXml>();
            AttributeDescriptors = new List<ICimJsonAttributeDescriptor>();
            RequiresValueProperty = false;
            Name = string.Empty;
        }

        public string Name { get; set; }
        public bool IsOptional { get; set; }
        public bool RequiresValueProperty { get; set; }
        public List<ICimJsonAttributeFromXml> Attributes { get; }
        public List<ICimJsonAttributeDescriptor> AttributeDescriptors { get; set; }

        public virtual void ReadData(XmlReader reader)
        {
            ParseAttributes(reader);
            ParseString(reader.ReadElementContentAsString());
        }

        public abstract void WriteJson(Utf8JsonWriter writer);
        public abstract void ReturnElementToPool();
        protected abstract void ParseString(string value);
        private void ParseAttributes(XmlReader reader)
        {
            if (!AttributeDescriptors.Any()) return;

            if (AttributeDescriptors.Count == 1)
            {
                var attribute = AttributeDescriptors.First().CreateAttribute();
                while (reader.MoveToNextAttribute())
                {
                    if (!string.Equals(attribute.Name, reader.LocalName, StringComparison.OrdinalIgnoreCase)) continue;
                    attribute.ParseAttribute(reader.Value);
                    Attributes.Add(attribute);
                }
            }
            else
            {
                Dictionary<string, ICimJsonAttributeFromXml> attributeLookup = new();
                foreach (var attributeDescriptor in AttributeDescriptors)
                {
                    var attribute = attributeDescriptor.CreateAttribute();
                    attributeLookup.Add(attribute.Name, attribute);
                }

                while (reader.MoveToNextAttribute())
                {
                   if (!attributeLookup.TryGetValue(reader.LocalName, out var attribute)) continue;
                   attribute.ParseAttribute(reader.Value);
                   Attributes.Add(attribute);
                }
            }

            AttributeDescriptors.Clear();
            reader.MoveToElement();
        }
    }
}
