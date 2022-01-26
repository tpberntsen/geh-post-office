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
using Microsoft.Azure.Functions.Worker.Http;

namespace Microsoft.Azure.Functions.Isolated.TestDoubles.Extensions
{
    public static class HttpResponseDataExtensions
    {
        public static string ReadHttpResponseData(this HttpResponseData response)
        {
            var stream = response.Body;
            if (stream is MemoryStream)
            {
                if (stream.Position != 0)
                {
                    stream.Position = 0;
                }

                using (var reader = new StreamReader(stream))
                {
                    return reader.ReadToEnd();
                }
            }

            return string.Empty;
        }
    }
}
