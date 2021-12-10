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

using System.Linq;
using System.Text.Json;
using Energinet.DataHub.PostOffice.Infrastructure.CIMJson.FluentCimJson.Factories;

namespace Energinet.DataHub.PostOffice.Infrastructure.CIMJson.FluentCimJson.Elements
{
    internal class CimJsonInteger : CimJsonBaseElement
    {
        private int _value;
        private bool _hasValue;

        public override void WriteJson(Utf8JsonWriter writer)
        {
            if (Attributes.Any())
            {
                writer.WriteStartObject(Name);
                foreach (var attribute in Attributes.OrderBy(x => x.Name))
                {
                    attribute.WriteJson(writer);
                }

                if (_hasValue)
                    writer.WriteNumber("value", _value);

                writer.WriteEndObject();
            }
            else
            {
                if (!_hasValue) return;
                if (RequiresValueProperty)
                {
                    writer.WriteStartObject(Name);
                    writer.WriteNumber("value", _value);
                    writer.WriteEndObject();
                }
                else
                {
                    writer.WriteNumber(Name, _value);
                }
            }
        }

        public override void ReturnElementToPool()
        {
            CimJsonElementFactory.ReleaseIntegerElement(this);
        }

        protected override void ParseString(string value)
        {
            _hasValue = int.TryParse(value, out _value);
        }
    }
}
