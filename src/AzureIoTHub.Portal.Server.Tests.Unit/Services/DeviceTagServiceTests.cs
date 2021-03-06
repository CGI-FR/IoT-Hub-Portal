// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace AzureIoTHub.Portal.Server.Tests.Unit.Services
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Azure;
    using Azure.Data.Tables;
    using AzureIoTHub.Portal.Server.Factories;
    using AzureIoTHub.Portal.Server.Mappers;
    using AzureIoTHub.Portal.Server.Services;
    using AzureIoTHub.Portal.Models.v10;
    using FluentAssertions;
    using Moq;
    using NUnit.Framework;
    using Server.Exceptions;

    [TestFixture]
    public class DeviceTagServiceTests
    {
        private MockRepository mockRepository;
        private Mock<IDeviceTagMapper> mockDeviceTagMapper;
        private Mock<ITableClientFactory> mockTableClientFactory;
        private Mock<TableClient> mockDeviceTagTableClient;

        [SetUp]
        public void SetUp()
        {
            this.mockRepository = new MockRepository(MockBehavior.Strict);
            this.mockDeviceTagMapper = this.mockRepository.Create<IDeviceTagMapper>();
            this.mockTableClientFactory = this.mockRepository.Create<ITableClientFactory>();
            this.mockDeviceTagTableClient = this.mockRepository.Create<TableClient>();
        }

        public DeviceTagService CreateDeviceTagService()
        {
            return new DeviceTagService(
                this.mockDeviceTagMapper.Object,
                this.mockTableClientFactory.Object);
        }

        [Test]
        public async Task UpdateShouldCreateNewEntity()
        {
            var deviceTagService = CreateDeviceTagService();

            var tag = new DeviceTag
            {
                Name = "testName",
                Label = "testLabel",
                Required = true,
                Searchable = true
            };

            var mockResponse = this.mockRepository.Create<Response>();

            _ = this.mockDeviceTagMapper.Setup(c => c.UpdateTableEntity(
               It.Is<TableEntity>(x => x.RowKey == tag.Name && x.PartitionKey == DeviceTagService.DefaultPartitionKey),
               It.Is<DeviceTag>(x => x == tag)));

            _ = this.mockDeviceTagTableClient.Setup(c => c.AddEntityAsync(
                It.Is<TableEntity>(x => x.PartitionKey == DeviceTagService.DefaultPartitionKey && x.RowKey == tag.Name),
                It.IsAny<CancellationToken>()))
                .ReturnsAsync(mockResponse.Object);

            _ = this.mockDeviceTagTableClient.Setup(c => c.Query<TableEntity>(
                It.Is<string>(_ => true),
                It.IsAny<int?>(),
                It.IsAny<IEnumerable<string>>(),
                It.IsAny<CancellationToken>()))
                .Returns(Pageable<TableEntity>.FromPages(new[] {
                    Page<TableEntity>.FromValues(new[]
                    {
                        new TableEntity(DeviceTagService.DefaultPartitionKey,tag.Name)
                    }, null, mockResponse.Object)
                }));

            _ = this.mockDeviceTagTableClient.Setup(c => c.DeleteEntityAsync(
                It.Is<string>(_ => true),
                It.IsAny<string>(),
                It.IsAny<ETag>(),
                It.IsAny<CancellationToken>()))
                .ReturnsAsync(mockResponse.Object);

            _ = this.mockTableClientFactory.Setup(c => c.GetDeviceTagSettings())
                .Returns(this.mockDeviceTagTableClient.Object);

            // Act
            await deviceTagService.UpdateTags(new List<DeviceTag>(new[] { tag }));

            // Assert
            this.mockTableClientFactory.Verify(c => c.GetDeviceTagSettings());

            this.mockDeviceTagMapper.VerifyAll();

            this.mockDeviceTagTableClient.Verify(c => c.AddEntityAsync(
                It.Is<TableEntity>(x => x.RowKey == tag.Name && x.PartitionKey == DeviceTagService.DefaultPartitionKey),
                It.IsAny<CancellationToken>()), Times.Once());

            this.mockRepository.VerifyAll();
        }

        [Test]
        public async Task UpdateTagsShouldThrowInternalServerErrorExceptionWhenGettingExistingTags()
        {
            var deviceTagService = CreateDeviceTagService();

            var tag = new DeviceTag
            {
                Name = "testName",
                Label = "testLabel",
                Required = true,
                Searchable = true
            };

            _ = this.mockDeviceTagTableClient.Setup(c => c.Query<TableEntity>(
                    It.Is<string>(_ => true),
                    It.IsAny<int?>(),
                    It.IsAny<IEnumerable<string>>(),
                    It.IsAny<CancellationToken>()))
                .Throws(new RequestFailedException("test"));

            _ = this.mockTableClientFactory.Setup(c => c.GetDeviceTagSettings())
                .Returns(this.mockDeviceTagTableClient.Object);

            // Act
            var act = () => deviceTagService.UpdateTags(new List<DeviceTag>(new[] { tag }));

            // Assert
            _ = await act.Should().ThrowAsync<InternalServerErrorException>();
            this.mockRepository.VerifyAll();
        }

        [Test]
        public async Task UpdateTagsShouldThrowInternalServerErrorExceptionWhenDeletingExistingTags()
        {
            var deviceTagService = CreateDeviceTagService();

            var tag = new DeviceTag
            {
                Name = "testName",
                Label = "testLabel",
                Required = true,
                Searchable = true
            };

            var mockResponse = this.mockRepository.Create<Response>();

            _ = this.mockDeviceTagTableClient.Setup(c => c.Query<TableEntity>(
                It.Is<string>(_ => true),
                It.IsAny<int?>(),
                It.IsAny<IEnumerable<string>>(),
                It.IsAny<CancellationToken>()))
                .Returns(Pageable<TableEntity>.FromPages(new[] {
                    Page<TableEntity>.FromValues(new[]
                    {
                        new TableEntity(DeviceTagService.DefaultPartitionKey,tag.Name)
                    }, null, mockResponse.Object)
                }));

            _ = this.mockDeviceTagTableClient.Setup(c => c.DeleteEntityAsync(
                It.Is<string>(_ => true),
                It.IsAny<string>(),
                It.IsAny<ETag>(),
                It.IsAny<CancellationToken>()))
                .ThrowsAsync(new RequestFailedException("test"));

            _ = this.mockTableClientFactory.Setup(c => c.GetDeviceTagSettings())
                .Returns(this.mockDeviceTagTableClient.Object);

            // Act
            var act = () => deviceTagService.UpdateTags(new List<DeviceTag>(new[] { tag }));

            // Assert
            _ = await act.Should().ThrowAsync<InternalServerErrorException>();
            this.mockRepository.VerifyAll();
        }

        [Test]
        public async Task UpdateTagsShouldThrowInternalServerErrorExceptionWhenAddingNewTags()
        {
            var deviceTagService = CreateDeviceTagService();

            var tag = new DeviceTag
            {
                Name = "testName",
                Label = "testLabel",
                Required = true,
                Searchable = true
            };

            var mockResponse = this.mockRepository.Create<Response>();

            _ = this.mockDeviceTagMapper.Setup(c => c.UpdateTableEntity(
               It.Is<TableEntity>(x => x.RowKey == tag.Name && x.PartitionKey == DeviceTagService.DefaultPartitionKey),
               It.Is<DeviceTag>(x => x == tag)));

            _ = this.mockDeviceTagTableClient.Setup(c => c.AddEntityAsync(
                It.Is<TableEntity>(x => x.PartitionKey == DeviceTagService.DefaultPartitionKey && x.RowKey == tag.Name),
                It.IsAny<CancellationToken>()))
                .ThrowsAsync(new RequestFailedException("test"));

            _ = this.mockDeviceTagTableClient.Setup(c => c.Query<TableEntity>(
                It.Is<string>(_ => true),
                It.IsAny<int?>(),
                It.IsAny<IEnumerable<string>>(),
                It.IsAny<CancellationToken>()))
                .Returns(Pageable<TableEntity>.FromPages(new[] {
                    Page<TableEntity>.FromValues(new[]
                    {
                        new TableEntity(DeviceTagService.DefaultPartitionKey,tag.Name)
                    }, null, mockResponse.Object)
                }));

            _ = this.mockDeviceTagTableClient.Setup(c => c.DeleteEntityAsync(
                It.Is<string>(_ => true),
                It.IsAny<string>(),
                It.IsAny<ETag>(),
                It.IsAny<CancellationToken>()))
                .ReturnsAsync(mockResponse.Object);

            _ = this.mockTableClientFactory.Setup(c => c.GetDeviceTagSettings())
                .Returns(this.mockDeviceTagTableClient.Object);

            // Act
            var act = () => deviceTagService.UpdateTags(new List<DeviceTag>(new[] { tag }));

            // Assert
            _ = await act.Should().ThrowAsync<InternalServerErrorException>();
            this.mockRepository.VerifyAll();
        }

        [Test]
        public void GetAllTagsShouldReturnAList()
        {
            // Arrange
            var deviceTagService = CreateDeviceTagService();
            var returnedIndex = 10;

            var mockTableResponse = this.mockRepository.Create<Pageable<TableEntity>>();
            var mockEnumerator = this.mockRepository.Create<IEnumerator<TableEntity>>();
            _ = mockEnumerator.Setup(x => x.Dispose()).Callback(() => { });
            _ = mockEnumerator.Setup(x => x.MoveNext()).Returns(() => returnedIndex-- > 0);

            _ = mockEnumerator.Setup(x => x.Current)
                .Returns(new TableEntity(DeviceTagService.DefaultPartitionKey, "test"));

            _ = mockTableResponse.Setup(x => x.GetEnumerator()).Returns(mockEnumerator.Object);

            _ = this.mockDeviceTagTableClient.Setup(c => c.Query<TableEntity>(It.IsAny<string>(), It.IsAny<int?>(), It.IsAny<IEnumerable<string>>(), It.IsAny<CancellationToken>()))
                .Returns(mockTableResponse.Object);

            _ = this.mockTableClientFactory.Setup(c => c.GetDeviceTagSettings())
                .Returns(this.mockDeviceTagTableClient.Object);

            _ = this.mockDeviceTagMapper.Setup(c => c.GetDeviceTag(It.IsAny<TableEntity>()))
                .Returns((TableEntity _) => new DeviceTag
                {
                    Name = Guid.NewGuid().ToString(),
                    Label = "test",
                    Required = true,
                    Searchable = true
                });

            // Act
            var result = deviceTagService.GetAllTags();

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(10, result.Count());
            Assert.IsAssignableFrom<DeviceTag>(result.First());

            this.mockRepository.VerifyAll();
        }

        [Test]
        public void GetAllTagsShouldThrowInternalServerErrorExceptionWhenIssueOccurs()
        {
            // Arrange
            var deviceTagService = CreateDeviceTagService();

            _ = this.mockDeviceTagTableClient.Setup(c => c.Query<TableEntity>(It.IsAny<string>(), It.IsAny<int?>(),
                    It.IsAny<IEnumerable<string>>(), It.IsAny<CancellationToken>()))
                .Throws(new RequestFailedException("test"));

            _ = this.mockTableClientFactory.Setup(c => c.GetDeviceTagSettings())
                .Returns(this.mockDeviceTagTableClient.Object);

            // Act
            var act = () => deviceTagService.GetAllTags();

            // Assert
            _ = act.Should().Throw<InternalServerErrorException>();
            this.mockRepository.VerifyAll();
        }

        [Test]
        public void GetAllTagsNamesShouldReturnAList()
        {
            // Arrange
            var deviceTagService = CreateDeviceTagService();
            var returnedIndex = 10;

            var mockTableResponse = this.mockRepository.Create<Pageable<TableEntity>>();
            var mockEnumerator = this.mockRepository.Create<IEnumerator<TableEntity>>();
            _ = mockEnumerator.Setup(x => x.Dispose()).Callback(() => { });
            _ = mockEnumerator.Setup(x => x.MoveNext()).Returns(() => returnedIndex-- > 0);

            _ = mockEnumerator.Setup(x => x.Current)
                .Returns(new TableEntity(DeviceTagService.DefaultPartitionKey, "test"));

            _ = mockTableResponse.Setup(x => x.GetEnumerator()).Returns(mockEnumerator.Object);

            _ = this.mockDeviceTagTableClient.Setup(c => c.Query<TableEntity>(It.IsAny<string>(), It.IsAny<int?>(), It.IsAny<IEnumerable<string>>(), It.IsAny<CancellationToken>()))
                .Returns(mockTableResponse.Object);

            _ = this.mockTableClientFactory.Setup(c => c.GetDeviceTagSettings())
                .Returns(this.mockDeviceTagTableClient.Object);

            _ = this.mockDeviceTagMapper.Setup(c => c.GetDeviceTag(It.IsAny<TableEntity>()))
                .Returns((TableEntity _) => new DeviceTag
                {
                    Name = Guid.NewGuid().ToString(),
                    Label = "test",
                    Required = true,
                    Searchable = true
                });

            // Act
            var result = deviceTagService.GetAllTagsNames();

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(10, result.Count());
            Assert.IsAssignableFrom<string>(result.First());

            this.mockRepository.VerifyAll();
        }

        [Test]
        public void GetAllTagsNamesShouldThrowInternalServerErrorExceptionWhenAnIssueOccurs()
        {
            // Arrange
            var deviceTagService = CreateDeviceTagService();

            _ = this.mockDeviceTagTableClient.Setup(c => c.Query<TableEntity>(It.IsAny<string>(), It.IsAny<int?>(), It.IsAny<IEnumerable<string>>(), It.IsAny<CancellationToken>()))
                .Throws(new RequestFailedException("test"));

            _ = this.mockTableClientFactory.Setup(c => c.GetDeviceTagSettings())
                .Returns(this.mockDeviceTagTableClient.Object);

            // Act
            var act = () => deviceTagService.GetAllTagsNames();

            // Assert
            _ = act.Should().Throw<InternalServerErrorException>();
            this.mockRepository.VerifyAll();
        }

        [Test]
        public void GetAllSearchableTagsNamesShouldReturnAList()
        {
            // Arrange
            var deviceTagService = CreateDeviceTagService();
            var boolList = new List<bool>() { true, false, true };
            var returnedIndex = boolList.Count;

            var mockTableResponse = this.mockRepository.Create<Pageable<TableEntity>>();
            var mockEnumerator = this.mockRepository.Create<IEnumerator<TableEntity>>();
            _ = mockEnumerator.Setup(x => x.Dispose()).Callback(() => { });
            _ = mockEnumerator.Setup(x => x.MoveNext()).Returns(() => returnedIndex-- > 0);

            _ = mockEnumerator.Setup(x => x.Current)
                .Returns(() =>
                {
                    return new TableEntity(DeviceTagService.DefaultPartitionKey, "test")
                    {
                        [nameof(DeviceTag.Searchable)] = boolList[returnedIndex],
                    };
                });

            _ = mockTableResponse.Setup(x => x.GetEnumerator()).Returns(mockEnumerator.Object);

            _ = this.mockDeviceTagTableClient.Setup(c => c.Query<TableEntity>(It.IsAny<string>(), It.IsAny<int?>(), It.IsAny<IEnumerable<string>>(), It.IsAny<CancellationToken>()))
                .Returns(mockTableResponse.Object);

            _ = this.mockTableClientFactory.Setup(c => c.GetDeviceTagSettings())
                .Returns(this.mockDeviceTagTableClient.Object);

            _ = this.mockDeviceTagMapper.Setup(c => c.GetDeviceTag(It.IsAny<TableEntity>()))
                .Returns((TableEntity entity) => new DeviceTag
                {
                    Name = Guid.NewGuid().ToString(),
                    Label = "test",
                    Required = true,
                    Searchable = bool.Parse(entity[nameof(DeviceTag.Searchable)].ToString())
                });

            // Act
            var result = deviceTagService.GetAllSearchableTagsNames();

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(2, result.Count());
            Assert.IsAssignableFrom<string>(result.First());

            this.mockRepository.VerifyAll();
        }

        [Test]
        public void GetAllSearchableTagsNamesShouldThrowInternalServerErrorExceptionWhenAnIssueOccurs()
        {
            // Arrange
            var deviceTagService = CreateDeviceTagService();

            _ = this.mockDeviceTagTableClient.Setup(c => c.Query<TableEntity>(It.IsAny<string>(), It.IsAny<int?>(), It.IsAny<IEnumerable<string>>(), It.IsAny<CancellationToken>()))
                .Throws(new RequestFailedException("test"));

            _ = this.mockTableClientFactory.Setup(c => c.GetDeviceTagSettings())
                .Returns(this.mockDeviceTagTableClient.Object);

            // Act
            var act = () => deviceTagService.GetAllSearchableTagsNames();

            // Assert
            _ = act.Should().Throw<InternalServerErrorException>();
            this.mockRepository.VerifyAll();
        }

        [Test]
        public async Task CreateOrUpdateDeviceTagShouldUpsertDeviceTag()
        {
            // Arrange
            var deviceTag = new DeviceTag
            {
                Name = Guid.NewGuid().ToString()
            };

            var deviceTagService = CreateDeviceTagService();

            var mockResponse = this.mockRepository.Create<Response>();

            _ = this.mockDeviceTagMapper.Setup(c => c.UpdateTableEntity(
                It.Is<TableEntity>(x => x.RowKey == deviceTag.Name && x.PartitionKey == DeviceTagService.DefaultPartitionKey),
                It.Is<DeviceTag>(x => x == deviceTag)));

            _ = this.mockDeviceTagTableClient.Setup(c => c.UpsertEntityAsync(It.Is<TableEntity>(x =>
                    x.PartitionKey == DeviceTagService.DefaultPartitionKey && x.RowKey == deviceTag.Name), It.IsAny<TableUpdateMode>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(mockResponse.Object);

            _ = this.mockTableClientFactory.Setup(c => c.GetDeviceTagSettings())
                .Returns(this.mockDeviceTagTableClient.Object);

            // Act
            var act = () => deviceTagService.CreateOrUpdateDeviceTag(deviceTag);

            // Assert
            _ = await act.Should().NotThrowAsync();
            this.mockRepository.VerifyAll();
        }

        [Test]
        public async Task CreateOrUpdateDeviceTagShouldThrowInternalServerErrorExceptionWhenAnIssueOccurs()
        {
            // Arrange
            var deviceTag = new DeviceTag
            {
                Name = Guid.NewGuid().ToString()
            };

            var deviceTagService = CreateDeviceTagService();

            _ = this.mockDeviceTagMapper.Setup(c => c.UpdateTableEntity(
                It.Is<TableEntity>(x => x.RowKey == deviceTag.Name && x.PartitionKey == DeviceTagService.DefaultPartitionKey),
                It.Is<DeviceTag>(x => x == deviceTag)));

            _ = this.mockDeviceTagTableClient.Setup(c => c.UpsertEntityAsync(It.Is<TableEntity>(x =>
                    x.PartitionKey == DeviceTagService.DefaultPartitionKey && x.RowKey == deviceTag.Name), It.IsAny<TableUpdateMode>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new RequestFailedException("test"));

            _ = this.mockTableClientFactory.Setup(c => c.GetDeviceTagSettings())
                .Returns(this.mockDeviceTagTableClient.Object);

            // Act
            var act = () => deviceTagService.CreateOrUpdateDeviceTag(deviceTag);

            // Assert
            _ = await act.Should().ThrowAsync<InternalServerErrorException>();
            this.mockRepository.VerifyAll();
        }

        [Test]
        public async Task DeleteDeviceTagByNameShouldDeleteDeviceTag()
        {
            // Arrange
            var deviceTag = new DeviceTag
            {
                Name = Guid.NewGuid().ToString()
            };

            var deviceTagService = CreateDeviceTagService();

            var mockResponse = this.mockRepository.Create<Response>();

            _ = this.mockDeviceTagTableClient.Setup(c => c.DeleteEntityAsync(
                    It.Is<string>(x => x.Equals(DeviceTagService.DefaultPartitionKey, StringComparison.Ordinal)),
                    It.Is<string>(x => x.Equals(deviceTag.Name, StringComparison.Ordinal)),
                    It.IsAny<ETag>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(mockResponse.Object);

            _ = this.mockTableClientFactory.Setup(c => c.GetDeviceTagSettings())
                .Returns(this.mockDeviceTagTableClient.Object);

            // Act
            var act = () => deviceTagService.DeleteDeviceTagByName(deviceTag.Name);

            // Assert
            _ = await act.Should().NotThrowAsync();
            this.mockRepository.VerifyAll();
        }

        [Test]
        public async Task DeleteDeviceTagByNameShouldThrowInternalServerErrorExceptionWhenAnIssueOccurs()
        {
            // Arrange
            var deviceTag = new DeviceTag
            {
                Name = Guid.NewGuid().ToString()
            };

            var deviceTagService = CreateDeviceTagService();

            _ = this.mockDeviceTagTableClient.Setup(c => c.DeleteEntityAsync(
                    It.Is<string>(x => x.Equals(DeviceTagService.DefaultPartitionKey, StringComparison.Ordinal)),
                    It.Is<string>(x => x.Equals(deviceTag.Name, StringComparison.Ordinal)),
                    It.IsAny<ETag>(),
                    It.IsAny<CancellationToken>()))
                .ThrowsAsync(new RequestFailedException("test"));

            _ = this.mockTableClientFactory.Setup(c => c.GetDeviceTagSettings())
                .Returns(this.mockDeviceTagTableClient.Object);

            // Act
            var act = () => deviceTagService.DeleteDeviceTagByName(deviceTag.Name);

            // Assert
            _ = await act.Should().ThrowAsync<InternalServerErrorException>();
            this.mockRepository.VerifyAll();
        }
    }
}
