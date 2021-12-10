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
using Energinet.DataHub.PostOffice.Infrastructure.CIMJson.FluentCimJson.Interfaces.Element;
using Energinet.DataHub.PostOffice.Infrastructure.CIMJson.FluentCimJson.Interfaces.Nested;

namespace Energinet.DataHub.PostOffice.Infrastructure.CIMJson.FluentCimJson.Interfaces.Array
{
    /// <summary>
    /// Interface to handle adding elements to a JSON array
    /// </summary>
    internal interface ICimJsonAddElementTypeFromWithinArray
    {
        /// <summary>
        /// Adds an string element to the Array
        /// </summary>
        /// <param name="configure"></param>
        /// <returns>The array builder currently being configured</returns>
        ICimJsonAddElementTypeFromWithinArray AddString(Action<ICimJsonElementDescriptorSelectNameBuilder> configure);

        /// <summary>
        /// Adds an integer element to the Array
        /// </summary>
        /// <param name="configure"></param>
        /// <returns>The array builder currently being configured</returns>
        ICimJsonAddElementTypeFromWithinArray AddInteger(Action<ICimJsonElementDescriptorSelectNameBuilder> configure);

        /// <summary>
        /// Adds a nested array to the current array
        /// </summary>
        /// <param name="configure"></param>
        /// <returns>The array builder currently being configured</returns>
        ICimJsonAddElementTypeFromWithinArray AddArray(Action<ICimJsonArrayDescriptorBuilderSelectName> configure);

        /// <summary>
        /// Ads an JSON object to the current array
        /// </summary>
        /// <param name="configure"></param>
        /// <returns>The array builder currently being configured</returns>
        ICimJsonAddElementTypeFromWithinArray AddNested(Action<ICimJsonNestedDescriptorBuilderSelectName> configure);
    }
}
