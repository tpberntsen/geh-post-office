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
using Energinet.DataHub.PostOffice.Infrastructure.CIMJson.FluentCimJson.Builders.Array;
using Energinet.DataHub.PostOffice.Infrastructure.CIMJson.FluentCimJson.Builders.Elements;
using Energinet.DataHub.PostOffice.Infrastructure.CIMJson.FluentCimJson.Elements;
using Energinet.DataHub.PostOffice.Infrastructure.CIMJson.FluentCimJson.Interfaces.Array;
using Energinet.DataHub.PostOffice.Infrastructure.CIMJson.FluentCimJson.Interfaces.Descriptor;
using Energinet.DataHub.PostOffice.Infrastructure.CIMJson.FluentCimJson.Interfaces.Element;
using Energinet.DataHub.PostOffice.Infrastructure.CIMJson.FluentCimJson.Interfaces.General;
using Energinet.DataHub.PostOffice.Infrastructure.CIMJson.FluentCimJson.Interfaces.Nested;

namespace Energinet.DataHub.PostOffice.Infrastructure.CIMJson.FluentCimJson.Builders.Nested
{
    internal class CimJsonNestedDescriptorBuilder :
        ICimJsonNestedDescriptorBuilder,
        ICimJsonNestedDescriptorBuilderSelectName,
        ICimJsonElementDescriptor
    {
        private readonly List<ICimJsonElementDescriptor> _elements;
        private string _name;
        private bool _isOptional;

        public CimJsonNestedDescriptorBuilder()
        {
            _elements = new List<ICimJsonElementDescriptor>();
            _name = string.Empty;
        }

        public ICimJsonElementDescriptor BuildDescriptor()
        {
            return this;
        }

        public ICimJsonConfigureElementDescriptor AddString(Action<ICimJsonElementDescriptorSelectNameBuilder> configure)
        {
            var builder = new CimJsonStringElementDescriptorBuilder();
            configure(builder);
            var element = builder.BuildDescriptor();
            _elements.Add(element);
            return this;
        }

        public ICimJsonConfigureElementDescriptor AddInteger(Action<ICimJsonElementDescriptorSelectNameBuilder> configure)
        {
            var builder = new CimJsonIntegerElementDescriptorBuilder();
            configure(builder);
            var element = builder.BuildDescriptor();
            _elements.Add(element);
            return this;
        }

        public ICimJsonConfigureElementDescriptor AddArray(Action<ICimJsonArrayDescriptorBuilderSelectName> configure)
        {
            var builder = new CimJsonArrayElementDescriptorBuilder();
            configure(builder);
            var element = builder.BuildDescriptor();
            _elements.Add(element);
            return this;
        }

        public ICimJsonConfigureElementDescriptor AddNested(Action<ICimJsonNestedDescriptorBuilderSelectName> configure)
        {
            var builder = new CimJsonNestedDescriptorBuilder();
            configure(builder);
            var element = builder.BuildDescriptor();
            _elements.Add(element);
            return this;
        }

        public ICimJsonNestedDescriptorBuilder WithName(string name)
        {
            _name = name;
            return this;
        }

        public ICimJsonElement CreateElement()
        {
            return new CimJsonNested(_name, _isOptional, _elements);
        }

        public ICimJsonNestedDescriptorBuilder IsOptional()
        {
            _isOptional = true;
            return this;
        }
    }
}
