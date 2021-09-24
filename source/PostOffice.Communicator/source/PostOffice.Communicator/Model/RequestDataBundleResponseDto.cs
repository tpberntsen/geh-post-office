using System;
using System.Collections.Generic;

namespace GreenEnergyHub.PostOffice.Communicator.Model
{
    public sealed class RequestDataBundleResponseDto
    {
        public RequestDataBundleResponseDto(Uri contentUri, IEnumerable<string> dataAvailableNotificationIds)
        {
            DataAvailableNotificationIds = dataAvailableNotificationIds;
            ContentUri = contentUri;
        }

        public RequestDataBundleResponseDto(DataBundleResponseError responseError, IEnumerable<string> dataAvailableNotificationIds)
        {
            DataAvailableNotificationIds = dataAvailableNotificationIds;
            ResponseError = responseError;
        }

        public IEnumerable<string> DataAvailableNotificationIds { get; }

        public Uri? ContentUri { get; }

        public DataBundleResponseError? ResponseError { get; }
    }
}
