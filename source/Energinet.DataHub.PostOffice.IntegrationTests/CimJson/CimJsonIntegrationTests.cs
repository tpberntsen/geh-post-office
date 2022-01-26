// Copyright 2020 Energinet DataHub A/S
//
// Licensed under the Apache License, Version 2.0 (the "License2");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//     http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using System.IO;
using System.Text;
using System.Threading.Tasks;
using Energinet.DataHub.PostOffice.Infrastructure.CIMJson.Templates;
using Xunit;
using Xunit.Categories;

namespace Energinet.DataHub.PostOffice.IntegrationTests.CimJson
{
    [Collection("IntegrationTest1")]
    [IntegrationTest]
    public sealed class CimJsonIntegrationTests
    {
        [Fact]
        public async Task ConvertXmlToJson_TemplateNotifyValidatedMeasureDataWithFluent_ReturnsNotNull()
        {
            // Arrange
            using var testTemplate2 = new NotifyValidatedMeasureDataWithFluentTemplate();
            await using var testFile = new FileStream(@"CimJson/TestData/RSM-012 - Notify validated measure data.xml", FileMode.Open);

            // Act
            var result = await testTemplate2.ParseXmlAsync(testFile).ConfigureAwait(false);

            // Assert
            Assert.NotNull(result);
        }
    }
}
