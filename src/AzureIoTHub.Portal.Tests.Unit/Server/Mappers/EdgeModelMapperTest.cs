// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace AzureIoTHub.Portal.Tests.Unit.Server.Mappers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Azure.Data.Tables;
    using AzureIoTHub.Portal.Models.v10;
    using AzureIoTHub.Portal.Server.Entities;
    using AzureIoTHub.Portal.Server.Managers;
    using AzureIoTHub.Portal.Server.Mappers;
    using Moq;
    using NUnit.Framework;

    [TestFixture]
    public class EdgeModelMapperTest
    {
        private MockRepository mockRepository;

        private Mock<IDeviceModelImageManager> mockDeviceModelImageManager;

        [SetUp]
        public void SetUp()
        {
            this.mockRepository = new MockRepository(MockBehavior.Strict);

            this.mockDeviceModelImageManager = this.mockRepository.Create<IDeviceModelImageManager>();
        }

        private EdgeModelMapper CreateEdgeModelMapper()
        {
            return new EdgeModelMapper(this.mockDeviceModelImageManager.Object);
        }

        [Test]
        public void CreateEdgeDeviceModelListItemShouldReturnIoTEdgeModelListItemObject()
        {
            // Arrange
            var edgeModelMapper = CreateEdgeModelMapper();

            var partitionKey = Guid.NewGuid().ToString();
            var rowKey = "000-000-001";

            _ = this.mockDeviceModelImageManager
                .Setup(x => x.ComputeImageUri(It.IsAny<string>()))
                .Returns(new Uri("http://fake.local/000-000-001"));

            var entity = new TableEntity(partitionKey, rowKey)
            {
                ["Name"] = "test-name",
                ["Description"] = "description_test",
            };

            // Act
            var result = edgeModelMapper.CreateEdgeDeviceModelListItem(entity);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(rowKey, result.ModelId);
            Assert.AreEqual("test-name", result.Name);

            this.mockRepository.VerifyAll();
        }

        [Test]
        public void CreateEdgeDeviceModelShouldReturnIoTEdgeModelObject()
        {
            // Arrange
            var edgeModelMapper = CreateEdgeModelMapper();

            var partitionKey = Guid.NewGuid().ToString();
            var rowKey = "000-000-001";

            _ = this.mockDeviceModelImageManager
                .Setup(x => x.ComputeImageUri(It.IsAny<string>()))
                .Returns(new Uri("http://fake.local/000-000-001"));

            var entity = new TableEntity(partitionKey, rowKey)
            {
                ["Name"] = "test-name",
                ["Description"] = "description_test",
            };

            var modules = new List<IoTEdgeModule>()
            {
                new IoTEdgeModule
                {
                    ModuleName = "module"
                }
            };
            var commands = new List<EdgeModuleCommand>()
            {
                new EdgeModuleCommand
                {
                    PartitionKey = partitionKey,
                    Name = "Test",
                    RowKey = modules.First().ModuleName + "-" + "Test",
                }
            };

            // Act
            var result = edgeModelMapper.CreateEdgeDeviceModel(entity, modules, commands);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(rowKey, result.ModelId);
            Assert.AreEqual("test-name", result.Name);
            Assert.AreEqual(1, result.EdgeModules.Count);
            Assert.AreEqual(1, result.EdgeModules.First().Commands.Count);

            this.mockRepository.VerifyAll();
        }

        [Test]
        public void UpdateTableEntityShouldUpdateTemplate()
        {
            // Arrange
            var edgeModelMapper = CreateEdgeModelMapper();

            var entity = new TableEntity();
            var edgeModel = new IoTEdgeModel()
            {
                ModelId = Guid.NewGuid().ToString(),
                Name = "test",
                Description = "description test"
            };

            // Act
            edgeModelMapper.UpdateTableEntity(entity, edgeModel);

            // Assert
            Assert.AreEqual("test", entity["Name"]);
            Assert.AreEqual("description test", entity["Description"]);
        }
    }
}
