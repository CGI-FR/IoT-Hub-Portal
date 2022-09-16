// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace AzureIoTHub.Portal.Tests.Unit.Server.Services
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using AzureIoTHub.Portal.Domain;
    using AzureIoTHub.Portal.Domain.Entities;
    using AzureIoTHub.Portal.Domain.Exceptions;
    using AzureIoTHub.Portal.Domain.Repositories;
    using AzureIoTHub.Portal.Server.Services;
    using Microsoft.EntityFrameworkCore;
    using Moq;
    using NUnit.Framework;

    [TestFixture]
    public class DeviceModelPropertiesServiceTests
    {
        private MockRepository mockRepository;
        private Mock<IDeviceModelPropertiesRepository> mockDeviceModelPropertiesRepository;
        private Mock<IDeviceModelRepository> mockDeviceModelRepository;
        private Mock<IUnitOfWork> mockUnitOfWork;

        [SetUp]
        public void SetUp()
        {
            this.mockRepository = new MockRepository(MockBehavior.Strict);
            this.mockDeviceModelPropertiesRepository = this.mockRepository.Create<IDeviceModelPropertiesRepository>();
            this.mockDeviceModelRepository = this.mockRepository.Create<IDeviceModelRepository>();
            this.mockUnitOfWork = this.mockRepository.Create<IUnitOfWork>();
        }

        [Test]
        public async Task GetPropertiesStateUnderTestExpectedBehavior()
        {
            // Arrange
            var instance = CreateDeviceModelPropertiesService();
            var entity = SetupMockEntity();

            _ = this.mockDeviceModelPropertiesRepository.Setup(c => c.GetModelProperties(entity.Id))
                    .ReturnsAsync(new[]
                        {
                            new DeviceModelProperty
                            {
                                Id = Guid.NewGuid().ToString(),
                                ModelId = entity.Id
                            }
                        });

            // Act
            var result = await instance.GetModelProperties(entity.Id);

            // Assert
            Assert.NotNull(result);
            Assert.AreEqual(1, result.Count());

            this.mockRepository.VerifyAll();
        }

        [Test]
        public void WhenDeviceModelNotExistsGetPropertiesShouldReturnHttp404()
        {
            // Arrange
            var instance = CreateDeviceModelPropertiesService();
            SetupNotFoundEntity();

            // Act
            _ = Assert.ThrowsAsync<ResourceNotFoundException>(async () => await instance.GetModelProperties(Guid.NewGuid().ToString()));
        }

        [Test]
        public void SavePropertiesForModelShouldThrowResourceNotFoundExceptionWhenDeviceModelNotExists()
        {
            // Arrange
            var instance = CreateDeviceModelPropertiesService();
            SetupNotFoundEntity();

            // Act
            _ = Assert.ThrowsAsync<ResourceNotFoundException>(async () => await instance.SavePropertiesForModel(Guid.NewGuid().ToString(), new List<DeviceModelProperty>()));
        }

        [Test]
        public async Task SetPropertiesStateUnderTestExpectedBehavior()
        {
            // Arrange
            var instance = CreateDeviceModelPropertiesService();
            var entity = SetupMockEntity();

            var properties = new[]
            {
                new DeviceModelProperty
                {
                    DisplayName =Guid.NewGuid().ToString(),
                    Name = Guid.NewGuid().ToString()
                }
            };

            _ = this.mockDeviceModelPropertiesRepository.Setup(c => c.SavePropertiesForModel(entity.Id, It.IsAny<IEnumerable<DeviceModelProperty>>()))
                .Returns(Task.CompletedTask);

            _ = this.mockUnitOfWork.Setup(c => c.SaveAsync())
                .Returns(Task.CompletedTask);

            // Act
            await instance.SavePropertiesForModel(entity.Id, properties);

            // Assert
            this.mockRepository.VerifyAll();
        }

        [Test]
        public void WhenExceptionOccuresSavePropertiesForModelShouldThrowInternalServerErrorException()
        {
            // Arrange
            var instance = CreateDeviceModelPropertiesService();

            var entity = SetupMockEntity();

            _ = this.mockDeviceModelPropertiesRepository.Setup(c => c.SavePropertiesForModel(entity.Id, It.IsAny<IEnumerable<DeviceModelProperty>>()))
                .Returns(Task.CompletedTask);

            _ = this.mockUnitOfWork.Setup(c => c.SaveAsync())
                .Throws<DbUpdateException>();

            // Act
            _ = Assert.ThrowsAsync<InternalServerErrorException>(async () => await instance.SavePropertiesForModel(entity.Id, Array.Empty<DeviceModelProperty>()));
        }

        private DeviceModel SetupMockEntity()
        {
            var modelId = Guid.NewGuid().ToString();
            var deviceModelEntity = new DeviceModel
            {
                Id = modelId
            };

            _ = this.mockDeviceModelRepository.Setup(repository => repository.GetByIdAsync(modelId))
                .ReturnsAsync(deviceModelEntity);

            return deviceModelEntity;
        }

        private void SetupNotFoundEntity()
        {
            _ = this.mockDeviceModelRepository.Setup(repository => repository.GetByIdAsync(It.IsAny<string>()))
                .ReturnsAsync((DeviceModel)null);
        }

        private DeviceModelPropertiesService CreateDeviceModelPropertiesService()
        {
            return new DeviceModelPropertiesService(
                this.mockUnitOfWork.Object,
                this.mockDeviceModelPropertiesRepository.Object,
                this.mockDeviceModelRepository.Object);
        }

    }
}
