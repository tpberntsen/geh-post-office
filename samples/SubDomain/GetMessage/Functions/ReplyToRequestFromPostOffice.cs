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
using System.Linq;
using System.Threading.Tasks;
using GreenEnergyHub.PostOffice.Communicator.Exceptions;
using GreenEnergyHub.PostOffice.Communicator.Model;
using GreenEnergyHub.PostOffice.Communicator.Peek;
using GreenEnergyHub.PostOffice.Communicator.Storage;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace GetMessage.Functions
{
    public class ReplyToRequestFromPostOffice
    {
        private readonly IRequestBundleParser _requestBundleParser;
        private readonly IDataBundleResponseSender _responseSender;
        private readonly IStorageHandler _storageHandler;

        public ReplyToRequestFromPostOffice(
            IRequestBundleParser requestBundleParser,
            IDataBundleResponseSender responseSender,
            IStorageHandler storageHandler)
        {
            _requestBundleParser = requestBundleParser;
            _responseSender = responseSender;
            _storageHandler = storageHandler;
        }

        [Function("ReplyToRequestFromPostOffice")]
        public async Task RunAsync(
            [ServiceBusTrigger(
            "%QueueListenerName%",
            Connection = "ServiceBusConnectionString",
            IsSessionsEnabled = true)]
            byte[] message,
            FunctionContext context)
        {
            var logger = context.GetLogger("ReplyToRequestFromPostOffice");
            logger.LogInformation($"C# ServiceBus queue trigger function processesing message: {message}");

            try
            {
                var bundleRequestDto = _requestBundleParser.Parse(message);

                var session = context.BindingContext.BindingData["MessageSession"] as string;

                var sessionData = JsonConvert.DeserializeObject<Dictionary<string, object>>(session ?? string.Empty);

                var sessionId = sessionData?["SessionId"] as string;

                var requestDataBundleResponseDto = await CreateResponseAsync(bundleRequestDto).ConfigureAwait(false);

                await _responseSender.SendAsync(requestDataBundleResponseDto, sessionId ?? string.Empty).ConfigureAwait(false);
            }
            catch (PostOfficeCommunicatorStorageException e)
            {
                logger.LogError("Error Processing message", e);
                throw;
            }
        }

        private async Task<RequestDataBundleResponseDto> CreateResponseAsync(DataBundleRequestDto requestDto)
        {
            if (requestDto.DataAvailableNotificationIds.Contains(new Guid("0ae6c542-385f-4d89-bfba-d6c451915a1b")))
                return CreateFailedResponse(requestDto, DataBundleResponseErrorReason.DatasetNotFound);
            else if (requestDto.DataAvailableNotificationIds.Contains(new Guid("3cfce64e-aa1d-4003-924d-69c8739e73a6")))
                return CreateFailedResponse(requestDto, DataBundleResponseErrorReason.DatasetNotAvailable);
            else if (requestDto.DataAvailableNotificationIds.Contains(new Guid("befdcf5a-f58d-493b-9a17-e5231609c8f6")))
                return CreateFailedResponse(requestDto, DataBundleResponseErrorReason.InternalError);

            return await CreateSuccessResponseAsync(requestDto).ConfigureAwait(false);
        }

        private RequestDataBundleResponseDto CreateFailedResponse(
            DataBundleRequestDto requestDto,
            DataBundleResponseErrorReason failedReason)
        {
            var responseDto = new RequestDataBundleResponseDto(
                new DataBundleResponseErrorDto
                {
                    Reason = failedReason,
                    FailureDescription = failedReason.ToString()
                },
                requestDto.DataAvailableNotificationIds);

            return responseDto;
        }

        private async Task<RequestDataBundleResponseDto> CreateSuccessResponseAsync(DataBundleRequestDto requestDto)
        {
            var resourceUrl = await SaveDataToBlobStorageAsync(requestDto).ConfigureAwait(false);

            var responseDto = new RequestDataBundleResponseDto(new Uri(resourceUrl.AbsoluteUri), requestDto.DataAvailableNotificationIds);

            return responseDto;
        }

        private async Task<Uri> SaveDataToBlobStorageAsync(DataBundleRequestDto requestDto)
        {
            var testString =
                $"<data><uuid>{requestDto.IdempotencyId}<uuid><dataAvailableNotifications>{string.Join(",", requestDto.DataAvailableNotificationIds)}</dataAvailableNotifications></data>";
            var testData = new BinaryData(testString);
            return await _storageHandler.AddStreamToStorageAsync(testData.ToStream(), requestDto).ConfigureAwait(false);
        }
    }
}
