// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace IoTHub.Portal.Tests.Unit.Server.Services
{
    using System;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Threading.Tasks;
    using AutoFixture;
    using AutoMapper;
    using IoTHub.Portal.Application.Mappers;
    using IoTHub.Portal.Application.Services;
    using IoTHub.Portal.Domain;
    using IoTHub.Portal.Domain.Entities;
    using IoTHub.Portal.Domain.Exceptions;
    using IoTHub.Portal.Domain.Repositories;
    using IoTHub.Portal.Infrastructure.Repositories;
    using IoTHub.Portal.Server.Services;
    using IoTHub.Portal.Shared.Models.v1._0;
    using IoTHub.Portal.Tests.Unit.UnitTests.Bases;
    using EntityFramework.Exceptions.Common;
    using FluentAssertions;
    using Microsoft.Azure.Devices;
    using Microsoft.Azure.Devices.Shared;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Logging;
    using Moq;
    using NUnit.Framework;
    using Shared.Models.v1._0.Filters;
    using Shared.Models.v1._0.LoRaWAN;
    using Device = Microsoft.Azure.Devices.Device;

    [TestFixture]
    public class LoRaWANConcentratorServiceTests : BackendUnitTest
    {
        private Mock<ILogger<LoRaWANConcentratorService>> mockLogger;
        private Mock<IExternalDeviceService> mockExternalDeviceService;
        private Mock<ILoRaWanManagementService> mockLloRaWanManagementService;
        private Mock<IConcentratorTwinMapper> mockConcentratorTwinMapper;
        private Mock<IConcentratorRepository> mockConcentratorRepository;
        private Mock<IUnitOfWork> mockUnitOfWork;

        private ILoRaWANConcentratorService concentratorService;

        [SetUp]
        public void SetUp()
        {
            base.Setup();

            this.mockLogger = this.MockRepository.Create<ILogger<LoRaWANConcentratorService>>();
            this.mockExternalDeviceService = this.MockRepository.Create<IExternalDeviceService>();
            this.mockLloRaWanManagementService = this.MockRepository.Create<ILoRaWanManagementService>();
            this.mockConcentratorTwinMapper = this.MockRepository.Create<IConcentratorTwinMapper>();
            this.mockConcentratorRepository = this.MockRepository.Create<IConcentratorRepository>();
            this.mockUnitOfWork = this.MockRepository.Create<IUnitOfWork>();

            _ = ServiceCollection.AddSingleton(this.mockLogger.Object);
            _ = ServiceCollection.AddSingleton(this.mockExternalDeviceService.Object);
            _ = ServiceCollection.AddSingleton(this.mockLloRaWanManagementService.Object);
            _ = ServiceCollection.AddSingleton(this.mockConcentratorTwinMapper.Object);
            _ = ServiceCollection.AddSingleton(this.mockConcentratorRepository.Object);
            _ = ServiceCollection.AddSingleton(this.mockUnitOfWork.Object);
            _ = ServiceCollection.AddSingleton(DbContext);
            _ = ServiceCollection.AddSingleton<ILoRaWANConcentratorService, LoRaWANConcentratorService>();

            Services = ServiceCollection.BuildServiceProvider();
            this.concentratorService = Services.GetRequiredService<ILoRaWANConcentratorService>();
            Mapper = Services.GetRequiredService<IMapper>();
        }

        [Test]
        public async Task GetAllDeviceConcentratorDefaultParametersShouldReturnConcentratorsList()
        {
            // Arrange
            var expectedTotalDevicesCount = 50;
            var expectedPageSize = 10;
            var expectedCurrentPage = 0;
            var expectedDevices = Fixture.CreateMany<Concentrator>(expectedTotalDevicesCount).ToList();

            await DbContext.AddRangeAsync(expectedDevices);
            _ = await DbContext.SaveChangesAsync();

            _ = this.mockConcentratorRepository.Setup(x => x.GetPaginatedListAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string[]>(), It.IsAny<Expression<Func<Concentrator, bool>>>(), default))
                .ReturnsAsync(new PaginatedResult<Concentrator>
                {
                    Data = expectedDevices.Skip(expectedCurrentPage * expectedPageSize).Take(expectedPageSize).ToList(),
                    PageSize = expectedPageSize,
                    CurrentPage = expectedCurrentPage,
                    TotalCount = expectedTotalDevicesCount
                });

            // Act
            var result = await this.concentratorService.GetAllDeviceConcentrator(new ConcentratorFilter());

            // Assert
            Assert.IsAssignableFrom<PaginatedResult<ConcentratorDto>>(result);
            _ = result.Data.Count.Should().Be(expectedPageSize);
            _ = result.TotalCount.Should().Be(expectedTotalDevicesCount);
            _ = result.PageSize.Should().Be(expectedPageSize);
            _ = result.CurrentPage.Should().Be(expectedCurrentPage);
            MockRepository.VerifyAll();
        }

        [Test]
        public async Task GetAllDeviceConcentratorWithPredicateShouldReturnConcentratorsList()
        {
            // Arrange
            var expectedTotalDevicesCount = 50;
            var expectedPageSize = 10;
            var expectedCurrentPage = 0;
            var expectedDevices = Fixture.CreateMany<Concentrator>(expectedTotalDevicesCount).ToList();

            await DbContext.AddRangeAsync(expectedDevices);
            _ = await DbContext.SaveChangesAsync();

            _ = this.mockConcentratorRepository.Setup(x => x.GetPaginatedListAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string[]>(), It.IsAny<Expression<Func<Concentrator, bool>>>(), default))
                .ReturnsAsync(new PaginatedResult<Concentrator>
                {
                    Data = expectedDevices.Skip(expectedCurrentPage * expectedPageSize).Take(expectedPageSize).ToList(),
                    PageSize = expectedPageSize,
                    CurrentPage = expectedCurrentPage,
                    TotalCount = expectedTotalDevicesCount
                });

            var concentratorFilter = new ConcentratorFilter
            {
                SearchText = "keyword",
                Status = true,
                State = false
            };

            // Act
            var result = await this.concentratorService.GetAllDeviceConcentrator(concentratorFilter);

            // Assert
            Assert.IsAssignableFrom<PaginatedResult<ConcentratorDto>>(result);
            _ = result.Data.Count.Should().Be(expectedPageSize);
            _ = result.TotalCount.Should().Be(expectedTotalDevicesCount);
            _ = result.PageSize.Should().Be(expectedPageSize);
            _ = result.CurrentPage.Should().Be(expectedCurrentPage);
            MockRepository.VerifyAll();
        }

        [Test]
        public async Task GetConcentratorExpectedBehaviorShouldReturnConcentratorDto()
        {
            // Arrange
            var expectedConcentrator = Fixture.Create<Concentrator>();
            var expectedConcentratorDto = Mapper.Map<ConcentratorDto>(expectedConcentrator);

            _ = this.mockConcentratorRepository.Setup(repository => repository.GetByIdAsync(expectedConcentratorDto.DeviceId))
                .ReturnsAsync(expectedConcentrator);

            // Act
            var result = await this.concentratorService.GetConcentrator(expectedConcentratorDto.DeviceId);

            // Assert
            _ = result.Should().BeEquivalentTo(expectedConcentratorDto);
            MockRepository.VerifyAll();
        }

        [Test]
        public async Task GetConcentratorConcentratorNotFoundShouldThrowResourceNotFoundException()
        {
            // Arrange
            var deviceId = Fixture.Create<string>();

            _ = this.mockConcentratorRepository.Setup(repository => repository.GetByIdAsync(deviceId))
                .ReturnsAsync((Concentrator)null);

            // Act
            var act = () => this.concentratorService.GetConcentrator(deviceId);

            // Assert
            _ = await act.Should().ThrowAsync<ResourceNotFoundException>();
            MockRepository.VerifyAll();
        }

        [Test]
        public async Task CreateDeviceAsyncExpectedBehaviorShouldCreateConcentrator()
        {
            // Arrange
            var expectedConcentrator = Fixture.Create<Concentrator>();
            var expectedConcentratorDto = Mapper.Map<ConcentratorDto>(expectedConcentrator);

            _ = this.mockConcentratorRepository.Setup(repository => repository.InsertAsync(It.IsAny<Concentrator>()))
                .Returns(Task.CompletedTask);

            _ = this.mockUnitOfWork.Setup(work => work.SaveAsync())
                .Returns(Task.CompletedTask);

            _ = this.mockExternalDeviceService.Setup(service => service.CreateNewTwinFromDeviceId(It.IsAny<string>()))
                .ReturnsAsync(new Twin { DeviceId = expectedConcentratorDto.DeviceId });

            _ = this.mockLloRaWanManagementService.Setup(manager => manager.GetRouterConfig(It.IsAny<string>()))
                .ReturnsAsync(new RouterConfig());

            this.mockConcentratorTwinMapper.Setup(mapper => mapper.UpdateTwin(It.IsAny<Twin>(), It.IsAny<ConcentratorDto>()))
                .Verifiable();

            _ = this.mockExternalDeviceService.Setup(service => service.CreateDeviceWithTwin(It.IsAny<string>(), false, It.IsAny<Twin>(), It.IsAny<DeviceStatus>()))
                .ReturnsAsync(new BulkRegistryOperationResult());

            // Act
            var result = await this.concentratorService.CreateDeviceAsync(expectedConcentratorDto);

            // Assert
            _ = result.Should().BeEquivalentTo(expectedConcentratorDto);
            MockRepository.VerifyAll();
        }

        [Test]
        public async Task CreateDeviceAsyncDbUpdateExceptionShouldThrowMaxLengthExceededException()
        {
            // Arrange
            var expectedConcentrator = Fixture.Create<Concentrator>();
            var expectedConcentratorDto = Mapper.Map<ConcentratorDto>(expectedConcentrator);

            _ = this.mockConcentratorRepository.Setup(repository => repository.InsertAsync(It.IsAny<Concentrator>()))
                .Returns(Task.CompletedTask);

            _ = this.mockUnitOfWork.Setup(work => work.SaveAsync())
                .ThrowsAsync(new MaxLengthExceededException());

            _ = this.mockExternalDeviceService.Setup(service => service.CreateNewTwinFromDeviceId(It.IsAny<string>()))
                .ReturnsAsync(new Twin { DeviceId = expectedConcentratorDto.DeviceId });

            _ = this.mockLloRaWanManagementService.Setup(manager => manager.GetRouterConfig(It.IsAny<string>()))
                .ReturnsAsync(new RouterConfig());

            this.mockConcentratorTwinMapper.Setup(mapper => mapper.UpdateTwin(It.IsAny<Twin>(), It.IsAny<ConcentratorDto>()))
                .Verifiable();

            _ = this.mockExternalDeviceService.Setup(service => service.CreateDeviceWithTwin(It.IsAny<string>(), false, It.IsAny<Twin>(), It.IsAny<DeviceStatus>()))
                .ReturnsAsync(new BulkRegistryOperationResult());

            // Act
            var act = () => this.concentratorService.CreateDeviceAsync(expectedConcentratorDto);

            // Assert
            _ = await act.Should().ThrowAsync<MaxLengthExceededException>();
            MockRepository.VerifyAll();
        }

        [Test]
        public async Task UpdateDeviceAsyncExpectedBehaviorShouldUpdateConcentrator()
        {
            // Arrange
            var expectedConcentrator = Fixture.Create<Concentrator>();
            var expectedConcentratorDto = Mapper.Map<ConcentratorDto>(expectedConcentrator);

            _ = this.mockConcentratorRepository.Setup(repository => repository.GetByIdAsync(It.IsAny<string>()))
                .ReturnsAsync(expectedConcentrator);

            this.mockConcentratorRepository.Setup(repository => repository.Update(It.IsAny<Concentrator>()))
                .Verifiable();

            _ = this.mockUnitOfWork.Setup(work => work.SaveAsync())
                .Returns(Task.CompletedTask);

            _ = this.mockExternalDeviceService.Setup(service => service.GetDevice(It.IsAny<string>()))
                .ReturnsAsync(new Device());

            _ = this.mockExternalDeviceService.Setup(service => service.UpdateDevice(It.IsAny<Device>()))
                .ReturnsAsync(new Device());

            _ = this.mockExternalDeviceService.Setup(service => service.GetDeviceTwin(It.IsAny<string>()))
                .ReturnsAsync(new Twin());

            _ = this.mockLloRaWanManagementService.Setup(manager => manager.GetRouterConfig(It.IsAny<string>()))
                .ReturnsAsync(new RouterConfig());

            this.mockConcentratorTwinMapper.Setup(mapper => mapper.UpdateTwin(It.IsAny<Twin>(), It.IsAny<ConcentratorDto>()))
                .Verifiable();

            _ = this.mockExternalDeviceService.Setup(service => service.UpdateDeviceTwin(It.IsAny<Twin>()))
                .ReturnsAsync(new Twin());

            // Act
            var result = await this.concentratorService.UpdateDeviceAsync(expectedConcentratorDto);

            // Assert
            _ = result.Should().BeEquivalentTo(expectedConcentratorDto);
            MockRepository.VerifyAll();
        }

        [Test]
        public async Task UpdateDeviceAsyncConcentratorNotFoundInDBShouldThrowResourceNotFoundException()
        {
            // Arrange
            var expectedConcentrator = Fixture.Create<Concentrator>();
            var expectedConcentratorDto = Mapper.Map<ConcentratorDto>(expectedConcentrator);

            _ = this.mockConcentratorRepository.Setup(repository => repository.GetByIdAsync(It.IsAny<string>()))
                .ReturnsAsync((Concentrator)null);

            _ = this.mockExternalDeviceService.Setup(service => service.GetDevice(It.IsAny<string>()))
                .ReturnsAsync(new Device());

            _ = this.mockExternalDeviceService.Setup(service => service.UpdateDevice(It.IsAny<Device>()))
                .ReturnsAsync(new Device());

            _ = this.mockExternalDeviceService.Setup(service => service.GetDeviceTwin(It.IsAny<string>()))
                .ReturnsAsync(new Twin());

            _ = this.mockLloRaWanManagementService.Setup(manager => manager.GetRouterConfig(It.IsAny<string>()))
                .ReturnsAsync(new RouterConfig());

            this.mockConcentratorTwinMapper.Setup(mapper => mapper.UpdateTwin(It.IsAny<Twin>(), It.IsAny<ConcentratorDto>()))
                .Verifiable();

            _ = this.mockExternalDeviceService.Setup(service => service.UpdateDeviceTwin(It.IsAny<Twin>()))
                .ReturnsAsync(new Twin());

            // Act
            var act = () => this.concentratorService.UpdateDeviceAsync(expectedConcentratorDto);

            // Assert
            _ = await act.Should().ThrowAsync<ResourceNotFoundException>();
            MockRepository.VerifyAll();
        }

        [Test]
        public async Task UpdateDeviceAsyncDbUpdateExceptionShouldThrowUniqueConstraintException()
        {
            // Arrange
            var expectedConcentrator = Fixture.Create<Concentrator>();
            var expectedConcentratorDto = Mapper.Map<ConcentratorDto>(expectedConcentrator);

            _ = this.mockConcentratorRepository.Setup(repository => repository.GetByIdAsync(It.IsAny<string>()))
                .ReturnsAsync(expectedConcentrator);

            this.mockConcentratorRepository.Setup(repository => repository.Update(It.IsAny<Concentrator>()))
                .Verifiable();

            _ = this.mockUnitOfWork.Setup(work => work.SaveAsync())
                .ThrowsAsync(new UniqueConstraintException());

            _ = this.mockExternalDeviceService.Setup(service => service.GetDevice(It.IsAny<string>()))
                .ReturnsAsync(new Device());

            _ = this.mockExternalDeviceService.Setup(service => service.UpdateDevice(It.IsAny<Device>()))
                .ReturnsAsync(new Device());

            _ = this.mockExternalDeviceService.Setup(service => service.GetDeviceTwin(It.IsAny<string>()))
                .ReturnsAsync(new Twin());

            _ = this.mockLloRaWanManagementService.Setup(manager => manager.GetRouterConfig(It.IsAny<string>()))
                .ReturnsAsync(new RouterConfig());

            this.mockConcentratorTwinMapper.Setup(mapper => mapper.UpdateTwin(It.IsAny<Twin>(), It.IsAny<ConcentratorDto>()))
                .Verifiable();

            _ = this.mockExternalDeviceService.Setup(service => service.UpdateDeviceTwin(It.IsAny<Twin>()))
                .ReturnsAsync(new Twin());

            // Act
            var act = () => this.concentratorService.UpdateDeviceAsync(expectedConcentratorDto);

            // Assert
            _ = await act.Should().ThrowAsync<UniqueConstraintException>();
            MockRepository.VerifyAll();
        }

        [Test]
        public async Task DeleteDeviceAsyncExpectedBehaviorShouldDeleteConcentrator()
        {
            // Arrange
            var expectedConcentrator = Fixture.Create<Concentrator>();
            var expectedConcentratorDto = Mapper.Map<ConcentratorDto>(expectedConcentrator);

            _ = this.mockConcentratorRepository.Setup(repository => repository.GetByIdAsync(It.IsAny<string>()))
                .ReturnsAsync(expectedConcentrator);

            this.mockConcentratorRepository.Setup(repository => repository.Delete(It.IsAny<string>()))
                .Verifiable();

            _ = this.mockUnitOfWork.Setup(work => work.SaveAsync())
                .Returns(Task.CompletedTask);

            _ = this.mockExternalDeviceService.Setup(service => service.DeleteDevice(It.IsAny<string>()))
                .Returns(Task.CompletedTask);

            // Act
            await this.concentratorService.DeleteDeviceAsync(expectedConcentratorDto.DeviceId);

            // Assert
            MockRepository.VerifyAll();
        }

        [Test]
        public async Task DeleteDeviceAsyncConcentratorNotFoundInDBShouldReturn()
        {
            // Arrange
            var expectedConcentrator = Fixture.Create<Concentrator>();
            var expectedConcentratorDto = Mapper.Map<ConcentratorDto>(expectedConcentrator);

            _ = this.mockConcentratorRepository.Setup(repository => repository.GetByIdAsync(It.IsAny<string>()))
                .ReturnsAsync((Concentrator)null);

            _ = this.mockExternalDeviceService.Setup(service => service.DeleteDevice(It.IsAny<string>()))
                .Returns(Task.CompletedTask);

            // Act
            await this.concentratorService.DeleteDeviceAsync(expectedConcentratorDto.DeviceId);

            // Assert
            MockRepository.VerifyAll();
        }

        [Test]
        public async Task DeleteDeviceAsyncDbUpdateExceptionShouldThrowReferenceConstraintException()
        {
            // Arrange
            var expectedConcentrator = Fixture.Create<Concentrator>();
            var expectedConcentratorDto = Mapper.Map<ConcentratorDto>(expectedConcentrator);

            _ = this.mockConcentratorRepository.Setup(repository => repository.GetByIdAsync(It.IsAny<string>()))
                .ReturnsAsync(expectedConcentrator);

            this.mockConcentratorRepository.Setup(repository => repository.Delete(It.IsAny<string>()))
                .Verifiable();

            _ = this.mockUnitOfWork.Setup(work => work.SaveAsync())
                .ThrowsAsync(new ReferenceConstraintException());

            _ = this.mockExternalDeviceService.Setup(service => service.DeleteDevice(It.IsAny<string>()))
                .Returns(Task.CompletedTask);

            // Act
            var act = () => this.concentratorService.DeleteDeviceAsync(expectedConcentratorDto.DeviceId);

            // Assert
            _ = await act.Should().ThrowAsync<ReferenceConstraintException>();
            MockRepository.VerifyAll();
        }
    }
}
