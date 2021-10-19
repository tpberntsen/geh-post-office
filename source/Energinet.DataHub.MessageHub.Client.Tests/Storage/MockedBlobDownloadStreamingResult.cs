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
using System.IO;
using System.Reflection;
using Azure.Storage.Blobs.Models;

namespace Energinet.DataHub.MessageHub.Client.Tests.Storage
{
    public static class MockedBlobDownloadStreamingResult
    {
        public static BlobDownloadStreamingResult Create(Stream content)
        {
            var ctor = typeof(BlobDownloadStreamingResult).GetConstructor(
                BindingFlags.Instance | BindingFlags.NonPublic,
                null,
                Type.EmptyTypes,
                null);

            var obj = ctor!.Invoke(Array.Empty<object>());
            var propInfo = typeof(BlobDownloadStreamingResult).GetProperty(
                "Content",
                BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            if (propInfo is not null) propInfo.SetValue(obj, content);
            return (BlobDownloadStreamingResult)obj;
        }
    }
}
