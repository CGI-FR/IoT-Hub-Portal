// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace IoTHub.Portal.Tests.Unit.Infrastructure.Jobs
{
    using System;
    using System.Collections.Generic;
    using System.Linq.Expressions;
    using System.Threading;
    using System.Threading.Tasks;
    using AutoFixture;
    using IoTHub.Portal.Application.Services;
    using IoTHub.Portal.Domain;
    using IoTHub.Portal.Domain.Repositories;
    using IoTHub.Portal.Infrastructure.Jobs;
    using Microsoft.Azure.Devices.Shared;
    using Microsoft.Extensions.DependencyInjection;
    using Moq;
    using NUnit.Framework;
    using Portal.Domain.Entities;
    using Quartz;
    using Shared;
    using UnitTests.Bases;

    public class SyncConcentratorsTests : BackendUnitTest
    {
        private IJob syncConcentratorsJob;

        private Mock<IExternalDeviceService> mockExternalDeviceService;
        private Mock<IConcentratorRepository> mockConcentratorRepository;
        private Mock<IUnitOfWork> mockUnitOfWork;

        public override void Setup()
        {
            base.Setup();

            this.mockExternalDeviceService = MockRepository.Create<IExternalDeviceService>();
            this.mockConcentratorRepository = MockRepository.Create<IConcentratorRepository>();
            this.mockUnitOfWork = MockRepository.Create<IUnitOfWork>();

            _ = ServiceCollection.AddSingleton(this.mockExternalDeviceService.Object);
            _ = ServiceCollection.AddSingleton(this.mockConcentratorRepository.Object);
            _ = ServiceCollection.AddSingleton(this.mockUnitOfWork.Object);
            _ = ServiceCollection.AddSingleton<IJob, SyncConcentratorsJob>();

            Services = ServiceCollection.BuildServiceProvider();

            this.syncConcentratorsJob = Services.GetRequiredService<IJob>();
        }

        [Test]
        public async Task ExecuteNewConcentratorShouldCreateEntity()
        {
            // Arrange
            var mockJobExecutionContext = MockRepository.Create<IJobExecutionContext>();

            var expectedTwinConcentrator = new Twin
            {
                DeviceId = Fixture.Create<string>(),
                Tags = new TwinCollection
                {
                    ["deviceName"] = Fixture.Create<string>(),
                    ["loraRegion"] = Fixture.Create<string>(),
                    ["deviceType"] = Fixture.Create<string>(),
                    ["clientThumbprint"] = Fixture.Create<string>()
                }
            };

            _ = this.mockExternalDeviceService
                .Setup(x => x.GetAllDevice(
                    It.IsAny<string>(),
                    It.Is<string>(x => x == "LoRa Concentrator"),
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<bool?>(),
                    It.IsAny<bool?>(),
                    It.IsAny<Dictionary<string, string>>(),
                    It.Is<int>(x => x == 100)))
                .ReturnsAsync(new PaginationResult<Twin>
                {
                    Items = new List<Twin>
                    {
                        expectedTwinConcentrator
                    },
                    TotalItems = 1
                });

            _ = this.mockConcentratorRepository.Setup(repository => repository.GetByIdAsync(expectedTwinConcentrator.DeviceId))
                .ReturnsAsync((Concentrator)null);

            _ = this.mockConcentratorRepository.Setup(repository => repository.InsertAsync(It.IsAny<Concentrator>()))
                .Returns(Task.CompletedTask);

            _ = this.mockConcentratorRepository.Setup(x => x.GetAllAsync(It.IsAny<Expression<Func<Concentrator, bool>>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<Concentrator>
                {
                    new Concentrator
                    {
                        Id = expectedTwinConcentrator.DeviceId
                    },
                    new Concentrator
                    {
                        Id = Guid.NewGuid().ToString()
                    }
                });

            this.mockConcentratorRepository.Setup(x => x.Delete(It.IsAny<string>())).Verifiable();

            _ = this.mockUnitOfWork.Setup(work => work.SaveAsync())
                .Returns(Task.CompletedTask);

            // Act
            await this.syncConcentratorsJob.Execute(mockJobExecutionContext.Object);

            // Assert
            MockRepository.VerifyAll();
        }

        [Test]
        public async Task ExecuteExistingConcentratorWithGreaterVersionShouldUpdateEntity()
        {
            // Arrange
            var mockJobExecutionContext = MockRepository.Create<IJobExecutionContext>();

            var expectedTwinConcentrator = new Twin
            {
                DeviceId = Fixture.Create<string>(),
                Tags = new TwinCollection
                {
                    ["deviceName"] = Fixture.Create<string>(),
                    ["loraRegion"] = Fixture.Create<string>(),
                    ["deviceType"] = Fixture.Create<string>()
                },
                Version = 2
            };

            var existingConcentrator = new Concentrator
            {
                Id = expectedTwinConcentrator.DeviceId,
                Version = 1,
            };

            _ = this.mockExternalDeviceService
                .Setup(x => x.GetAllDevice(
                    It.IsAny<string>(),
                    It.Is<string>(x => x == "LoRa Concentrator"),
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<bool?>(),
                    It.IsAny<bool?>(),
                    It.IsAny<Dictionary<string, string>>(),
                    It.Is<int>(x => x == 100)))
                .ReturnsAsync(new PaginationResult<Twin>
                {
                    Items = new List<Twin>
                    {
                        expectedTwinConcentrator
                    },
                    TotalItems = 1
                });

            _ = this.mockConcentratorRepository.Setup(repository => repository.GetByIdAsync(expectedTwinConcentrator.DeviceId))
                .ReturnsAsync(existingConcentrator);

            this.mockConcentratorRepository.Setup(repository => repository.Update(It.IsAny<Concentrator>()))
                .Verifiable();

            _ = this.mockConcentratorRepository.Setup(x => x.GetAllAsync(It.IsAny<Expression<Func<Concentrator, bool>>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<Concentrator>
                {
                    new Concentrator
                    {
                        Id = expectedTwinConcentrator.DeviceId
                    }
                });

            _ = this.mockUnitOfWork.Setup(work => work.SaveAsync())
                .Returns(Task.CompletedTask);

            // Act
            await this.syncConcentratorsJob.Execute(mockJobExecutionContext.Object);

            // Assert
            MockRepository.VerifyAll();
        }

        [Test]
        public async Task ExecuteExistingConcentratorWithLowerOrEqualVersionShouldReturn()
        {
            // Arrange
            var mockJobExecutionContext = MockRepository.Create<IJobExecutionContext>();

            var expectedTwinConcentrator = new Twin
            {
                DeviceId = Fixture.Create<string>(),
                Tags = new TwinCollection
                {
                    ["deviceName"] = Fixture.Create<string>(),
                    ["loraRegion"] = Fixture.Create<string>(),
                    ["deviceType"] = Fixture.Create<string>()
                },
                Version = 1
            };

            var existingConcentrator = new Concentrator
            {
                Id = expectedTwinConcentrator.DeviceId,
                Version = 1,
            };

            _ = this.mockExternalDeviceService
                .Setup(x => x.GetAllDevice(
                    It.IsAny<string>(),
                    It.Is<string>(x => x == "LoRa Concentrator"),
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<bool?>(),
                    It.IsAny<bool?>(),
                    It.IsAny<Dictionary<string, string>>(),
                    It.Is<int>(x => x == 100)))
                .ReturnsAsync(new PaginationResult<Twin>
                {
                    Items = new List<Twin>
                    {
                        expectedTwinConcentrator
                    },
                    TotalItems = 1
                });

            _ = this.mockConcentratorRepository.Setup(repository => repository.GetByIdAsync(expectedTwinConcentrator.DeviceId))
                .ReturnsAsync(existingConcentrator);

            _ = this.mockConcentratorRepository.Setup(x => x.GetAllAsync(It.IsAny<Expression<Func<Concentrator, bool>>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<Concentrator>
                {
                    new Concentrator
                    {
                        Id = expectedTwinConcentrator.DeviceId
                    }
                });

            _ = this.mockUnitOfWork.Setup(work => work.SaveAsync())
                .Returns(Task.CompletedTask);

            // Act
            await this.syncConcentratorsJob.Execute(mockJobExecutionContext.Object);

            // Assert
            MockRepository.VerifyAll();
        }
    }
}
