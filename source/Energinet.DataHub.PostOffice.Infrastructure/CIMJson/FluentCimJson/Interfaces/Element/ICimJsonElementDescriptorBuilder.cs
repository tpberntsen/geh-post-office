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
using Energinet.DataHub.PostOffice.Infrastructure.CIMJson.FluentCimJson.Interfaces.Attributes;
using Energinet.DataHub.PostOffice.Infrastructure.CIMJson.FluentCimJson.Interfaces.Descriptor;

namespace Energinet.DataHub.PostOffice.Infrastructure.CIMJson.FluentCimJson.Interfaces.Element
{
    /// <summary>
    /// Describes an element descriptor builder, this is used to build element descriptors for XML elements
    /// </summary>
    internal interface ICimJsonElementDescriptorBuilder
    {
        /// <summary>
        /// Sets whether the element should be wrapped in an object and it's actual value being in an JSON property named "value"
        /// </summary>
        /// <returns>The builder currently being configured</returns>
        ICimJsonElementDescriptorBuilder WithValueWrappedInProperty();

        /// <summary>
        /// Adds an attribute builder to this element, if it contains attributes in the XML
        /// </summary>
        /// <param name="configure"></param>
        /// <returns>An Element descriptor describing the attributes fot this element</returns>
        ICimJsonElementDescriptorBuilder WithAttributes(Action<ICimJsonAttributeDescriptorBuilder> configure);

        /// <summary>
        /// Marks the array as optional
        /// </summary>
        ICimJsonElementDescriptorBuilder IsOptional();
    }
}
