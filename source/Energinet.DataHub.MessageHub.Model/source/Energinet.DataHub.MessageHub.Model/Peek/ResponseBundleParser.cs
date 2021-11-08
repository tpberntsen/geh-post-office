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
using Energinet.DataHub.MessageHub.Model.Exceptions;
using Energinet.DataHub.MessageHub.Model.Model;
using Energinet.DataHub.MessageHub.Model.Protobuf;
using Google.Protobuf;

namespace Energinet.DataHub.MessageHub.Model.Peek
{
    public sealed class ResponseBundleParser : IResponseBundleParser
    {
        public byte[] Parse(DataBundleResponseDto dataBundleResponseDto)
        {
            if (dataBundleResponseDto == null)
                throw new ArgumentNullException(nameof(dataBundleResponseDto));

            var contract = new DataBundleResponseContract { RequestId = dataBundleResponseDto.RequestId.ToString() };

            if (!dataBundleResponseDto.IsErrorResponse)
            {
                contract.Success = new DataBundleResponseContract.Types.FileResource { ContentUri = dataBundleResponseDto.ContentUri.AbsoluteUri };
                return contract.ToByteArray();
            }

            var contractErrorReason = MapToFailureReason(dataBundleResponseDto.ResponseError.Reason);
            contract.Failure = new DataBundleResponseContract.Types.RequestFailure
            {
                Reason = contractErrorReason,
                FailureDescription = dataBundleResponseDto.ResponseError.FailureDescription
            };

            return contract.ToByteArray();
        }

        public DataBundleResponseDto Parse(byte[] dataBundleReplyContract)
        {
            try
            {
                var bundleResponse = DataBundleResponseContract.Parser.ParseFrom(dataBundleReplyContract);
                var requestId = Guid.Parse(bundleResponse.RequestId);

                if (bundleResponse.ReplyCase == DataBundleResponseContract.ReplyOneofCase.Success)
                {
                    var successReply = bundleResponse.Success;
                    return new DataBundleResponseDto(
                        requestId,
                        new Uri(successReply.ContentUri),
                        successReply.DataAvailableNotificationIds.Select(Guid.Parse).ToList());
                }

                var failureReply = bundleResponse.Failure;
                var errorResponse = new DataBundleResponseErrorDto
                {
                    FailureDescription = failureReply.FailureDescription,
                    Reason = MapToFailureReason(failureReply.Reason)
                };

                return new DataBundleResponseDto(
                    requestId,
                    errorResponse,
                    failureReply.DataAvailableNotificationIds.Select(Guid.Parse).ToList());
            }
            catch (Exception ex) when (ex is InvalidProtocolBufferException or FormatException)
            {
                throw new MessageHubException("Error parsing bytes for DataBundleRequestDto", ex);
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

        private static DataBundleResponseErrorReason MapToFailureReason(DataBundleResponseContract.Types.RequestFailure.Types.Reason errorReason)
        {
            return errorReason switch
            {
                DataBundleResponseContract.Types.RequestFailure.Types.Reason.DatasetNotFound => DataBundleResponseErrorReason.DatasetNotFound,
                DataBundleResponseContract.Types.RequestFailure.Types.Reason.DatasetNotAvailable => DataBundleResponseErrorReason.DatasetNotAvailable,
                DataBundleResponseContract.Types.RequestFailure.Types.Reason.InternalError => DataBundleResponseErrorReason.InternalError,
                _ => DataBundleResponseErrorReason.InternalError
            };
        }
    }
}
