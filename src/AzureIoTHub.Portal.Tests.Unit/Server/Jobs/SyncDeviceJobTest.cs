// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace AzureIoTHub.Portal.Tests.Unit.Server.Jobs
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using AutoMapper;
    using AzureIoTHub.Portal.Domain;
    using AzureIoTHub.Portal.Domain.Entities;
    using AzureIoTHub.Portal.Domain.Repositories;
    using AzureIoTHub.Portal.Server.Jobs;
    using AzureIoTHub.Portal.Server.Services;
    using Microsoft.Azure.Devices.Shared;
    using Microsoft.Extensions.Logging;
    using Moq;
    using NUnit.Framework;
    using Quartz;

    public class SyncDeviceJobTest : IDisposable
    {
        private MockRepository mockRepository;
        private SyncDevicesJob syncDevicesJob;

        private Mock<ILogger<SyncDevicesJob>> mockLogger;
        private Mock<IDeviceService> mockDeviceService;
        private Mock<IDeviceModelRepository> mockDeviceModelRepository;
        private Mock<ILorawanDeviceRepository> mockLorawanDeviceRepository;
        private Mock<IDeviceRepository> mockDeviceRepository;
        private Mock<IUnitOfWork> mockUnitOfWork;
        private Mock<IMapper> mockMapper;

        [SetUp]
        public void SetUp()
        {
            this.mockRepository = new MockRepository(MockBehavior.Strict);

            this.mockLogger = this.mockRepository.Create<ILogger<SyncDevicesJob>>();
            this.mockDeviceService = this.mockRepository.Create<IDeviceService>();
            this.mockDeviceModelRepository = this.mockRepository.Create<IDeviceModelRepository>();
            this.mockLorawanDeviceRepository = this.mockRepository.Create<ILorawanDeviceRepository>();
            this.mockDeviceRepository = this.mockRepository.Create<IDeviceRepository>();
            this.mockUnitOfWork = this.mockRepository.Create<IUnitOfWork>();
            this.mockMapper = this.mockRepository.Create<IMapper>();

            this.syncDevicesJob =
                new SyncDevicesJob(
                this.mockDeviceService.Object,
                this.mockDeviceModelRepository.Object,
                this.mockLorawanDeviceRepository.Object,
                this.mockDeviceRepository.Object,
                this.mockMapper.Object,
                this.mockUnitOfWork.Object,
                this.mockLogger.Object);
        }

        [Test]
        public async Task ExecuteShould()
        {
            // Arrange
            var mockJobExecutionContext = this.mockRepository.Create<IJobExecutionContext>();
            var twinCollection = new TwinCollection();

            _ = this.mockDeviceService
                .Setup(x => x.GetAllDevice(
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.Is<string>(x => x == "LoRa Concentrator"),
                    It.IsAny<string>(),
                    It.IsAny<bool?>(),
                    It.IsAny<bool?>(),
                    It.IsAny<Dictionary<string, string>>(),
                    It.Is<int>(x => x == 100)))
                .ReturnsAsync(new PaginationResult<Twin>
                {
                    Items = Enumerable.Range(0, 100).Select(x => new Twin
                    {
                        DeviceId = FormattableString.Invariant($"{x}"),
                        Tags = twinCollection
                    }),
                    TotalItems = 1000,
                    NextPage = Guid.NewGuid().ToString()
                });


            _ = this.mockDeviceModelRepository
                .Setup(x => x.GetByIdAsync(It.IsAny<string>()))
                .ReturnsAsync(new Portal.Domain.Entities.DeviceModel()
                {
                    Id = Guid.NewGuid().ToString(),
                    SupportLoRaFeatures = true
                });

            // Act
            await this.syncDevicesJob.Execute(mockJobExecutionContext.Object);

            // Assert
            this.mockRepository.VerifyAll();
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
        }
    }
}
