// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace AzureIoTHub.Portal.Tests.Unit.Domain.Services
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net;
    using System.Net.Http;
    using System.Threading;
    using System.Threading.Tasks;
    using AutoFixture;
    using Azure;
    using Azure.Data.Tables;
    using AzureIoTHub.Portal.Domain;
    using AzureIoTHub.Portal.Domain.Exceptions;
    using AzureIoTHub.Portal.Server.Services;
    using FluentAssertions;
    using Microsoft.Azure.Devices.Shared;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.DependencyInjection;
    using Models.v10;
    using Models.v10.LoRaWAN;
    using Moq;
    using NUnit.Framework;
    using Portal.Domain.Entities;
    using Portal.Domain.Repositories;
    using Portal.Server.Managers;
    using Portal.Server.Mappers;
    using UnitTests.Bases;

    [TestFixture]
    public class LoRaWANCommandServiceTests : BackendUnitTest
    {
        private Mock<IDeviceModelRepository> mockDeviceModelRepository;
        private Mock<IDeviceModelCommandRepository> mockDeviceModelCommandRepository;
        private Mock<IUnitOfWork> mockUnitOfWork;
        private Mock<IExternalDeviceService> mockDeviceService;
        private Mock<IDeviceTagService> mockDeviceTagService;
        private Mock<IDeviceTwinMapper<DeviceListItem, LoRaDeviceDetails>> mockDeviceTwinMapper;
        private Mock<ITableClientFactory> mockTableClientFactory;
        private Mock<ILoraDeviceMethodManager> mockLoraDeviceMethodManager;
        private Mock<IDeviceModelCommandMapper> mockDeviceModelCommandMapper;
        private Mock<TableClient> mockCommandsTableClient;

        private ILoRaWANCommandService loRaWanCommandService;

        public override void Setup()
        {
            base.Setup();

            this.mockDeviceModelRepository = MockRepository.Create<IDeviceModelRepository>();
            this.mockDeviceModelCommandRepository = MockRepository.Create<IDeviceModelCommandRepository>();
            this.mockUnitOfWork = MockRepository.Create<IUnitOfWork>();
            this.mockDeviceService = MockRepository.Create<IExternalDeviceService>();
            this.mockDeviceTagService = MockRepository.Create<IDeviceTagService>();
            this.mockDeviceTwinMapper = MockRepository.Create<IDeviceTwinMapper<DeviceListItem, LoRaDeviceDetails>>();
            this.mockTableClientFactory = MockRepository.Create<ITableClientFactory>();
            this.mockLoraDeviceMethodManager = MockRepository.Create<ILoraDeviceMethodManager>();
            this.mockDeviceModelCommandMapper = MockRepository.Create<IDeviceModelCommandMapper>();
            this.mockCommandsTableClient = MockRepository.Create<TableClient>();

            _ = ServiceCollection.AddSingleton(this.mockDeviceModelRepository.Object);
            _ = ServiceCollection.AddSingleton(this.mockDeviceModelCommandRepository.Object);
            _ = ServiceCollection.AddSingleton(this.mockUnitOfWork.Object);


            _ = ServiceCollection.AddSingleton(this.mockDeviceService.Object);
            _ = ServiceCollection.AddSingleton(this.mockDeviceTagService.Object);
            _ = ServiceCollection.AddSingleton(this.mockDeviceTwinMapper.Object);
            _ = ServiceCollection.AddSingleton(this.mockTableClientFactory.Object);
            _ = ServiceCollection.AddSingleton(this.mockLoraDeviceMethodManager.Object);
            _ = ServiceCollection.AddSingleton(this.mockDeviceModelCommandMapper.Object);
            _ = ServiceCollection.AddSingleton(this.mockCommandsTableClient.Object);

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

        [Test]
        public async Task ExecuteCommandStateUnderTestExpectedBehavior()
        {
            // Arrange
            var modelId = Guid.NewGuid().ToString();
            var deviceId = Guid.NewGuid().ToString();
            var commandId = Guid.NewGuid().ToString();

            var mockResponse = MockRepository.Create<Response>();
            _ = this.mockDeviceModelCommandMapper
                .Setup(c => c.GetDeviceModelCommand(It.Is<TableEntity>(x => x.RowKey == commandId && x.PartitionKey == modelId)))
                .Returns(new DeviceModelCommandDto
                {
                    Name = commandId,
                    Frame = Guid.NewGuid().ToString(),
                    Port = 125
                });

            _ = this.mockCommandsTableClient.Setup(c => c.Query<TableEntity>(
                    It.Is<string>(x => x == $"RowKey eq '{commandId}' and PartitionKey eq '{modelId}'"),
                    It.IsAny<int?>(),
                    It.IsAny<IEnumerable<string>>(),
                    It.IsAny<CancellationToken>()))
                    .Returns(Pageable<TableEntity>.FromPages(new[]
                    {
                                    Page<TableEntity>.FromValues(new[]
                                    {
                                        new TableEntity(modelId, commandId)
                                    }, null, mockResponse.Object)
                    }));

            _ = this.mockTableClientFactory.Setup(c => c.GetDeviceCommands())
                .Returns(this.mockCommandsTableClient.Object);

            using var success = new HttpResponseMessage(HttpStatusCode.Accepted);

            _ = this.mockLoraDeviceMethodManager.Setup(c => c.ExecuteLoRaDeviceMessage(
                It.Is<string>(x => x == deviceId),
                It.Is<DeviceModelCommandDto>(x => x.Name == commandId)))
                .ReturnsAsync(success);

            _ = this.mockDeviceService.Setup(c => c.GetDeviceTwin(It.Is<string>(x => x == deviceId)))
                    .ReturnsAsync(new Twin()
                    {
                        DeviceId = deviceId,
                        ModelId = modelId,
                    });

            _ = this.mockDeviceTwinMapper.Setup(c => c.CreateDeviceDetails(It.IsAny<Twin>(), It.IsAny<IEnumerable<string>>()))
                    .Returns<Twin, IEnumerable<string>>((_, _) => new LoRaDeviceDetails
                    {
                        DeviceID = deviceId,
                        ModelId = modelId,
                    });

            // Act
            await this.loRaWanCommandService.ExecuteLoRaWANCommand(deviceId, commandId);

            // Assert
            MockRepository.VerifyAll();
        }

        [Test]
        public async Task ExecuteCommandShouldThrowInternalServerErrorExceptionWheIssueOccursWhenQueryingCommands()
        {
            // Arrange
            var modelId = Guid.NewGuid().ToString();
            var deviceId = Guid.NewGuid().ToString();
            var commandId = Guid.NewGuid().ToString();

            _ = this.mockCommandsTableClient.Setup(c => c.Query<TableEntity>(
                    It.Is<string>(x => x == $"RowKey eq '{commandId}' and PartitionKey eq '{modelId}'"),
                    It.IsAny<int?>(),
                    It.IsAny<IEnumerable<string>>(),
                    It.IsAny<CancellationToken>()))
                    .Throws(new RequestFailedException("test"));

            _ = this.mockTableClientFactory.Setup(c => c.GetDeviceCommands())
                .Returns(this.mockCommandsTableClient.Object);

            _ = this.mockDeviceService.Setup(c => c.GetDeviceTwin(It.Is<string>(x => x == deviceId)))
                    .ReturnsAsync(new Twin()
                    {
                        DeviceId = deviceId,
                        ModelId = modelId,
                    });

            _ = this.mockDeviceTwinMapper.Setup(c => c.CreateDeviceDetails(It.IsAny<Twin>(), It.IsAny<IEnumerable<string>>()))
                    .Returns<Twin, IEnumerable<string>>((_, _) => new LoRaDeviceDetails
                    {
                        DeviceID = deviceId,
                        ModelId = modelId,
                    });

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
            var modelId = Guid.NewGuid().ToString();
            var deviceId = Guid.NewGuid().ToString();
            var commandId = Guid.NewGuid().ToString();

            var mockResponse = MockRepository.Create<Response>();

            _ = this.mockCommandsTableClient.Setup(c => c.Query<TableEntity>(
                    It.Is<string>(x => x == $"RowKey eq '{commandId}' and PartitionKey eq '{modelId}'"),
                    It.IsAny<int?>(),
                    It.IsAny<IEnumerable<string>>(),
                    It.IsAny<CancellationToken>()))
                .Returns(Pageable<TableEntity>.FromPages(new[]
                {
                    Page<TableEntity>.FromValues(Array.Empty<TableEntity>(), null, mockResponse.Object)
                }));

            _ = this.mockTableClientFactory.Setup(c => c.GetDeviceCommands())
                .Returns(this.mockCommandsTableClient.Object);

            _ = this.mockDeviceService.Setup(c => c.GetDeviceTwin(It.Is<string>(x => x == deviceId)))
                .ReturnsAsync(new Twin()
                {
                    DeviceId = deviceId,
                    ModelId = modelId,
                });

            _ = this.mockDeviceTwinMapper.Setup(c => c.CreateDeviceDetails(It.IsAny<Twin>(), It.IsAny<IEnumerable<string>>()))
                .Returns<Twin, IEnumerable<string>>((_, _) => new LoRaDeviceDetails
                {
                    DeviceID = deviceId,
                    ModelId = modelId,
                });

            // Act
            _ = Assert.ThrowsAsync<ResourceNotFoundException>(async () => await this.loRaWanCommandService.ExecuteLoRaWANCommand(deviceId, commandId));

            // Assert
            MockRepository.VerifyAll();
        }

        [Test]
        public void ExecuteCommandFailedShouldThrowInternalServerErrorException()
        {
            // Arrange
            var modelId = Guid.NewGuid().ToString();
            var deviceId = Guid.NewGuid().ToString();
            var commandId = Guid.NewGuid().ToString();

            var mockResponse = MockRepository.Create<Response>();
            _ = this.mockDeviceModelCommandMapper
                .Setup(c => c.GetDeviceModelCommand(It.Is<TableEntity>(x => x.RowKey == commandId && x.PartitionKey == modelId)))
                .Returns(new DeviceModelCommandDto
                {
                    Name = commandId,
                    Frame = Guid.NewGuid().ToString(),
                    Port = 125
                });

            _ = this.mockCommandsTableClient.Setup(c => c.Query<TableEntity>(
                    It.Is<string>(x => x == $"RowKey eq '{commandId}' and PartitionKey eq '{modelId}'"),
                    It.IsAny<int?>(),
                    It.IsAny<IEnumerable<string>>(),
                    It.IsAny<CancellationToken>()))
                    .Returns(Pageable<TableEntity>.FromPages(new[]
                    {
                                    Page<TableEntity>.FromValues(new[]
                                    {
                                        new TableEntity(modelId, commandId)
                                    }, null, mockResponse.Object)
                    }));

            _ = this.mockTableClientFactory.Setup(c => c.GetDeviceCommands())
                .Returns(this.mockCommandsTableClient.Object);

            using var internalServerError = new HttpResponseMessage(HttpStatusCode.InternalServerError);

            _ = this.mockLoraDeviceMethodManager.Setup(c => c.ExecuteLoRaDeviceMessage(
                It.Is<string>(x => x == deviceId),
                It.Is<DeviceModelCommandDto>(x => x.Name == commandId)))
                .ReturnsAsync(internalServerError);

            _ = this.mockDeviceService.Setup(c => c.GetDeviceTwin(It.Is<string>(x => x == deviceId)))
                .ReturnsAsync(new Twin()
                {
                    DeviceId = deviceId,
                    ModelId = modelId,
                });

            _ = this.mockDeviceTwinMapper.Setup(c => c.CreateDeviceDetails(It.IsAny<Twin>(), It.IsAny<IEnumerable<string>>()))
                .Returns<Twin, IEnumerable<string>>((_, _) => new LoRaDeviceDetails
                {
                    DeviceID = deviceId,
                    ModelId = modelId,
                });

            // Assert
            _ = Assert.ThrowsAsync<InternalServerErrorException>(async () => await this.loRaWanCommandService.ExecuteLoRaWANCommand(deviceId, commandId));

            MockRepository.VerifyAll();
        }
    }
}
