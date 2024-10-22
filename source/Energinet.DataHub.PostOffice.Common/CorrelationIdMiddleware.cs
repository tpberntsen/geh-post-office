﻿// Copyright 2020 Energinet DataHub A/S
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

using System.Threading.Tasks;
using Energinet.DataHub.PostOffice.Infrastructure.Correlation;
using Energinet.DataHub.PostOffice.Utilities;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Middleware;
using TraceContext = Energinet.DataHub.PostOffice.Infrastructure.Correlation.TraceContext;

namespace Energinet.DataHub.PostOffice.Common
{
    public class CorrelationIdMiddleware : IFunctionsWorkerMiddleware
    {
        private readonly ICorrelationContext _correlationContext;

        public CorrelationIdMiddleware(
            ICorrelationContext correlationContext)
        {
            _correlationContext = correlationContext;
        }

        public async Task Invoke(FunctionContext context, FunctionExecutionDelegate next)
        {
            Guard.ThrowIfNull(context, nameof(context));
            Guard.ThrowIfNull(next, nameof(next));

            var traceContext = TraceContext.Parse(context.TraceContext.TraceParent);

            _correlationContext.SetId(traceContext.TraceId);
            _correlationContext.SetParentId(traceContext.ParentId);

            await next(context).ConfigureAwait(false);
        }
    }
}
