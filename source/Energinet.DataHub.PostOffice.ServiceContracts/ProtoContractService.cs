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

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;

namespace Energinet.DataHub.PostOffice.ServiceContracts
{
    #nullable disable

    public class ProtoContractService
    {
        private readonly string _baseDirectory;
        private readonly Dictionary<string, IEnumerable<string>> _protosByVersion;

        public ProtoContractService(ExecutionContext context)
        {
            if (context == null) throw new ArgumentNullException(nameof(context));

            _baseDirectory = Path.GetFullPath(Path.Combine(context.FunctionAppDirectory, @"..\..\..\.."));
            _protosByVersion = GetProtos(_baseDirectory);
        }

        public async Task<string> GetAllAsync()
        {
            using (var stream = new MemoryStream())
            {
                await JsonSerializer.SerializeAsync(stream, _protosByVersion).ConfigureAwait(false);
                stream.Position = 0;
                using var reader = new StreamReader(stream);
                return await reader.ReadToEndAsync().ConfigureAwait(false);
            }
        }

        public byte[] Get(string version, string protoName)
        {
            var filePath = $"{_baseDirectory}\\Contracts\\v{version}\\{protoName}";
            var exist = File.Exists(filePath);
            if (!exist)
            {
                return null;
            }

            var bytes = File.ReadAllBytes(filePath);
            return bytes;
        }

        private static Dictionary<string, IEnumerable<string>> GetProtos(string baseDirectory) =>

            Directory.GetDirectories($"{baseDirectory}\\Contracts")
                .Select(path => new { version = new DirectoryInfo(path).Name, protos = Directory.GetFiles(path).Select(Path.GetFileName) })
                .ToDictionary(o => o.version, o => o.protos);
    }
    #nullable restore
}
