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
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Google.Protobuf;
using GreenEnergyHub.PostOffice.Communicator.Contracts;
using GreenEnergyHub.PostOffice.Communicator.Model;

namespace GreenEnergyHub.PostOffice.Communicator.Peek
{
    public class ResponseBundleParser : IResponseBundleParser
    {
        public bool TryParse(RequestDataBundleResponseDto requestDataBundleResponseDto, [NotNullWhen(true)] out byte[]? bytes)
        {
            if (requestDataBundleResponseDto == null) throw new ArgumentNullException(nameof(requestDataBundleResponseDto));

            var contract = new RequestBundleResponse();

            if (!requestDataBundleResponseDto.IsErrorResponse.IsError)
            {
                contract.Success = new RequestBundleResponse.Types.FileResource { Uri = requestDataBundleResponseDto.ContentUri?.AbsoluteUri };
                bytes = contract.ToByteArray();
            }
            else
            {
                var contractErrorReason = MapToFailureReason(requestDataBundleResponseDto.ResponseError!.Reason);
                contract.Failure = new RequestBundleResponse.Types.RequestFailure { Reason = contractErrorReason, FailureDescription = requestDataBundleResponseDto.ResponseError.FailureDescription };
                bytes = contract.ToByteArray();
            }

            return bytes != null;
        }

        public bool TryParse(byte[] dataBundleReplyContract, [NotNullWhen(true)] out RequestDataBundleResponseDto? response)
        {
            try
            {
                var bundleResponse = RequestBundleResponse.Parser.ParseFrom(dataBundleReplyContract);

                response = bundleResponse.ReplyCase != RequestBundleResponse.ReplyOneofCase.Success
                    ? null
                    : new RequestDataBundleResponseDto(new Uri(bundleResponse.Success.Uri), bundleResponse.Success.UUID.AsEnumerable());
            }
#pragma warning disable CA1031
            catch (Exception)
#pragma warning restore CA1031
            {
                response = null;
            }

            return response != null;
        }

        private static RequestBundleResponse.Types.RequestFailure.Types.Reason MapToFailureReason(DataBundleResponseErrorReason errorReason)
        {
            return errorReason switch
            {
                DataBundleResponseErrorReason.DatasetNotFound => RequestBundleResponse.Types.RequestFailure.Types.Reason.DatasetNotFound,
                DataBundleResponseErrorReason.DatasetNotAvailable => RequestBundleResponse.Types.RequestFailure.Types.Reason.DatasetNotAvailable,
                DataBundleResponseErrorReason.InternalError => RequestBundleResponse.Types.RequestFailure.Types.Reason.InternalError,
                _ => RequestBundleResponse.Types.RequestFailure.Types.Reason.InternalError
            };
        }
    }
}
