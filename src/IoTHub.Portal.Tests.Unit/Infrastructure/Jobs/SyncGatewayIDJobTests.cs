// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace IoTHub.Portal.Tests.Unit.Infrastructure.Jobs
{
    public class SyncGatewayIdJobTests : BackendUnitTest
    {
        private IJob syncGatewayIdJob;
        private Mock<ILogger<SyncGatewayIdJob>> mockLogger;
        private Mock<IExternalDeviceService> mockExternalDeviceService;
        private LoRaGatewayIdList loRaGatewayIdList;

        public override void Setup()
        {
            base.Setup();

            this.mockExternalDeviceService = MockRepository.Create<IExternalDeviceService>();
            this.mockLogger = MockRepository.Create<ILogger<SyncGatewayIdJob>>();
            this.loRaGatewayIdList = new LoRaGatewayIdList();

            _ = ServiceCollection.AddSingleton(this.mockExternalDeviceService.Object);
            _ = ServiceCollection.AddSingleton<IJob, SyncGatewayIdJob>();

            Services = ServiceCollection.BuildServiceProvider();

            this.syncGatewayIdJob = new SyncGatewayIdJob(this.mockExternalDeviceService.Object, this.loRaGatewayIdList, this.mockLogger.Object);
        }

        [Test]
        public void ExecuteExpectedBehaviorShouldSynchronizeGatewayID()
        {
            // Arrange
            _ = this.mockLogger.Setup(x => x.Log(LogLevel.Information, It.IsAny<EventId>(), It.IsAny<It.IsAnyType>(), It.IsAny<Exception>(), It.IsAny<Func<It.IsAnyType, Exception, string>>()));

            _ = this.mockExternalDeviceService.Setup(service => service.GetAllGatewayID())
                    .ReturnsAsync(new List<string>() { "GatewayID01", "GatewayID02" });

            // Act
            _ = this.syncGatewayIdJob.Execute(null);

            // Assert
            Assert.AreEqual(2, this.loRaGatewayIdList.GatewayIds.Count);
            Assert.AreEqual("GatewayID01", this.loRaGatewayIdList.GatewayIds[0]);
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
            _ = this.syncGatewayIdJob.Execute(null);

            // Assert
            Assert.IsEmpty(this.loRaGatewayIdList.GatewayIds);
            MockRepository.VerifyAll();
        }
    }
}
