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
using System.Threading.Tasks;
using Azure.Messaging.ServiceBus;
using Energinet.DataHub.PostOffice.Contracts;
using GetMessage.Storage;
using Google.Protobuf;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace GetMessage.Functions
{
    public class ReplyToRequestFromPostOffice
    {
        private readonly ServiceBusClient _serviceBusClient;
        private readonly StorageController _blobStorageController;

        public ReplyToRequestFromPostOffice(ServiceBusClient serviceBusClient, StorageController blobStorageController)
        {
            _serviceBusClient = serviceBusClient;
            _blobStorageController = blobStorageController;
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
                var queueNameForResponse = context.BindingContext.BindingData["ReplyTo"]?.ToString();

                await using var sender = _serviceBusClient.CreateSender(queueNameForResponse);

                var session = context.BindingContext.BindingData["MessageSession"] as string;

                var sessionData = JsonConvert.DeserializeObject<Dictionary<string, object>>(session ?? string.Empty);

                var requestData = RequestDataset.Parser.ParseFrom(message);

                var protoResponse = await CreateResponseAsync(requestData).ConfigureAwait(false);

                var reply = new ServiceBusMessage
                {
                    Body = new BinaryData(protoResponse.ToByteArray()), SessionId = sessionData?["SessionId"] as string
                };

                logger.LogInformation("Reply to session {sessionId}", reply.SessionId);

                await sender.SendMessageAsync(reply).ConfigureAwait(false);
            }
            catch (Exception e)
            {
                throw new Exception("Could not process message.", e);
            }
        }

        private async Task<DatasetReply> CreateResponseAsync(RequestDataset requestData)
        {
            if (requestData.UUID.Contains("0ae6c542-385f-4d89-bfba-d6c451915a1b"))
                return CreateFailedResponse(requestData, DatasetReply.Types.RequestFailure.Types.Reason.DatasetNotFound);
            else if (requestData.UUID.Contains("3cfce64e-aa1d-4003-924d-69c8739e73a6"))
                return CreateFailedResponse(requestData, DatasetReply.Types.RequestFailure.Types.Reason.DatasetNotAvailable);
            else if (requestData.UUID.Contains("befdcf5a-f58d-493b-9a17-e5231609c8f6"))
                return CreateFailedResponse(requestData, DatasetReply.Types.RequestFailure.Types.Reason.InternalError);

            return await CreateSuccessResponseAsync(requestData).ConfigureAwait(false);
        }

        private DatasetReply CreateFailedResponse(
            RequestDataset requestData,
            DatasetReply.Types.RequestFailure.Types.Reason failedReason)
        {
            var proto = new DatasetReply()
            {
                Failure = new DatasetReply.Types.RequestFailure()
                {
                    Reason = failedReason,
                    FailureDescription = $"Failed with reason: {failedReason}"
                }
            };
            proto.Failure.UUID.AddRange(requestData.UUID);
            return proto;
        }

        private async Task<DatasetReply> CreateSuccessResponseAsync(RequestDataset requestData)
        {
            var ressourceUrl = await SaveDataToBlobStorageAsync(requestData).ConfigureAwait(false);

            var proto = new DatasetReply()
            {
                Success = new DatasetReply.Types.FileResource()
                {
                    Uri = ressourceUrl.AbsoluteUri,
                }
            };
            proto.Success.UUID.AddRange(requestData.UUID);
            return proto;
        }

        private async Task<Uri> SaveDataToBlobStorageAsync(RequestDataset requestData)
        {
            var blobName = $"blob-ressource-{Environment.GetEnvironmentVariable("QueueListenerName")}-{DateTime.UtcNow:yyyy-MM-dd-HH-mm-ss-ff}.txt";
            var data = $"<data>{blobName}</data>";
            var blobUri = await _blobStorageController.CreateBlobRessourceAsync(blobName, new BinaryData(data)).ConfigureAwait(false);
            return blobUri;
        }
    }
}
