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
using Energinet.DataHub.PostOffice.Common.Extensions;
using Xunit;
using Xunit.Categories;

namespace Energinet.DataHub.PostOffice.Tests.Common.Extensions
{
    [UnitTest]
    public class UriExtensionsTests
    {
        [Fact]
        public void Should_parse()
        {
            // arrange
            const string someString = "value";
            const int someInt = 42;
            var someDate = new DateTime(2012, 08, 24, 14, 13, 12);
            var url = new Uri($"http://localhost:8080?{nameof(someString)}={someString}&{nameof(someDate)}={someDate:O}&{nameof(someInt)}={someInt}");

            // act
            var (actualString, actualInt, actualDate) = url.ParseQuery<CommandWithMultipleProps>();

            // assert
            Assert.Equal(someString, actualString);
            Assert.Equal(someInt, actualInt);
            Assert.Equal(someDate, actualDate);
        }

        [Fact]
        public void Should_parse_nullable_props()
        {
            // arrange
            const string? someNullableString = "value";
            var url = new Uri($"http://localhost:8080?{nameof(someNullableString)}={someNullableString}");

            // act
            var (actualString, actualNullableInt) = url.ParseQuery<CommandWithNullableProps>();

            // assert
            Assert.Equal(someNullableString, actualString);
            Assert.Null(actualNullableInt);
        }

        [Fact]
        public void Should_default_to_default_value()
        {
            // arrange
            var url = new Uri("http://localhost:8080");

            // act
            var actual = url.ParseQuery<CommandWithDefaultValueParam>();

            // assert
            Assert.Equal("hello_there", actual.SomeString);
        }

        [Fact]
        public void Should_parse_collection_props()
        {
            // arrange
            var someStringCollection = new[] { "v1", "v2" };
            var someNullableDateCollection = new[] { new DateTime(2021, 8, 25, 13, 14, 15), new DateTime(2021, 8, 26, 13, 14, 15) };
            var url = new Uri("http://localhost:8080" +
                              $"?{string.Join("&", someStringCollection.Select(x => $"{nameof(someStringCollection)}={x}"))}" +
                              $"&{string.Join("&", someNullableDateCollection.Select(x => $"{nameof(someNullableDateCollection)}={x:O}"))}");

            // act
            var (actualStringCollection, actualNullableIntCollection, actualNullableDateCollection) = url.ParseQuery<CommandWithEnumeralProps>();

            // assert
            Assert.Equal(someStringCollection, actualStringCollection);
            Assert.Null(actualNullableIntCollection);
            Assert.Equal(someNullableDateCollection, actualNullableDateCollection);
        }

        [Fact]
        public void Should_throw_argument_exception_if_unable_to_parse()
        {
            // arrange
            var url = new Uri($"http://localhost:8080?{nameof(CommandWithMultipleProps.SomeDate)}=invalid_date_value");

            // act
            var actual = Assert.Throws<ArgumentException>(() => url.ParseQuery<CommandWithMultipleProps>());

            // assert
            Assert.Contains(url.Query, actual.Message, StringComparison.InvariantCulture);
            Assert.Contains(nameof(CommandWithMultipleProps), actual.Message, StringComparison.InvariantCulture);
        }

        [Fact]
        public void Should_check_uri_for_null()
        {
            // arrange, act, assert
            Assert.Throws<ArgumentNullException>(() => default(Uri)!.ParseQuery<CommandWithEnumeralProps>());
        }

        // ReSharper disable ClassNeverInstantiated.Local
        private record CommandWithDefaultValueParam(string SomeString = "hello_there");
        private record CommandWithMultipleProps(string SomeString, int SomeInt, DateTime SomeDate);
        private record CommandWithNullableProps(string? SomeNullableString, int? SomeNullableInt);
        private record CommandWithEnumeralProps(IEnumerable<string> SomeStringCollection, IEnumerable<int>? SomeNullableIntCollection, IEnumerable<DateTime>? SomeNullableDateCollection);
    }
}
