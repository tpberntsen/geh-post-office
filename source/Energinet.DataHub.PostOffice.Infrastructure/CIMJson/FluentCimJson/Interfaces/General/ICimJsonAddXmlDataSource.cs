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
using System.Xml;
using Energinet.DataHub.PostOffice.Infrastructure.CIMJson.FluentCimJson.Builders.General;

namespace Energinet.DataHub.PostOffice.Infrastructure.CIMJson.FluentCimJson.Interfaces.General
{
    /// <summary>
    /// Interface used to make sure an XML datasource from an XML Reader is selected
    /// </summary>
    internal interface ICimJsonAddXmlDataSource
    {
        /// <summary>
        /// Set the XML reader to use as the source for the conversion
        /// </summary>
        /// <param name="configure"></param>
        /// <param name="reader"></param>
        /// <returns>The CimJsonBuilder that can be used to construct the template for conversion</returns>
        CimJsonBuilder WithXmlReader(
            Action<ICimJsonAddElementDescriptors> configure,
            XmlReader reader);
    }
}
