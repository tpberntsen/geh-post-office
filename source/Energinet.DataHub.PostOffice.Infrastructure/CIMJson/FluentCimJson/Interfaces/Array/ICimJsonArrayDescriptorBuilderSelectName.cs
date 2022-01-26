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

namespace Energinet.DataHub.PostOffice.Infrastructure.CIMJson.FluentCimJson.Interfaces.Array
{
    /// <summary>
    /// Ensures that elements added to an array all have a name
    /// </summary>
    internal interface ICimJsonArrayDescriptorBuilderSelectName
    {
        /// <summary>
        /// Method to name an element
        /// </summary>
        /// <param name="name"></param>
        /// <returns>The element builder currently being configured</returns>
        ICimJsonArrayElementDescriptorBuilder WithName(string name);
    }
}
