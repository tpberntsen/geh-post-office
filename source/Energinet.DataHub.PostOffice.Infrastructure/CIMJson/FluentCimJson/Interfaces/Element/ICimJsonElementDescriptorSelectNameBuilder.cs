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

namespace Energinet.DataHub.PostOffice.Infrastructure.CIMJson.FluentCimJson.Interfaces.Element
{
    /// <summary>
    /// Interface to ensure an element builder has a method to set its name
    /// </summary>
    internal interface ICimJsonElementDescriptorSelectNameBuilder
    {
        /// <summary>
        /// Sets the name of the attributeElement
        /// </summary>
        /// <param name="name"></param>
        /// <returns>THe attribute builder currently being configured</returns>
        ICimJsonElementDescriptorBuilder WithName(string name);
    }
}
