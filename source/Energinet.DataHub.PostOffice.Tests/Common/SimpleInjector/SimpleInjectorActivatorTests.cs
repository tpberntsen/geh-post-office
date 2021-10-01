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
using System.Threading.Tasks;
using Energinet.DataHub.PostOffice.Common.SimpleInjector;
using SimpleInjector;
using Xunit;
using Xunit.Categories;

namespace Energinet.DataHub.PostOffice.Tests.Common.SimpleInjector
{
    [UnitTest]
    public class SimpleInjectorActivatorTests
    {
        [Fact]
        public async Task Invoke_InstanceTypeIsNull_Throws()
        {
            // arrange
            await using var container = new Container();
            var target = new SimpleInjectorActivator(container);

            // act, assert
            Assert.Throws<ArgumentNullException>(() => target.CreateInstance(null!, new MockedFunctionContext()));
        }

        [Fact]
        public async Task Invoke_AllGood_ResolvesInstance()
        {
            // arrange
            await using var container = new Container();
            container.Register<IFoo>(() => new Foo());

            var target = new SimpleInjectorActivator(container);

            // act
            var actual = target.CreateInstance(typeof(IFoo), new MockedFunctionContext());

            // assert
            Assert.NotNull(actual);
            Assert.IsType<Foo>(actual);
        }

#pragma warning disable SA1201
#pragma warning disable SA1600
        private interface IFoo
#pragma warning restore SA1600
#pragma warning restore SA1201
        {
        }

        private sealed class Foo : IFoo
        {
        }
    }
}
