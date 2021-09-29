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
using GetMessage.Storage;
using GreenEnergyHub.PostOffice.Communicator.Model;
using GreenEnergyHub.PostOffice.Communicator.Peek;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace GetMessage.Functions
{
    public class ReplyToRequestFromPostOffice
    {
        private readonly StorageController _blobStorageController;
        private readonly IRequestBundleParser _requestBundleParser;
        private readonly IDataBundleResponseSender _responseSender;

        public ReplyToRequestFromPostOffice(
            StorageController blobStorageController,
            IRequestBundleParser requestBundleParser,
            IDataBundleResponseSender responseSender)
        {
            _blobStorageController = blobStorageController;
            _requestBundleParser = requestBundleParser;
            _responseSender = responseSender;
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
            logger.LogInformation($"C# ServiceBus queue trigger function processed message: {message}");

            try
            {
                var bundleRequestDto = _requestBundleParser.Parse(message);

                var session = context.BindingContext.BindingData["MessageSession"] as string;

                var sessionData = JsonConvert.DeserializeObject<Dictionary<string, object>>(session ?? string.Empty);

                var sessionId = sessionData?["SessionId"] as string;

                var requestDataBundleResponseDto = await CreateResponseAsync(bundleRequestDto).ConfigureAwait(false);

                await _responseSender.SendAsync(requestDataBundleResponseDto, sessionId ?? string.Empty);
            }
            catch (Exception e)
            {
                throw new Exception("Could not process message.", e);
            }
        }

        private async Task<RequestDataBundleResponseDto> CreateResponseAsync(DataBundleRequestDto requestDto)
        {
            if (requestDto.DataAvailableNotificationIds.Contains("0ae6c542-385f-4d89-bfba-d6c451915a1b"))
                return CreateFailedResponse(requestDto, DataBundleResponseErrorReason.DatasetNotFound);
            else if (requestDto.DataAvailableNotificationIds.Contains("3cfce64e-aa1d-4003-924d-69c8739e73a6"))
                return CreateFailedResponse(requestDto, DataBundleResponseErrorReason.DatasetNotAvailable);
            else if (requestDto.DataAvailableNotificationIds.Contains("befdcf5a-f58d-493b-9a17-e5231609c8f6"))
                return CreateFailedResponse(requestDto, DataBundleResponseErrorReason.InternalError);

            return await CreateSuccessResponseAsync(requestDto).ConfigureAwait(false);
        }

        private RequestDataBundleResponseDto CreateFailedResponse(
            DataBundleRequestDto requestDto,
            DataBundleResponseErrorReason failedReason)
        {
            var responseDto = new RequestDataBundleResponseDto(
                new DataBundleResponseError()
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
            var blobName = $"blob-resource-{Environment.GetEnvironmentVariable("QueueListenerName")}-{DateTime.UtcNow:yyyy-MM-dd-HH-mm-ss-ff}.txt";
            var data = $"<data><uuid>{requestDto.IdempotencyId}<uuid><blobname>{blobName}</blobname></data>";
            var blobUri = await _blobStorageController.CreateBlobResourceAsync(blobName, new BinaryData(data)).ConfigureAwait(false);
            return blobUri;
        }
    }
}
