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
using Energinet.DataHub.MessageHub.Client.Factories;
using Energinet.DataHub.MessageHub.Client.Storage;
using Energinet.DataHub.MessageHub.Model.Exceptions;
using Energinet.DataHub.MessageHub.Model.Model;
using Moq;
using Xunit;
using Xunit.Categories;

namespace Energinet.DataHub.MessageHub.Client.Tests.Storage
{
    [UnitTest]
    public class StorageHandlerTests
    {
        [Fact]
        public async Task AddStreamToStorageAsync_StreamIsEmpty_ThrowsArgumentException()
        {
            // arrange
            var mockedStorageServiceClientFactory = new Mock<IStorageServiceClientFactory>();
            var mockedBlobServiceClient = new Mock<BlobServiceClient>();
            var mockedDataBundleRequestDto = new DataBundleRequestDto(
                Guid.NewGuid(),
                "A6A7912F-A578-4448-A2C1-6545F4C8EA8C",
                Guid.NewGuid().ToString(),
                Guid.NewGuid().ToString());

            mockedStorageServiceClientFactory.Setup(
                    x => x.Create())
                .Returns(mockedBlobServiceClient.Object);

            var target = new StorageHandler(mockedStorageServiceClientFactory.Object, new StorageConfig("fake_value"));

            // act, assert
            await Assert.ThrowsAsync<ArgumentException>(
                () => target.AddStreamToStorageAsync(
                    Stream.Null,
                    mockedDataBundleRequestDto))
                .ConfigureAwait(false);
        }

        [Fact]
        public async Task AddStreamToStorageAsync_RequestDtoIsNull_ThrowsArgumentNullException()
        {
            // arrange
            var mockedStorageServiceClientFactory = new Mock<IStorageServiceClientFactory>();
            var mockedBlobServiceClient = new Mock<BlobServiceClient>();

            mockedStorageServiceClientFactory
                .Setup(x => x.Create())
                .Returns(mockedBlobServiceClient.Object);

            var target = new StorageHandler(mockedStorageServiceClientFactory.Object, new StorageConfig("fake_value"));

            // act, assert
            await Assert.ThrowsAsync<ArgumentNullException>(
                    () => target.AddStreamToStorageAsync(
                        Stream.Null,
                        null!))
                .ConfigureAwait(false);
        }

        [Fact]
        public async Task AddStreamToStorageAsync_RequestFailure_ThrowsCustomException()
        {
            // arrange
            var mockedStorageServiceClientFactory = new Mock<IStorageServiceClientFactory>();
            var mockedBlobServiceClient = new Mock<BlobServiceClient>();
            var mockedBlobContainerClient = new Mock<BlobContainerClient>();
            var mockedBlobClient = new Mock<BlobClient>();
            var mockedDataBundleRequestDto = new DataBundleRequestDto(
                Guid.NewGuid(),
                "9B0A9B6C-EA5F-4DD0-817B-4D29A681984E",
                Guid.NewGuid().ToString(),
                Guid.NewGuid().ToString());

            mockedBlobServiceClient.Setup(
                    x => x.GetBlobContainerClient(It.IsAny<string>()))
                .Returns(mockedBlobContainerClient.Object);

            mockedBlobContainerClient
                .Setup(client => client.GetBlobClient(It.IsAny<string>()))
                .Returns(mockedBlobClient.Object);

            mockedBlobClient.Setup(
                    x => x.UploadAsync(
                        It.IsAny<Stream>(),
                        It.IsAny<bool>(),
                        default))
                .ThrowsAsync(new RequestFailedException("test"));

            mockedStorageServiceClientFactory.Setup(
                    x => x.Create())
                .Returns(mockedBlobServiceClient.Object);

            var target = new StorageHandler(mockedStorageServiceClientFactory.Object, new StorageConfig("fake_value"));

            // act, assert
            await using var inputStream = new MemoryStream(new byte[] { 1, 2, 3 });
            await Assert.ThrowsAsync<MessageHubStorageException>(
                    () => target.AddStreamToStorageAsync(
                        inputStream,
                        mockedDataBundleRequestDto))
                .ConfigureAwait(false);
        }

        [Fact]
        public async Task AddStreamToStorageAsync_StreamIsValid_ReturnsUri()
        {
            // arrange
            var mockedStorageServiceClientFactory = new Mock<IStorageServiceClientFactory>();
            var mockedBlobServiceClient = new Mock<BlobServiceClient>();
            var mockedBlobContainerClient = new Mock<BlobContainerClient>();
            var mockedBlobClient = new Mock<BlobClient>();
            var mockedDataBundleRequestDto = new DataBundleRequestDto(
                Guid.NewGuid(),
                "24AEFA9F-91D5-4908-A111-157C48D7FC6D",
                Guid.NewGuid().ToString(),
                Guid.NewGuid().ToString());

            var testUri = new Uri("https://test.test.dk/FileStorage/postoffice-blobstorage");
            mockedBlobClient.Setup(
                    x => x.Uri)
                .Returns(testUri);

            mockedBlobContainerClient.Setup(
                x => x.UploadBlobAsync(It.IsAny<string>(), It.IsAny<Stream>(), default));

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
            await using var inputStream = new MemoryStream(new byte[] { 1, 2, 3 });
            var result = await target.AddStreamToStorageAsync(inputStream, mockedDataBundleRequestDto).ConfigureAwait(false);

            // assert
            Assert.Equal(testUri, result);
        }

        [Fact]
        public async Task GetDataAvailableNotificationIdsAsync_DataBundleRequestIsNull_ThrowsException()
        {
            // arrange
            var mockedStorageServiceClientFactory = new Mock<IStorageServiceClientFactory>();
            var mockedBlobServiceClient = new Mock<BlobServiceClient>();

            mockedStorageServiceClientFactory
                .Setup(x => x.Create())
                .Returns(mockedBlobServiceClient.Object);

            var target = new StorageHandler(mockedStorageServiceClientFactory.Object, new StorageConfig("fake_value"));

            // act, assert
            await Assert
                .ThrowsAsync<ArgumentNullException>(() => target.GetDataAvailableNotificationIdsAsync(((DataBundleRequestDto)null)!))
                .ConfigureAwait(false);
        }

        [Fact]
        public async Task GetDataAvailableNotificationIdsAsync_DequeueNotificationIsNull_ThrowsException()
        {
            // arrange
            var mockedStorageServiceClientFactory = new Mock<IStorageServiceClientFactory>();
            var mockedBlobServiceClient = new Mock<BlobServiceClient>();

            mockedStorageServiceClientFactory
                .Setup(x => x.Create())
                .Returns(mockedBlobServiceClient.Object);

            var target = new StorageHandler(mockedStorageServiceClientFactory.Object, new StorageConfig("fake_value"));

            // act, assert
            await Assert
                .ThrowsAsync<ArgumentNullException>(() => target.GetDataAvailableNotificationIdsAsync(((DequeueNotificationDto)null)!))
                .ConfigureAwait(false);
        }
    }
}
