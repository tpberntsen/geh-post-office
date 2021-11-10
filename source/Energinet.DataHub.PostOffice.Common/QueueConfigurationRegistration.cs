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

using Energinet.DataHub.MessageHub.Core;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SimpleInjector;

namespace Energinet.DataHub.PostOffice.Common
{
    internal static class QueueConfigurationRegistration
    {
        public static void AddQueueConfiguration(this Container container)
        {
            container.RegisterSingleton(() =>
            {
                var configuration = container.GetService<IConfiguration>();
                var timeSeriesQueue = configuration.GetValue("TIMESERIES_QUEUE_NAME", "sbq-timeseries");
                var timeSeriesReplyQueue = configuration.GetValue("TIMESERIES_REPLY_QUEUE_NAME", "sbq-timeseries-reply");
                var chargesQueue = configuration.GetValue("CHARGES_QUEUE_NAME", "sbq-charges");
                var chargesReplyQueue = configuration.GetValue("CHARGES_REPLY_QUEUE_NAME", "sbq-charges-reply");
                var marketRolesQueue = configuration.GetValue("MARKETROLES_QUEUE_NAME", "sbq-marketroles");
                var marketRolesReplyQueue = configuration.GetValue("MARKETROLES_REPLY_QUEUE_NAME", "sbq-marketroles-reply");
                var meteringPointsQueue = configuration.GetValue("METERINGPOINTS_QUEUE_NAME", "sbq-meteringpoints");
                var meteringPointsReplyQueue = configuration.GetValue("METERINGPOINTS_REPLY_QUEUE_NAME", "sbq-meteringpoints-reply");
                var aggregationsQueue = configuration.GetValue("AGGREGATIONS_QUEUE_NAME", "sbq-aggregations");
                var aggregationsReplyQueue = configuration.GetValue("AGGREGATIONS_REPLY_QUEUE_NAME", "sbq-aggregations-reply");

                return new PeekRequestConfig(
                    timeSeriesQueue,
                    timeSeriesReplyQueue,
                    chargesQueue,
                    chargesReplyQueue,
                    marketRolesQueue,
                    marketRolesReplyQueue,
                    meteringPointsQueue,
                    meteringPointsReplyQueue,
                    aggregationsQueue,
                    aggregationsReplyQueue);
            });

            container.RegisterSingleton(() =>
            {
                var configuration = container.GetService<IConfiguration>();
                var timeSeriesDequeueQueue = configuration.GetValue("TIMESERIES_DEQUEUE_QUEUE_NAME", "sbq-timeseries-dequeue");
                var chargesDequeueQueue = configuration.GetValue("CHARGES_DEQUEUE_QUEUE_NAME", "sbq-charges-dequeue");
                var marketRolesDequeueQueue = configuration.GetValue("MARKETROLES_DEQUEUE_QUEUE_NAME", "sbq-marketroles-dequeue");
                var meteringPointsDequeueQueue = configuration.GetValue("METERINGPOINTS_DEQUEUE_QUEUE_NAME", "sbq-meteringpoints-dequeue");
                var aggregationsDequeueQueue = configuration.GetValue("AGGREGATIONS_DEQUEUE_QUEUE_NAME", "sbq-aggregations-dequeue");

                return new DequeueConfig(
                    timeSeriesDequeueQueue,
                    chargesDequeueQueue,
                    marketRolesDequeueQueue,
                    meteringPointsDequeueQueue,
                    aggregationsDequeueQueue);
            });
        }
    }
}
