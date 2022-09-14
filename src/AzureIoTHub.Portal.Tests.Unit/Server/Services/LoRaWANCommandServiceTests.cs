// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace AzureIoTHub.Portal.Tests.Unit.Domain.Services
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;
    using AutoFixture;
    using AzureIoTHub.Portal.Domain;
    using AzureIoTHub.Portal.Domain.Exceptions;
    using AzureIoTHub.Portal.Server.Services;
    using FluentAssertions;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.DependencyInjection;
    using Models.v10.LoRaWAN;
    using Moq;
    using NUnit.Framework;
    using Portal.Domain.Entities;
    using Portal.Domain.Repositories;
    using UnitTests.Bases;

    [TestFixture]
    public class LoRaWANCommandServiceTests : BackendUnitTest
    {
        private Mock<IDeviceModelRepository> mockDeviceModelRepository;
        private Mock<IDeviceModelCommandRepository> mockDeviceModelCommandRepository;
        private Mock<IUnitOfWork> mockUnitOfWork;

        private ILoRaWANCommandService loRaWanCommandService;

        public override void Setup()
        {
            base.Setup();

            this.mockDeviceModelRepository = MockRepository.Create<IDeviceModelRepository>();
            this.mockDeviceModelCommandRepository = MockRepository.Create<IDeviceModelCommandRepository>();
            this.mockUnitOfWork = MockRepository.Create<IUnitOfWork>();

            _ = ServiceCollection.AddSingleton(this.mockDeviceModelRepository.Object);
            _ = ServiceCollection.AddSingleton(this.mockDeviceModelCommandRepository.Object);
            _ = ServiceCollection.AddSingleton(this.mockUnitOfWork.Object);
            _ = ServiceCollection.AddSingleton<ILoRaWANCommandService, LoRaWANCommandService>();

            Services = ServiceCollection.BuildServiceProvider();

            this.loRaWanCommandService = Services.GetRequiredService<ILoRaWANCommandService>();
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
        public async Task PostShouldCreateCommandShouldThrowInternalServerErrorExceptionWhenDDbUpdateExceptionIsThrown()
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
                .ThrowsAsync(new DbUpdateException());

            // Act
            var act = () =>  this.loRaWanCommandService.PostDeviceModelCommands(deviceModel.Id, commands.ToArray());

            // Assert
            _ = await act.Should().ThrowAsync<InternalServerErrorException>();
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
    }
}
