// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace IoTHub.Portal.Tests.Unit.Infrastructure.Jobs
{
    using System;
    using System.Collections.Generic;
    using IoTHub.Portal.Application.Services;
    using IoTHub.Portal.Infrastructure.Jobs;
    using IoTHub.Portal.Shared.Models.v10;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Logging;
    using Moq;
    using NUnit.Framework;
    using Quartz;
    using UnitTests.Bases;

    public class SyncGatewayIDJobTests : BackendUnitTest
    {
        private IJob syncGatewayIDJob;
        private Mock<ILogger<SyncGatewayIDJob>> mockLogger;
        private Mock<IExternalDeviceService> mockExternalDeviceService;
        private LoRaGatewayIDList loRaGatewayIDList;

        public override void Setup()
        {
            base.Setup();

            this.mockExternalDeviceService = MockRepository.Create<IExternalDeviceService>();
            this.mockLogger = MockRepository.Create<ILogger<SyncGatewayIDJob>>();
            this.loRaGatewayIDList = new LoRaGatewayIDList();

            _ = ServiceCollection.AddSingleton(this.mockExternalDeviceService.Object);
            _ = ServiceCollection.AddSingleton<IJob, SyncGatewayIDJob>();

            Services = ServiceCollection.BuildServiceProvider();

            this.syncGatewayIDJob = new SyncGatewayIDJob(this.mockExternalDeviceService.Object, this.loRaGatewayIDList, this.mockLogger.Object);
        }

        [Test]
        public void ExecuteExpectedBehaviorShouldSynchronizeGatewayID()
        {
            // Arrange
            _ = this.mockLogger.Setup(x => x.Log(LogLevel.Information, It.IsAny<EventId>(), It.IsAny<It.IsAnyType>(), It.IsAny<Exception>(), It.IsAny<Func<It.IsAnyType, Exception, string>>()));

            _ = this.mockExternalDeviceService.Setup(service => service.GetAllGatewayID())
                    .ReturnsAsync(new List<string>() { "GatewayID01", "GatewayID02" });

            // Act
            _ = this.syncGatewayIDJob.Execute(null);

            // Assert
            Assert.AreEqual(2, this.loRaGatewayIDList.GatewayIds.Count);
            Assert.AreEqual("GatewayID01", this.loRaGatewayIDList.GatewayIds[0]);
            MockRepository.VerifyAll();
        }

        [Test]
        public void ExecuteExceptionThrownShouldRecordLogError()
        {
            // Arrange
            _ = this.mockLogger.Setup(x => x.Log(LogLevel.Information, It.IsAny<EventId>(), It.IsAny<It.IsAnyType>(), It.IsAny<Exception>(), It.IsAny<Func<It.IsAnyType, Exception, string>>()));
            _ = this.mockLogger.Setup(x => x.Log(LogLevel.Error, It.IsAny<EventId>(), It.IsAny<It.IsAnyType>(), It.IsAny<Exception>(), It.IsAny<Func<It.IsAnyType, Exception, string>>()));
            _ = this.mockExternalDeviceService.Setup(service => service.GetAllGatewayID())
                    .Throws(new Exception());

            // Act
            _ = this.syncGatewayIDJob.Execute(null);

            // Assert
            Assert.IsEmpty(this.loRaGatewayIDList.GatewayIds);
            MockRepository.VerifyAll();
        }
    }
}
