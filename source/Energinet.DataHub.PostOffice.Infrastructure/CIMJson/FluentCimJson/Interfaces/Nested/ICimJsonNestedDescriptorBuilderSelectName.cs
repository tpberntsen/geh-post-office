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

namespace Energinet.DataHub.PostOffice.Infrastructure.CIMJson.FluentCimJson.Interfaces.Nested
{
    /// <summary>
    /// Interface used to ensure that a name is selected for the elements in the builder
    /// </summary>
    internal interface ICimJsonNestedDescriptorBuilderSelectName
    {
        /// <summary>
        /// Sets the name for this element in the builder
        /// </summary>
        /// <param name="name"></param>
        /// <returns>The builder that is currently being configured</returns>
        ICimJsonNestedDescriptorBuilder WithName(string name);
    }
}
