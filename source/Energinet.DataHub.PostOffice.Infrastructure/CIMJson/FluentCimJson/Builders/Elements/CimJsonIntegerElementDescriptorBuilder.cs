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
using Energinet.DataHub.PostOffice.Infrastructure.CIMJson.FluentCimJson.Builders.Attributes;
using Energinet.DataHub.PostOffice.Infrastructure.CIMJson.FluentCimJson.Elements;
using Energinet.DataHub.PostOffice.Infrastructure.CIMJson.FluentCimJson.Factories;
using Energinet.DataHub.PostOffice.Infrastructure.CIMJson.FluentCimJson.Interfaces.Attributes;
using Energinet.DataHub.PostOffice.Infrastructure.CIMJson.FluentCimJson.Interfaces.Descriptor;
using Energinet.DataHub.PostOffice.Infrastructure.CIMJson.FluentCimJson.Interfaces.Element;

namespace Energinet.DataHub.PostOffice.Infrastructure.CIMJson.FluentCimJson.Builders.Elements
{
    internal class CimJsonIntegerElementDescriptorBuilder :
        ICimJsonElementDescriptorBuilder,
        ICimJsonElementDescriptor,
        ICimJsonElementDescriptorSelectNameBuilder
    {
        private readonly List<ICimJsonAttributeDescriptor> _attributes;
        private bool _wrapValueInProperty;
        private string _name;
        private bool _isOptional;

        public CimJsonIntegerElementDescriptorBuilder()
        {
            _name = string.Empty;
            _wrapValueInProperty = false;
            _attributes = new List<ICimJsonAttributeDescriptor>();
        }

        public ICimJsonElementDescriptorBuilder WithValueWrappedInProperty()
        {
            _wrapValueInProperty = true;
            return this;
        }

        public ICimJsonElementDescriptorBuilder WithAttributes(Action<ICimJsonAttributeDescriptorBuilder> configure)
        {
            var builder = new CimJsonAttributeDescriptorBuilder();
            configure(builder);
            _attributes.AddRange(builder.BuildDescriptors());
            return this;
        }

        public ICimJsonElementDescriptorBuilder IsOptional()
        {
            _isOptional = true;
            return this;
        }

        public ICimJsonElement CreateElement()
        {
            return CimJsonElementFactory.CreateInteger(_name, _wrapValueInProperty, _isOptional, _attributes);
        }

        public ICimJsonElementDescriptorBuilder WithName(string name)
        {
            _name = name;
            return this;
        }

        public ICimJsonElementDescriptor BuildDescriptor()
        {
            return this;
        }
    }
}
