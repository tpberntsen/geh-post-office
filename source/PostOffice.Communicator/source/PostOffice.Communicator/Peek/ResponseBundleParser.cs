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
using System.Linq;
using Google.Protobuf;
using GreenEnergyHub.PostOffice.Communicator.Contracts;
using GreenEnergyHub.PostOffice.Communicator.Exceptions;
using GreenEnergyHub.PostOffice.Communicator.Model;

namespace GreenEnergyHub.PostOffice.Communicator.Peek
{
    public class ResponseBundleParser : IResponseBundleParser
    {
        public byte[] Parse(RequestDataBundleResponseDto requestDataBundleResponseDto)
        {
            if (requestDataBundleResponseDto == null)
                throw new ArgumentNullException(nameof(requestDataBundleResponseDto));
            var contract = new DataBundleResponseContract();

            if (!requestDataBundleResponseDto.IsErrorResponse)
            {
                contract.Success = new DataBundleResponseContract.Types.FileResource { ContentUri = requestDataBundleResponseDto.ContentUri?.AbsoluteUri };
                return contract.ToByteArray();
            }

            var contractErrorReason = MapToFailureReason(requestDataBundleResponseDto.ResponseError!.Reason);
            contract.Failure = new DataBundleResponseContract.Types.RequestFailure
            {
                Reason = contractErrorReason,
                FailureDescription = requestDataBundleResponseDto.ResponseError.FailureDescription
            };

            return contract.ToByteArray();
        }

        public RequestDataBundleResponseDto? Parse(byte[] dataBundleReplyContract)
        {
            try
            {
                var bundleResponse = DataBundleResponseContract.Parser.ParseFrom(dataBundleReplyContract);
                return bundleResponse!.ReplyCase != DataBundleResponseContract.ReplyOneofCase.Success
                    ? null
                    : new RequestDataBundleResponseDto(
                        new Uri(bundleResponse.Success.ContentUri),
                        bundleResponse.Success.DataAvailableNotificationIds.Select(Guid.Parse).ToList());
            }
            catch (InvalidProtocolBufferException e)
            {
                throw new PostOfficeCommunicatorException("Error parsing bytes for DataBundleRequestDto", e);
            }
        }

        private static DataBundleResponseContract.Types.RequestFailure.Types.Reason MapToFailureReason(DataBundleResponseErrorReason errorReason)
        {
            return errorReason switch
            {
                DataBundleResponseErrorReason.DatasetNotFound => DataBundleResponseContract.Types.RequestFailure.Types.Reason.DatasetNotFound,
                DataBundleResponseErrorReason.DatasetNotAvailable => DataBundleResponseContract.Types.RequestFailure.Types.Reason.DatasetNotAvailable,
                DataBundleResponseErrorReason.InternalError => DataBundleResponseContract.Types.RequestFailure.Types.Reason.InternalError,
                _ => DataBundleResponseContract.Types.RequestFailure.Types.Reason.InternalError
            };
        }
    }
}
