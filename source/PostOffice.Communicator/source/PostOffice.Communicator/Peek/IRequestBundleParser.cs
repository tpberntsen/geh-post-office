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

using System.Diagnostics.CodeAnalysis;
using GreenEnergyHub.PostOffice.Communicator.Model;

namespace GreenEnergyHub.PostOffice.Communicator.Peek
{
    /// <summary>
    /// Parses the bundle content request sent to a sub-domain.
    /// </summary>
    public interface IRequestBundleParser
    {
        /// <summary>
        /// Converts the specified request into a protobuf contract.
        /// </summary>
        /// <param name="request">The request to convert.</param>
        /// <param name="bytes">The bytes containing the protobuf contract.</param>
        /// <returns><see cref="bool"/></returns>
        bool TryParse(DataBundleRequestDto request, [NotNullWhen(true)] out byte[]? bytes);

        /// <summary>
        /// Parses the protobuf contract request.
        /// </summary>
        /// <param name="dataBundleRequestContract">The bytes containing the protobuf contract.</param>
        /// <param name="request">The converted request.</param>
        /// <returns><see cref="bool"/></returns>
        bool TryParse(byte[] dataBundleRequestContract, [NotNullWhen(true)] out DataBundleRequestDto? request);
    }
}