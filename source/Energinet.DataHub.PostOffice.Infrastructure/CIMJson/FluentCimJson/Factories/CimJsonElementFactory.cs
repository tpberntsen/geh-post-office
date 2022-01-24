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

using System.Collections.Generic;
using Energinet.DataHub.PostOffice.Infrastructure.CIMJson.FluentCimJson.Elements;
using Energinet.DataHub.PostOffice.Infrastructure.CIMJson.FluentCimJson.Interfaces.Attributes;
using Microsoft.Extensions.ObjectPool;

namespace Energinet.DataHub.PostOffice.Infrastructure.CIMJson.FluentCimJson.Factories
{
    internal static class CimJsonElementFactory
    {
        private static readonly ObjectPool<CimJsonString> _stringElementsPool = new DefaultObjectPool<CimJsonString>(new DefaultPooledObjectPolicy<CimJsonString>());
        private static readonly ObjectPool<CimJsonInteger> _integerElementsPool = new DefaultObjectPool<CimJsonInteger>(new DefaultPooledObjectPolicy<CimJsonInteger>());

        public static CimJsonString CreateString(string name, bool requiresValueProperty, bool isOptional, List<ICimJsonAttributeDescriptor> attributeDescriptors)
        {
            var pooledElement = _stringElementsPool.Get();
            pooledElement.Name = name;
            pooledElement.IsOptional = isOptional;
            pooledElement.Attributes.Clear();
            pooledElement.AttributeDescriptors = attributeDescriptors;
            pooledElement.RequiresValueProperty = requiresValueProperty;
            return pooledElement;
        }

        public static void ReleaseStringElement(CimJsonString element)
        {
            _stringElementsPool.Return(element);
        }

        public static CimJsonInteger CreateInteger(string name, bool requiresValueProperty, bool isOptional, List<ICimJsonAttributeDescriptor> attributeDescriptors)
        {
            var pooledElement = _integerElementsPool.Get();
            pooledElement.Name = name;
            pooledElement.IsOptional = isOptional;
            pooledElement.Attributes.Clear();
            pooledElement.AttributeDescriptors = attributeDescriptors;
            pooledElement.RequiresValueProperty = requiresValueProperty;
            return pooledElement;
        }

        public static void ReleaseIntegerElement(CimJsonInteger element)
        {
            _integerElementsPool.Return(element);
        }
    }
}
