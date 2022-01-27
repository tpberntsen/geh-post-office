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
using Microsoft.Azure.Functions.Worker;

namespace Microsoft.Azure.Functions.Isolated.TestDoubles
{
    public class MockFunctionInvocation : FunctionInvocation
    {
        public MockFunctionInvocation(string id = "", string functionId = "")
        {
            if (!string.IsNullOrWhiteSpace(id)) Id = id;

            if (!string.IsNullOrWhiteSpace(functionId)) FunctionId = functionId;
        }

        public override string Id { get; } = Guid.NewGuid().ToString();

        public override string FunctionId { get; } = Guid.NewGuid().ToString();

        public override TraceContext TraceContext { get; } = new MockedTraceContext(Guid.NewGuid().ToString(), Guid.NewGuid().ToString());
    }
}
