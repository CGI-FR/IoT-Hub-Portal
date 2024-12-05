// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace IoTHub.Portal.Tests.Unit.Infrastructure.Jobs
{
    [TestFixture]
    public class SendPlanningCommandJobTests : BackendUnitTest
    {
        private SendPlanningCommandJob sendPlanningCommandJob;

        private MockRepository mockRepository;
        private Mock<IDeviceService<DeviceDetails>> mockDeviceService;
        private Mock<ILayerService> mockLayerService;
        private Mock<IPlanningService> mockPlanningService;
        private Mock<IScheduleService> mockScheduleService;
        private Mock<ILoRaWANCommandService> mockLoraWANCommandService;
        private Mock<ILogger<SendPlanningCommandJob>> mockLogger;

        [SetUp]
        public void SetUp()
        {
            this.mockRepository = new MockRepository(MockBehavior.Strict);

            this.mockDeviceService = this.mockRepository.Create<IDeviceService<DeviceDetails>>();
            this.mockLayerService = this.mockRepository.Create<ILayerService>();

            this.mockPlanningService = this.MockRepository.Create<IPlanningService>();
            this.mockScheduleService = this.MockRepository.Create<IScheduleService>();
            this.mockLoraWANCommandService = this.MockRepository.Create<ILoRaWANCommandService>();

            this.mockLogger = this.mockRepository.Create<ILogger<SendPlanningCommandJob>>();

            this.sendPlanningCommandJob =
                new SendPlanningCommandJob(this.mockDeviceService.Object, this.mockLayerService.Object, this.mockPlanningService.Object,
                                           this.mockScheduleService.Object, this.mockLoraWANCommandService.Object, this.mockLogger.Object);
        }


        [Test]
        public async Task Execute_PlanningActive_ShouldSendCommandToDevice()
        {
            // Arrange
            var mockJobExecutionContext = MockRepository.Create<IJobExecutionContext>();

            _ = this.mockLogger.Setup(x => x.Log(It.IsAny<LogLevel>(), It.IsAny<EventId>(), It.IsAny<It.IsAnyType>(), It.IsAny<Exception>(), It.IsAny<Func<It.IsAnyType, Exception, string>>()));

            var commands = new List<DeviceModelCommandDto>
            {
                new DeviceModelCommandDto
                {
                    Id = Guid.NewGuid().ToString()
                }
            };

            var plannings = new List<PlanningDto>
            {
                new PlanningDto
                {
                    Id = Guid.NewGuid().ToString(),
                    Start = DateTime.Now.AddDays(-1).ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
                    End = DateTime.Now.AddDays(1).ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
                    DayOff = 0,
                    CommandId = commands.Single().Id
                }
            };

            var layers = new List<LayerDto>
            {
                new LayerDto
                {
                    Id = Guid.NewGuid().ToString(),
                    Planning = plannings.Single().Id
                }
            };

            var schedules = new List<ScheduleDto>
            {
                new ScheduleDto
                {
                    Id = Guid.NewGuid().ToString(),
                    Start = "00:00",
                    End = "00:00:00",
                    CommandId = commands.Single().Id,
                    PlanningId = plannings.Single().Id
                }
            };

            var device = new DeviceListItem
            {
                DeviceID = Guid.NewGuid().ToString(),
                LayerId = layers.Single().Id
            };

            var expectedPaginatedDevices = new PaginatedResult<DeviceListItem>()
            {
                Data = Enumerable.Range(0, 1).Select(x => device).ToList(),
                TotalCount = 1
            };

            _ = this.mockDeviceService.Setup(service => service.GetDevices(It.IsAny<string>(), It.IsAny<bool?>(),
                    It.IsAny<bool?>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string[]>(),
                    It.IsAny<Dictionary<string, string>>(), It.IsAny<string>(),
                    It.IsAny<List<string>>(), It.IsAny<string>()))
                .ReturnsAsync(expectedPaginatedDevices);

            _ = this.mockLayerService.Setup(x => x.GetLayers()).ReturnsAsync(layers);
            _ = this.mockPlanningService.Setup(x => x.GetPlannings()).ReturnsAsync(plannings);
            _ = this.mockScheduleService.Setup(x => x.GetSchedules()).ReturnsAsync(schedules);

            // Act
            await this.sendPlanningCommandJob.Execute(mockJobExecutionContext.Object);

            // Assert
            MockRepository.VerifyAll();
        }

        [Test]
        public async Task Execute_PlanningInactive_ShouldNotSendCommandToDevice()
        {
            // Arrange
            var mockJobExecutionContext = MockRepository.Create<IJobExecutionContext>();

            _ = this.mockLogger.Setup(x => x.Log(It.IsAny<LogLevel>(), It.IsAny<EventId>(), It.IsAny<It.IsAnyType>(), It.IsAny<Exception>(), It.IsAny<Func<It.IsAnyType, Exception, string>>()));

            var commands = new List<DeviceModelCommandDto>
            {
                new DeviceModelCommandDto
                {
                    Id = Guid.NewGuid().ToString()
                }
            };

            var plannings = new List<PlanningDto>
            {
                new PlanningDto
                {
                    Id = Guid.NewGuid().ToString(),
                    Start = DateTime.Now.AddDays(-10).ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
                    End = DateTime.Now.AddDays(-1).ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
                    DayOff = 0,
                    CommandId = commands.Single().Id
                }
            };

            var layers = new List<LayerDto>
            {
                new LayerDto
                {
                    Id = Guid.NewGuid().ToString(),
                    Planning = plannings.Single().Id
                }
            };

            var schedules = new List<ScheduleDto>
            {
                new ScheduleDto
                {
                    Id = Guid.NewGuid().ToString(),
                    Start = "00:00",
                    End = "00:00:00",
                    CommandId = commands.Single().Id,
                    PlanningId = plannings.Single().Id
                }
            };

            var device = new DeviceListItem
            {
                DeviceID = Guid.NewGuid().ToString(),
                LayerId = layers.Single().Id
            };

            var expectedPaginatedDevices = new PaginatedResult<DeviceListItem>()
            {
                Data = Enumerable.Range(0, 1).Select(x => device).ToList(),
                TotalCount = 1
            };

            _ = this.mockDeviceService.Setup(service => service.GetDevices(It.IsAny<string>(), It.IsAny<bool?>(),
                    It.IsAny<bool?>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string[]>(),
                    It.IsAny<Dictionary<string, string>>(), It.IsAny<string>(),
                    It.IsAny<List<string>>(), It.IsAny<string>()))
                .ReturnsAsync(expectedPaginatedDevices);

            _ = this.mockLayerService.Setup(x => x.GetLayers()).ReturnsAsync(layers);
            _ = this.mockPlanningService.Setup(x => x.GetPlannings()).ReturnsAsync(plannings);
            _ = this.mockScheduleService.Setup(x => x.GetSchedules()).ReturnsAsync(schedules);

            // Act
            await this.sendPlanningCommandJob.Execute(mockJobExecutionContext.Object);

            // Assert
            MockRepository.VerifyAll();
        }
    }
}
