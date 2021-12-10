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

namespace Energinet.DataHub.PostOffice.Infrastructure.CIMJson.FluentCimJson.Interfaces.Attributes
{
    /// <summary>
    /// Interface for the attribute builder
    /// </summary>
    internal interface ICimJsonAttributeDescriptorBuilder
    {
        /// <summary>
        /// Adds a string element for an attribute descriptors
        /// </summary>
        /// <returns>The attribute builder currently being configured</returns>
        ICimJsonAttributeDescriptorBuilder AddString(Action<ICimJsonAttributeElementSelectNameBuilder> configure);

        /// <summary>
        ///  Adds an integer element for an attribute descriptors
        /// </summary>
        /// <param name="configure"></param>
        /// <returns>The attribute builder currently being configured</returns>
        ICimJsonAttributeDescriptorBuilder AddInteger(Action<ICimJsonAttributeElementSelectNameBuilder> configure);
    }
}
