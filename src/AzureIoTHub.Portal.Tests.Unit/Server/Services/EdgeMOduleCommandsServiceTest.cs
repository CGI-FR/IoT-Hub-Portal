// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace AzureIoTHub.Portal.Tests.Unit.Server.Services
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net.Http;
    using System.Net;
    using System.Threading.Tasks;
    using AutoMapper;
    using AzureIoTHub.Portal.Domain;
    using AzureIoTHub.Portal.Domain.Entities;
    using AzureIoTHub.Portal.Domain.Repositories;
    using AzureIoTHub.Portal.Models.v10;
    using AzureIoTHub.Portal.Server.Managers;
    using AzureIoTHub.Portal.Server.Services;
    using AzureIoTHub.Portal.Shared.Models.v1._0.IoTEdgeModuleCommand;
    using AzureIoTHub.Portal.Tests.Unit.UnitTests.Bases;
    using Microsoft.Extensions.DependencyInjection;
    using Moq;
    using NUnit.Framework;

    [TestFixture]
    public class EdgeModuleCommandsServiceTest : BackendUnitTest
    {
        private Mock<IUnitOfWork> mockUnitOfWork;
        private Mock<IEdgeModuleCommandMethodManager> mockEdgeModuleCommandMethodManager;
        private Mock<IEdgeModuleCommandsRepository> mockEdgeModuleCommandsRepository;

        private IEdgeModuleCommandsService edgeModuleCommandsService;

        [SetUp]
        public void SetUp()
        {
            base.Setup();

            this.mockUnitOfWork = MockRepository.Create<IUnitOfWork>();
            this.mockEdgeModuleCommandMethodManager = MockRepository.Create<IEdgeModuleCommandMethodManager>();
            this.mockEdgeModuleCommandsRepository = MockRepository.Create<IEdgeModuleCommandsRepository>();

            _ = ServiceCollection.AddSingleton(this.mockUnitOfWork.Object);
            _ = ServiceCollection.AddSingleton(this.mockEdgeModuleCommandMethodManager.Object);
            _ = ServiceCollection.AddSingleton(this.mockEdgeModuleCommandsRepository.Object);
            _ = ServiceCollection.AddSingleton<IEdgeModuleCommandsService, EdgeModuleCommandsService>();

            Services = ServiceCollection.BuildServiceProvider();

            this.edgeModuleCommandsService = Services.GetRequiredService<IEdgeModuleCommandsService>();
            Mapper = Services.GetRequiredService<IMapper>();
        }

        [Test]
        public async Task GetAllEdgeModuleCommandsShouldReturnValues()
        {
            // Arrange
            var edgeModelId = Guid.NewGuid().ToString();
            var expectedCommands = new List<EdgeModuleCommand>()
            {
                new EdgeModuleCommand{ Id = Guid.NewGuid().ToString() , EdgeModelId = edgeModelId},
                new EdgeModuleCommand{ Id = Guid.NewGuid().ToString() , EdgeModelId = edgeModelId},
                new EdgeModuleCommand{ Id = Guid.NewGuid().ToString() , EdgeModelId = Guid.NewGuid().ToString()},
                new EdgeModuleCommand{ Id = Guid.NewGuid().ToString() , EdgeModelId = Guid.NewGuid().ToString()},
            };

            _ = this.mockEdgeModuleCommandsRepository
                .Setup(x => x.GetAll())
                .Returns(expectedCommands);

            // Act
            var result = await this.edgeModuleCommandsService.GetAllEdgeModuleCommands(edgeModelId);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(2, result.Count());

            MockRepository.VerifyAll();
        }

        [Test]
        public async Task SaveEdgeModuleCommandAsyncShouldSuccessfullySaveCommand()
        {
            // Arrange
            var edgeModelId = Guid.NewGuid().ToString();
            var moduleName = Guid.NewGuid().ToString();

            var existingCommands = new List<EdgeModuleCommand>()
            {
                new EdgeModuleCommand{ Id = Guid.NewGuid().ToString() , EdgeModelId = edgeModelId},
                new EdgeModuleCommand{ Id = Guid.NewGuid().ToString() , EdgeModelId = edgeModelId},
                new EdgeModuleCommand{ Id = Guid.NewGuid().ToString() , EdgeModelId = Guid.NewGuid().ToString()},
                new EdgeModuleCommand{ Id = Guid.NewGuid().ToString() , EdgeModelId = Guid.NewGuid().ToString()},
            };

            _ = this.mockEdgeModuleCommandsRepository
                .Setup(x => x.GetAll())
                .Returns(existingCommands);

            _ = this.mockEdgeModuleCommandsRepository
                .Setup(x => x.Delete(It.IsAny<string>()));

            var edgeModuleList = new List<IoTEdgeModule>()
            {
                new IoTEdgeModule
                {
                    ModuleName= moduleName,
                    Commands = new List<EdgeModuleCommandDto>()
                    {
                        new EdgeModuleCommandDto
                        {
                            Id = Guid.NewGuid().ToString() ,
                            Name = Guid.NewGuid().ToString(),
                            DisplayName = Guid.NewGuid().ToString(),
                            Type = "Command",
                        }
                    },
                    ImageURI= Guid.NewGuid().ToString(),
                }
            };

            _ = this.mockEdgeModuleCommandsRepository
                .Setup(x => x.InsertAsync(It.IsAny<EdgeModuleCommand>()))
                .Returns(Task.CompletedTask);

            _ = this.mockUnitOfWork.Setup(x => x.SaveAsync()).Returns(Task.CompletedTask);

            // Act
            await this.edgeModuleCommandsService.SaveEdgeModuleCommandAsync(edgeModelId, edgeModuleList);

            // Assert

            MockRepository.VerifyAll();
        }

        [Test]
        public void DeleteEdgeModuleCommandAsyncShouldDeleteSuccessFully()
        {
            // Arrange
            var edgeModelId = Guid.NewGuid().ToString();
            var command = new EdgeModuleCommand
            {
                Id = Guid.NewGuid().ToString(),
                EdgeModelId = edgeModelId
            };

            _ = this.mockEdgeModuleCommandsRepository
                .Setup(x => x.Delete(It.Is<string>(c => c.Equals(command.Id, StringComparison.Ordinal))));

            // Act
            this.edgeModuleCommandsService.DeleteEdgeModuleCommandAsync(command.Id);

            // Assert
            MockRepository.VerifyAll();
        }

        [Test]
        public async Task ExecuteModuleCommandShouldNotFail()
        {
            // Arrange
            var deviceId = Guid.NewGuid().ToString();
            var commandEntity =  new EdgeModuleCommand
            {
                Id = Guid.NewGuid().ToString(),
                Name= "Test",
                EdgeModelId= Guid.NewGuid().ToString(),
                DisplayName= "Test",
                EdgeModuleName = "moduleTest"
            };

            _ = this.mockEdgeModuleCommandsRepository
                .Setup(x => x.GetByIdAsync(It.Is<string>(c => c.Equals(commandEntity.Id, StringComparison.Ordinal))))
                .ReturnsAsync(commandEntity);

            using var success = new HttpResponseMessage(HttpStatusCode.Accepted);

            _ = this.mockEdgeModuleCommandMethodManager
                .Setup(x => x.ExecuteEdgeModuleCommandMessage(It.IsAny<string>(), It.IsAny<EdgeModuleCommandDto>()))
                .ReturnsAsync(success);

            // Act
            await this.edgeModuleCommandsService.ExecuteModuleCommand(deviceId, commandEntity.Id);

            // Assert
            MockRepository.VerifyAll();
        }
    }
}
