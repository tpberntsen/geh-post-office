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
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;
using Energinet.DataHub.PostOffice.Common;
using Energinet.DataHub.PostOffice.Infrastructure.Correlation;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.Channel;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.Azure.Functions.Worker;
using Moq;
using Xunit;
using Xunit.Categories;

namespace Energinet.DataHub.PostOffice.Tests.Common
{
    [UnitTest]
    public class EntryPointTelemetryScopeMiddlewareTests
    {
        [Fact]
        public async Task Invoke_ContextIsNull_Throws()
        {
            // arrange
            var correlationContext = new Mock<ICorrelationContext>();
            TelemetryClient mockTelemetryClient = InitializeMockTelemetryChannel();
            var target = new EntryPointTelemetryScopeMiddleware(mockTelemetryClient, correlationContext.Object);

            // act, assert
            await Assert.ThrowsAsync<ArgumentNullException>(() => target.Invoke(null!, _ => Task.CompletedTask)).ConfigureAwait(false);
        }

        [Fact]
        public async Task Invoke_NextIsNull_Throws()
        {
            // arrange
            var correlationContext = new Mock<ICorrelationContext>();
            TelemetryClient mockTelemetryClient = InitializeMockTelemetryChannel();
            var target = new EntryPointTelemetryScopeMiddleware(mockTelemetryClient, correlationContext.Object);

            var bindingData = new Dictionary<string, object?>()
            {
                { "bundleId", Guid.NewGuid().ToString() },
                { "marketOperator", Guid.NewGuid().ToString() }
            };

            var functionContextMock = BuildFunctionContext(bindingData, "Test");

            // act, assert
            await Assert.ThrowsAsync<NullReferenceException>(() => target.Invoke(functionContextMock, null!)).ConfigureAwait(false);
        }

        [Fact]
        public async Task Invoke_AllGood_NextCalled()
        {
            // arrange
            var nextCalled = false;
            var correlationContext = new Mock<ICorrelationContext>();
            TelemetryClient mockTelemetryClient = InitializeMockTelemetryChannel();
            var target = new EntryPointTelemetryScopeMiddleware(mockTelemetryClient, correlationContext.Object);

            var bindingData = new Dictionary<string, object?>()
            {
                { "bundleId", Guid.NewGuid().ToString() },
                { "marketOperator", Guid.NewGuid().ToString() }
            };

            var functionContextMock = BuildFunctionContext(bindingData, "Test");

            // act, assert
            await target.Invoke(functionContextMock, _ => Task.FromResult(nextCalled = true)).ConfigureAwait(false);

            // assert
            Assert.True(nextCalled);
        }

        [Fact]
        public async Task Invoke_AllGood_WithoutBindingData_NextCalled()
        {
            // arrange
            var nextCalled = false;
            var correlationContext = new Mock<ICorrelationContext>();
            TelemetryClient mockTelemetryClient = InitializeMockTelemetryChannel();
            var target = new EntryPointTelemetryScopeMiddleware(mockTelemetryClient, correlationContext.Object);

            var functionContextMock = BuildFunctionContext(new Dictionary<string, object?>(), "Test");

            // act, assert
            await target.Invoke(functionContextMock, _ => Task.FromResult(nextCalled = true)).ConfigureAwait(false);

            // assert
            Assert.True(nextCalled);
        }

        private static FunctionContext BuildFunctionContext(Dictionary<string, object?> bindingData, string functionName)
        {
            var mockedFunctionContext = new MockedFunctionContext();
            var functionContextMock = mockedFunctionContext.FunctionContext;
            mockedFunctionContext.FunctionDefinitionMock
                .SetupGet(f => f.Name)
                .Returns(functionName);
            mockedFunctionContext.BindingContext
                .SetupGet(b => b.BindingData)
                .Returns(bindingData);
            return functionContextMock;
        }

        private static TelemetryClient InitializeMockTelemetryChannel()
        {
            var mockTelemetryChannel = new MockTelemetryChannel()
            {
                DeveloperMode = false,
                EndpointAddress = string.Empty
            };

            using var mockTelemetryConfig = new TelemetryConfiguration()
            {
                TelemetryChannel = mockTelemetryChannel,
                InstrumentationKey = Guid.NewGuid().ToString(),
            };

            var mockTelemetryClient = new TelemetryClient(mockTelemetryConfig);
            return mockTelemetryClient;
        }

        private sealed class MockTelemetryChannel : ITelemetryChannel
        {
            private readonly ConcurrentBag<ITelemetry> _sentTelemtries = new();
            public bool IsFlushed { get; private set; }
            public bool? DeveloperMode { get; set; }
            public string? EndpointAddress { get; set; }

            public void Send(ITelemetry item)
            {
                _sentTelemtries.Add(item);
            }

            public void Flush()
            {
                IsFlushed = true;
            }

            public void Dispose()
            {
            }
        }
    }
}
