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
using Energinet.DataHub.PostOffice.Infrastructure.CIMJson.FluentCimJson.Interfaces.Array;
using Energinet.DataHub.PostOffice.Infrastructure.CIMJson.FluentCimJson.Interfaces.Element;
using Energinet.DataHub.PostOffice.Infrastructure.CIMJson.FluentCimJson.Interfaces.Nested;

namespace Energinet.DataHub.PostOffice.Infrastructure.CIMJson.FluentCimJson.Interfaces.General
{
    /// <summary>
    /// Interface used to describe the possible elements that can be added from an XML and converted to JSON
    /// </summary>
    internal interface ICimJsonAddElementDescriptors
    {
        /// <summary>
        /// Adds an string element to the JSON file
        /// </summary>
        /// <param name="configure"></param>
        /// <returns>The builder that is currently being configured</returns>
        ICimJsonAddElementDescriptors AddString(Action<ICimJsonElementDescriptorSelectNameBuilder> configure);

        /// <summary>
        /// Adds an Integer element to the JSON file
        /// </summary>
        /// <param name="configure"></param>
        /// <returns>The builder that is currently being configured</returns>
        ICimJsonAddElementDescriptors AddInteger(Action<ICimJsonElementDescriptorSelectNameBuilder> configure);

        /// <summary>
        /// Adds an array to the JSON file
        /// </summary>
        /// <param name="configure"></param>
        /// <returns>The builder that is currently being configured</returns>
        ICimJsonAddElementDescriptors AddArray(Action<ICimJsonArrayDescriptorBuilderSelectName> configure);

        /// <summary>
        /// Adds an object to the JSON file, usually used to handle nested XML elements
        /// </summary>
        /// <param name="configure"></param>
        /// <returns>The builder that is currently being configured</returns>
        ICimJsonAddElementDescriptors AddNested(Action<ICimJsonNestedDescriptorBuilderSelectName> configure);
    }
}
