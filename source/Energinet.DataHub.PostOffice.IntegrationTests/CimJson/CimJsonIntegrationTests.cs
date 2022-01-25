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
            using var testTemplate2 = new TemplateNotifyValidatedMeasureDataWithFluent();
            await using var testFile = new FileStream(@"CimJson/TestData/Notify validated measure data 2 series.xml", FileMode.Open);
            // await File.WriteAllTextAsync(@$"{Path.GetDirectoryName(Assembly.GetEntryAssembly()!.Location)}..\..\..\..\..\data\Converted-template2.json", json2);

            // Act
            var result2 = await testTemplate2.ParseXmlAsync(testFile).ConfigureAwait(false);
            var json2 = Encoding.UTF8.GetString(result2.ToArray());

            // Assert
            Assert.NotNull(json2);
        }
    }
}
