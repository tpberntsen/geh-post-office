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
using System.Diagnostics.CodeAnalysis;

namespace GreenEnergyHub.PostOffice.Communicator.Model
{
    /// <summary>
    /// Contains the result of the request for bundle contents.
    /// </summary>
    public sealed class RequestDataBundleResponseDto
    {
        public RequestDataBundleResponseDto(Uri contentUri, IEnumerable<string> dataAvailableNotificationIds)
        {
            DataAvailableNotificationIds = dataAvailableNotificationIds;
            ContentUri = contentUri;
            IsErrorResponse = new IsErrorResponse(false);
        }

        public RequestDataBundleResponseDto(DataBundleResponseError responseError, IEnumerable<string> dataAvailableNotificationIds)
        {
            DataAvailableNotificationIds = dataAvailableNotificationIds;
            ResponseError = responseError;
            IsErrorResponse = new IsErrorResponse(true);
        }

        [MemberNotNullWhen(false, nameof(ContentUri))]
        [MemberNotNullWhen(true, nameof(ResponseError))]
        public IsErrorResponse IsErrorResponse { get; }

        public IEnumerable<string> DataAvailableNotificationIds { get; }

        public Uri? ContentUri { get; }

        public DataBundleResponseError? ResponseError { get; }
    }
}
