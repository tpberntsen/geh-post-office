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
using Energinet.DataHub.MessageHub.Model.Model;

namespace Energinet.DataHub.MessageHub.Model.Extensions
{
    public static class DataBundleRequestDtoExtensions
    {
        /// <summary>
        /// Creates <see cref="DataBundleResponseDto"/> from <see cref="DataBundleRequestDto"/>.
        /// </summary>
        /// <param name="request">The request to create a response from.</param>
        /// <param name="path">The path of the created bundle.</param>
        /// <returns>The response to the specified request.</returns>
        public static DataBundleResponseDto CreateResponse(this DataBundleRequestDto request, Uri path)
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request));

            return new DataBundleResponseDto(
                request.RequestId,
                request.IdempotencyId,
                path,
                request.DataAvailableNotificationIds);
        }

        /// <summary>
        /// Creates a failed <see cref="DataBundleResponseDto"/> from <see cref="DataBundleRequestDto"/>.
        /// </summary>
        /// <param name="request">The request to create a response from.</param>
        /// <param name="errorResponse">A description of the error.</param>
        /// <returns>The response to the specified request.</returns>
        public static DataBundleResponseDto CreateErrorResponse(this DataBundleRequestDto request, DataBundleResponseErrorDto errorResponse)
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request));

            return new DataBundleResponseDto(
                request.RequestId,
                request.IdempotencyId,
                errorResponse,
                request.DataAvailableNotificationIds);
        }
    }
}
