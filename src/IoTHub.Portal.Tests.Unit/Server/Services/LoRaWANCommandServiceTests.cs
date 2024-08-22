// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace IoTHub.Portal.Tests.Unit.Server.Services
{
    using System;
    using System.Linq;
    using System.Net;
    using System.Net.Http;
    using System.Threading.Tasks;
    using AutoFixture;
    using AutoMapper;
    using EntityFramework.Exceptions.Common;
    using FluentAssertions;
    using IoTHub.Portal.Application.Services;
    using IoTHub.Portal.Domain;
    using IoTHub.Portal.Domain.Entities;
    using IoTHub.Portal.Domain.Exceptions;
    using IoTHub.Portal.Domain.Repositories;
    using IoTHub.Portal.Server.Services;
    using IoTHub.Portal.Tests.Unit.UnitTests.Bases;
    using Microsoft.Extensions.DependencyInjection;
    using Moq;
    using NUnit.Framework;
    using Shared.Models.v1._0.LoRaWAN;

    [TestFixture]
    public class LoRaWANCommandServiceTests : BackendUnitTest
    {
        private Mock<IDeviceModelRepository> mockDeviceModelRepository;
        private Mock<IDeviceModelCommandRepository> mockDeviceModelCommandRepository;
        private Mock<IUnitOfWork> mockUnitOfWork;
        private Mock<ILoRaWanManagementService> loRaWanManagementService;

        private ILoRaWANCommandService loRaWanCommandService;

        public override void Setup()
        {
            base.Setup();

            this.mockDeviceModelRepository = MockRepository.Create<IDeviceModelRepository>();
            this.mockDeviceModelCommandRepository = MockRepository.Create<IDeviceModelCommandRepository>();
            this.mockUnitOfWork = MockRepository.Create<IUnitOfWork>();
            this.loRaWanManagementService = MockRepository.Create<ILoRaWanManagementService>();

            _ = ServiceCollection.AddSingleton(this.mockDeviceModelRepository.Object);
            _ = ServiceCollection.AddSingleton(this.mockDeviceModelCommandRepository.Object);
            _ = ServiceCollection.AddSingleton(this.mockUnitOfWork.Object);

            _ = ServiceCollection.AddSingleton(this.loRaWanManagementService.Object);

            _ = ServiceCollection.AddSingleton<ILoRaWANCommandService, LoRaWANCommandService>();

            Services = ServiceCollection.BuildServiceProvider();

            this.loRaWanCommandService = Services.GetRequiredService<ILoRaWANCommandService>();
            Mapper = Services.GetRequiredService<IMapper>();
        }

        [Test]
        public async Task PostShouldCreateCommandShouldDeleteExistingModelCommandsAndCreateNewModelCommands()
        {
            // Arrange
            var deviceModel = Fixture.Create<DeviceModel>();
            var existingCommands = Fixture.CreateMany<DeviceModelCommand>(2)
                .Select(command =>
                {
                    command.DeviceModelId = deviceModel.Id;
                    return command;
                })
                .ToList();

            var newCommands = Fixture.CreateMany<DeviceModelCommandDto>(5);

            _ = this.mockDeviceModelRepository.Setup(repository => repository.GetByIdAsync(deviceModel.Id))
                .ReturnsAsync(deviceModel);

            _ = this.mockDeviceModelCommandRepository.Setup(repository => repository.GetAll())
                .Returns(existingCommands);

            this.mockDeviceModelCommandRepository
                .Setup(repository => repository.Delete(It.IsAny<string>()))
                .Verifiable();

            _ = this.mockDeviceModelCommandRepository
                .Setup(repository => repository.InsertAsync(It.IsAny<DeviceModelCommand>()))
                .Returns(Task.CompletedTask);

            _ = this.mockUnitOfWork.Setup(work => work.SaveAsync())
                .Returns(Task.CompletedTask);

            // Act
            await this.loRaWanCommandService.PostDeviceModelCommands(deviceModel.Id, newCommands.ToArray());

            // Assert
            MockRepository.VerifyAll();
        }

        [Test]
        public void PostShouldCreateCommandShouldThrowAResourceNotFoundExceptionWhenModelentityIsNull()
        {
            // Arrange
            var deviceModel = Fixture.Create<DeviceModel>();

            var newCommands = Fixture.CreateMany<DeviceModelCommandDto>(5);

            _ = this.mockDeviceModelRepository.Setup(repository => repository.GetByIdAsync(deviceModel.Id))
                .ReturnsAsync(value: null);

            // Act
            var act = async () => await this.loRaWanCommandService.PostDeviceModelCommands(deviceModel.Id, newCommands.ToArray());

            // Assert
            _ = act.Should().ThrowAsync<ResourceNotFoundException>();
            MockRepository.VerifyAll();
        }

        [Test]
        public async Task PostShouldCreateCommandShouldThrowUniqueConstraintExceptionWhenDDbUpdateExceptionIsThrown()
        {
            // Arrange
            var deviceModel = Fixture.Create<DeviceModel>();
            var commands = Fixture.CreateMany<DeviceModelCommandDto>(5);

            _ = this.mockDeviceModelRepository.Setup(repository => repository.GetByIdAsync(deviceModel.Id))
                .ReturnsAsync(deviceModel);

            _ = this.mockDeviceModelCommandRepository.Setup(repository => repository.GetAll())
                .Returns(Array.Empty<DeviceModelCommand>());

            _ = this.mockDeviceModelCommandRepository
                .Setup(repository => repository.InsertAsync(It.IsAny<DeviceModelCommand>()))
                .Returns(Task.CompletedTask);

            _ = this.mockUnitOfWork.Setup(work => work.SaveAsync())
                .ThrowsAsync(new UniqueConstraintException());

            // Act
            var act = () =>  this.loRaWanCommandService.PostDeviceModelCommands(deviceModel.Id, commands.ToArray());

            // Assert
            _ = await act.Should().ThrowAsync<UniqueConstraintException>();
            MockRepository.VerifyAll();
        }

        [Test]
        public async Task GetShouldReturnDeviceModelCommands()
        {
            // Arrange
            var deviceModel = Fixture.Create<DeviceModel>();
            var commands = Fixture.CreateMany<DeviceModelCommand>(5).Select(command =>
            {
                command.DeviceModelId = deviceModel.Id;
                return command;
            }) .ToList();

            var expectedCommands = commands.Select(command => Mapper.Map<DeviceModelCommandDto>(command)).ToArray();

            _ = this.mockDeviceModelCommandRepository.Setup(repository => repository.GetAll())
                .Returns(commands);

            // Act
            var result = await this.loRaWanCommandService.GetDeviceModelCommandsFromModel(deviceModel.Id);

            // Assert
            _ = result.Should().BeEquivalentTo(expectedCommands);
            MockRepository.VerifyAll();
        }

        [Test]
        public async Task ExecuteCommandStateUnderTestExpectedBehavior()
        {
            // Arrange
            var deviceId = Guid.NewGuid().ToString();
            var commandId = Guid.NewGuid().ToString();

            _ = this.mockDeviceModelCommandRepository.Setup(c => c.GetByIdAsync(commandId))
                .ReturnsAsync(new DeviceModelCommand
                {
                    Id = commandId,
                    Name = Guid.NewGuid().ToString(),
                    Frame = Guid.NewGuid().ToString(),
                    Port = 125
                });

            using var success = new HttpResponseMessage(HttpStatusCode.Accepted);

            _ = this.loRaWanManagementService.Setup(c => c.ExecuteLoRaDeviceMessage(
                It.Is<string>(x => x == deviceId),
                It.Is<DeviceModelCommandDto>(x => x.Id == commandId)))
                .ReturnsAsync(success);

            // Act
            await this.loRaWanCommandService.ExecuteLoRaWANCommand(deviceId, commandId);

            // Assert
            MockRepository.VerifyAll();
        }

        [Test]
        public async Task ExecuteCommandShouldThrowInternalServerErrorExceptionWheIssueOccursWhenQueryingCommands()
        {
            // Arrange
            var deviceId = Guid.NewGuid().ToString();
            var commandId = Guid.NewGuid().ToString();

            _ = this.mockDeviceModelCommandRepository.Setup(c => c.GetByIdAsync(commandId))
                .ReturnsAsync(new DeviceModelCommand
                {
                    Id = commandId,
                    Name = Guid.NewGuid().ToString(),
                    Frame = Guid.NewGuid().ToString(),
                    Port = 125
                });

            using var response = new HttpResponseMessage(HttpStatusCode.InternalServerError);

            _ = this.loRaWanManagementService.Setup(c => c.ExecuteLoRaDeviceMessage(deviceId, It.IsAny<DeviceModelCommandDto>()))
                .ReturnsAsync(response);

            // Act
            var act = () => this.loRaWanCommandService.ExecuteLoRaWANCommand(deviceId, commandId);

            // Assert
            _ = await act.Should().ThrowAsync<InternalServerErrorException>();
            MockRepository.VerifyAll();
        }

        [Test]
        public void ExecuteCommandShouldReturnNotFoundWhenCommandIsNotFound()
        {
            // Arrange
            var deviceId = Guid.NewGuid().ToString();
            var commandId = Guid.NewGuid().ToString();

            _ = this.mockDeviceModelCommandRepository.Setup(c => c.GetByIdAsync(commandId))
                .ReturnsAsync((DeviceModelCommand)null);

            // Act
            _ = Assert.ThrowsAsync<ResourceNotFoundException>(async () => await this.loRaWanCommandService.ExecuteLoRaWANCommand(deviceId, commandId));

            // Assert
            MockRepository.VerifyAll();
        }

    }
}
