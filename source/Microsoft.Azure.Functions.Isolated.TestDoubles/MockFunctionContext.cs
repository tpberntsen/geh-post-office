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
using Microsoft.Azure.Functions.Worker;

namespace Microsoft.Azure.Functions.Isolated.TestDoubles
{
    public sealed class MockFunctionContext : FunctionContext, IDisposable
    {
        private readonly FunctionInvocation _invocation;

        public MockFunctionContext()
            : this(new MockFunctionDefinition(), new MockFunctionInvocation())
        {
        }

        public MockFunctionContext(FunctionDefinition functionDefinition, FunctionInvocation invocation)
        {
            FunctionDefinition = functionDefinition;
            _invocation = invocation;
        }

        public bool IsDisposed { get; private set; }

        public override IServiceProvider InstanceServices { get; set; }

        public override FunctionDefinition FunctionDefinition { get; }

        public override IDictionary<object, object> Items { get; set; } = new Dictionary<object, object>();

        public override IInvocationFeatures Features { get; } /*= new InvocationFeatures(Enumerable.Empty<IInvocationFeatureProvider>());*/

        public override string InvocationId => _invocation.Id;

        public override string FunctionId => _invocation.FunctionId;

        public override TraceContext TraceContext => _invocation.TraceContext;

        public override BindingContext BindingContext { get; }
        public override RetryContext RetryContext { get; }

        public void Dispose()
        {
            IsDisposed = true;
        }
    }
}
