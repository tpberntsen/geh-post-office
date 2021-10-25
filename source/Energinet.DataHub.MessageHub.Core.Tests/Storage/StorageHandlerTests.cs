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
using System.IO;
using System.Threading.Tasks;
using Azure;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Energinet.DataHub.MessageHub.Core.Factories;
using Energinet.DataHub.MessageHub.Core.Storage;
using Energinet.DataHub.MessageHub.Model.Exceptions;
using Moq;
using Xunit;
using Xunit.Categories;

namespace Energinet.DataHub.MessageHub.Core.Tests.Storage
{
    [UnitTest]
    public class StorageHandlerTests
    {
        [Fact]
        public async Task GetStreamFromStorageAsync_UriIsValid_ReturnsStream()
        {
            // arrange
            var mockedStorageServiceClientFactory = new Mock<IStorageServiceClientFactory>();
            var mockedBlobServiceClient = new Mock<BlobServiceClient>();
            var mockedBlobContainerClient = new Mock<BlobContainerClient>();
            var mockedBlobClient = new Mock<BlobClient>();
            var mockedResponse = new Mock<Response<BlobDownloadStreamingResult>>();
            await using var inputStream = new MemoryStream(new byte[] { 1, 2, 3 });
            var mockedBlobDownloadStreamingResult = MockedBlobDownloadStreamingResult.Create(inputStream);
            var testUri = new Uri("https://test.test.dk/FileStorage/postoffice-blobstorage");

            mockedResponse.Setup(
                    x => x.Value)
                .Returns(mockedBlobDownloadStreamingResult);

            mockedBlobClient.Setup(
                    x => x.DownloadStreamingAsync(
                        default,
                        default,
                        default,
                        default))
                .ReturnsAsync(mockedResponse.Object);

            mockedBlobContainerClient.Setup(
                    x => x.GetBlobClient(It.IsAny<string>()))
                .Returns(mockedBlobClient.Object);

            mockedBlobServiceClient.Setup(
                    x => x.GetBlobContainerClient(It.IsAny<string>()))
                .Returns(mockedBlobContainerClient.Object);

            mockedStorageServiceClientFactory.Setup(
                    x => x.Create())
                .Returns(mockedBlobServiceClient.Object);

            var target = new StorageHandler(mockedStorageServiceClientFactory.Object, new StorageConfig("fake_value"));

            // act
            var result = await target.GetStreamFromStorageAsync(testUri).ConfigureAwait(false);

            // assert
            Assert.NotNull(result);
        }

        [Fact]
        public async Task GetStreamFromStorageAsync_ContentPathIsNull_ThrowsArgumentNullException()
        {
            // arrange
            var mockedStorageServiceClientFactory = new Mock<IStorageServiceClientFactory>();
            var target = new StorageHandler(mockedStorageServiceClientFactory.Object, new StorageConfig("fake_value"));

            // act, assert
            await Assert.ThrowsAsync<ArgumentNullException>(
                    () => target.GetStreamFromStorageAsync(null!))
                .ConfigureAwait(false);
        }

        [Fact]
        public async Task GetStreamFromStorageAsync_StorageFailure_ThrowsCustomException()
        {
            // arrange
            var mockedStorageServiceClientFactory = new Mock<IStorageServiceClientFactory>();
            var mockedBlobServiceClient = new Mock<BlobServiceClient>();
            var mockedBlobContainerClient = new Mock<BlobContainerClient>();
            var mockedBlobClient = new Mock<BlobClient>();
            var testUri = new Uri("https://test.test.dk/FileStorage/postoffice-blobstorage");

            mockedBlobClient.Setup(
                    x => x.DownloadStreamingAsync(
                        default,
                        default,
                        default,
                        default))
                .ThrowsAsync(new RequestFailedException("test"));

            mockedBlobContainerClient.Setup(
                    x => x.GetBlobClient(It.IsAny<string>()))
                .Returns(mockedBlobClient.Object);

            mockedBlobServiceClient.Setup(
                    x => x.GetBlobContainerClient(It.IsAny<string>()))
                .Returns(mockedBlobContainerClient.Object);

            mockedStorageServiceClientFactory.Setup(
                    x => x.Create())
                .Returns(mockedBlobServiceClient.Object);

            var target = new StorageHandler(mockedStorageServiceClientFactory.Object, new StorageConfig("fake_value"));

            // act, assert
            await Assert.ThrowsAsync<MessageHubStorageException>(
                    () => target.GetStreamFromStorageAsync(testUri))
                .ConfigureAwait(false);
        }
    }
}
