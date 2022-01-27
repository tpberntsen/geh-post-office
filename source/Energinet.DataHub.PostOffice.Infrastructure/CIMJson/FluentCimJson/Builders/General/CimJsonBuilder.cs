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
using Energinet.DataHub.PostOffice.Infrastructure.CIMJson.FluentCimJson.Elements;
using Energinet.DataHub.PostOffice.Infrastructure.CIMJson.FluentCimJson.Interfaces.Descriptor;
using Energinet.DataHub.PostOffice.Infrastructure.CIMJson.FluentCimJson.Interfaces.General;

namespace Energinet.DataHub.PostOffice.Infrastructure.CIMJson.FluentCimJson.Builders.General
{
    internal class CimJsonBuilder : ICimJsonAddXmlDataSource
    {
        private readonly List<ICimJsonElementDescriptor> _elementDescriptors;
        private XmlReader? _xmlReader;
        private CimJsonBuilder()
        {
            _elementDescriptors = new List<ICimJsonElementDescriptor>(20);
        }

        public static ICimJsonAddXmlDataSource Create() => new CimJsonBuilder();

        public CimJsonBuilder WithXmlReader(Action<ICimJsonConfigureElementDescriptor> configure, XmlReader reader)
        {
            var builder = new CimJsonElementDescriptorBuilder();
            configure(builder);
            _elementDescriptors.AddRange(builder.BuildDescriptor());
            _xmlReader = reader;
            return this;
        }

        public void Build(Utf8JsonWriter jsonWriter)
        {
            if (_xmlReader is not null)
            {
                var elementsToWrite = new List<ICimJsonElement>(_elementDescriptors.Count);
                foreach (var elementDescriptor in _elementDescriptors)
                {
                    var element = elementDescriptor.CreateElement();
                    if (ReadToElement(element.Name, element.IsOptional))
                    {
                        element.ReadData(_xmlReader);
                        elementsToWrite.Add(element);
                    }
                }

                elementsToWrite.Sort((x, y) => string.Compare(x.Name, y.Name, StringComparison.OrdinalIgnoreCase));
                foreach (var jsonElement in elementsToWrite)
                {
                    jsonElement.WriteJson(jsonWriter);
                    jsonElement.ReturnElementToPool();
                }
            }
            else
            {
                throw new InvalidOperationException(
                    "XmlReader was not correctly initialized or is null, we can't read an XML file without a valid reader");
            }
        }

        private bool ReadToElement(string elementName, bool isOptional)
        {
            if (_xmlReader is null) return false;
            if (_xmlReader.IsStartElement() && _xmlReader.LocalName == elementName)
            {
                return true;
            }

            if (isOptional && _xmlReader.IsStartElement() && _xmlReader.LocalName != elementName)
                return false;

            while (_xmlReader.Read())
            {
                _xmlReader.MoveToContent();
                if (_xmlReader.NodeType == XmlNodeType.Element && _xmlReader.LocalName == elementName)
                {
                    return true;
                }

                if (isOptional && _xmlReader.IsStartElement() && _xmlReader.LocalName != elementName)
                    return false;
            }

            return false;
        }
    }
}
