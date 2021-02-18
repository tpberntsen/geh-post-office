﻿// Copyright 2020 Energinet DataHub A/S
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
using Newtonsoft.Json;

namespace Energinet.DataHub.PostOffice.Tests.Tooling
{
    internal class LocalSettings
    {
        public LocalSettings()
        {
            Values = new Dictionary<string, string>();
        }

        public bool IsEncrypted { get; set; }

        public Dictionary<string, string> Values { get; set; }

        public static void SetupEnvironment()
        {
            string basePath = Path.GetFullPath(@"..\..\..\");
            var settings = JsonConvert.DeserializeObject<LocalSettings>(
                File.ReadAllText(basePath + "\\local.settings.json"));

            foreach (var setting in settings.Values)
            {
                Environment.SetEnvironmentVariable(setting.Key, setting.Value);
            }
        }
    }
}