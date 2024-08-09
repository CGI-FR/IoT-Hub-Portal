// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace IoTHub.Portal.Tests.Unit.Client.Components.Planning
{
    using AutoFixture;
    using Bunit;
    using System.Collections.Generic;
    using Bunit.TestDoubles;
    using IoTHub.Portal.Client.Models;
    using IoTHub.Portal.Client.Services;
    using IoTHub.Portal.Models.v10;
    using IoTHub.Portal.Shared.Models.v10;
    using IoTHub.Portal.Tests.Unit.UnitTests.Bases;
    using Microsoft.Extensions.DependencyInjection;
    using Moq;
    using NUnit.Framework;
    using IoTHub.Portal.Shared.Models.v10.Filters;
    using IoTHub.Portal.Client.Components.Planning;
    using System.Linq;
    using IoTHub.Portal.Models.v10.LoRaWAN;
    using IoTHub.Portal.Client.Exceptions;
    using MudBlazor;
    using System.Threading.Tasks;
    using FluentAssertions;
    using IoTHub.Portal.Shared.Constants;

    internal class EditPlanningTest : BlazorUnitTest
    {
        private Mock<IPlanningClientService> mockPlanningClientService;
        private Mock<IScheduleClientService> mockScheduleClientService;
        private Mock<ILayerClientService> mockLayerClientService;
        private Mock<IDeviceModelsClientService> mockDeviceModelsClientService;
        private Mock<ILoRaWanDeviceModelsClientService> mockLoRaWanDeviceModelsClientService;

        private FakeNavigationManager mockNavigationManager;

        public override void Setup()
        {
            base.Setup();

            this.mockScheduleClientService = MockRepository.Create<IScheduleClientService>();
            this.mockPlanningClientService = MockRepository.Create<IPlanningClientService>();
            this.mockLayerClientService = MockRepository.Create<ILayerClientService>();
            this.mockDeviceModelsClientService = MockRepository.Create<IDeviceModelsClientService>();
            this.mockLoRaWanDeviceModelsClientService = MockRepository.Create<ILoRaWanDeviceModelsClientService>();

            _ = Services.AddSingleton(this.mockScheduleClientService.Object);
            _ = Services.AddSingleton(this.mockPlanningClientService.Object);
            _ = Services.AddSingleton(this.mockLayerClientService.Object);
            _ = Services.AddSingleton(this.mockDeviceModelsClientService.Object);
            _ = Services.AddSingleton(this.mockLoRaWanDeviceModelsClientService.Object);
            _ = Services.AddSingleton(new PortalSettings { IsLoRaSupported = true });

            this.mockNavigationManager = Services.GetRequiredService<FakeNavigationManager>();
        }

        [Test]
        public void EditPlanningInit()
        {
            var expectedLayers = Fixture.CreateMany<DeviceModelDto>(1).ToList();
            var expectedDeviceModelCommandDto = Fixture.CreateMany<DeviceModelCommandDto>(3).ToList();

            PlanningDto Planning = new PlanningDto
            {
                DayOff = DaysEnumFlag.DaysOfWeek.Saturday | DaysEnumFlag.DaysOfWeek.Sunday,
                CommandId = expectedDeviceModelCommandDto[0].Id
            };
            ScheduleDto firstSchedule = new ScheduleDto
            {
                Start = "00:00"
            };

            List<ScheduleDto> ScheduleList = new List<ScheduleDto>
            {
                firstSchedule
            };

            _ = this.mockLayerClientService.Setup(service => service.GetLayers())
                .ReturnsAsync(new List<LayerDto>());

            _ = this.mockDeviceModelsClientService.Setup(service => service.GetDeviceModels(It.IsAny<DeviceModelFilter>()))
                .ReturnsAsync(new PaginationResult<DeviceModelDto>
                {
                    Items = expectedLayers
                });

            _ = this.mockLoRaWanDeviceModelsClientService.Setup(service => service.GetDeviceModelCommands(It.IsAny<string>()))
                .ReturnsAsync(expectedDeviceModelCommandDto);

            // Act
            var cut = RenderComponent<EditPlanning>(
                ComponentParameter.CreateParameter("mode", "New"),
                ComponentParameter.CreateParameter("planning", Planning),
                ComponentParameter.CreateParameter("scheduleList", ScheduleList )
            );

            Assert.AreEqual(cut.Instance.planning.DayOff, DaysEnumFlag.DaysOfWeek.Saturday | DaysEnumFlag.DaysOfWeek.Sunday);
            Assert.AreEqual(cut.Instance.scheduleList[0].Start, "00:00");
            cut.WaitForAssertion(() => MockRepository.VerifyAll());
        }

        [Test]
        public void EditPlanningInit_AddScheduleShouldNotWork()
        {
            var expectedLayers = Fixture.CreateMany<DeviceModelDto>(1).ToList();
            var expectedDeviceModelCommandDto = Fixture.CreateMany<DeviceModelCommandDto>(3).ToList();

            PlanningDto Planning = new PlanningDto
            {
                DayOff = DaysEnumFlag.DaysOfWeek.Saturday | DaysEnumFlag.DaysOfWeek.Sunday,
                CommandId = expectedDeviceModelCommandDto[0].Id
            };
            ScheduleDto firstSchedule = new ScheduleDto
            {
                Start = "00:00"
            };

            List<ScheduleDto> ScheduleList = new List<ScheduleDto>
            {
                firstSchedule
            };

            _ = this.mockLayerClientService.Setup(service => service.GetLayers())
                .ReturnsAsync(new List<LayerDto>());

            _ = this.mockDeviceModelsClientService.Setup(service => service.GetDeviceModels(It.IsAny<DeviceModelFilter>()))
                .ReturnsAsync(new PaginationResult<DeviceModelDto>
                {
                    Items = expectedLayers
                });

            _ = this.mockLoRaWanDeviceModelsClientService.Setup(service => service.GetDeviceModelCommands(It.IsAny<string>()))
                .ReturnsAsync(expectedDeviceModelCommandDto);

            // Act
            var cut = RenderComponent<EditPlanning>(
                ComponentParameter.CreateParameter("mode", "New"),
                ComponentParameter.CreateParameter("planning", Planning),
                ComponentParameter.CreateParameter("scheduleList", ScheduleList )
            );

            var editPlanningAddLayers = cut.WaitForElement("#editPlanningAddLayers");
            editPlanningAddLayers.Click();

            Assert.AreEqual(cut.Instance.scheduleList.Count, 1);
            cut.WaitForAssertion(() => MockRepository.VerifyAll());
        }

        [Test]
        public async Task EditPlanningInit_AddSchedule()
        {
            var expectedLayers = Fixture.CreateMany<DeviceModelDto>(1).ToList();
            var expectedDeviceModelCommandDto = Fixture.CreateMany<DeviceModelCommandDto>(3).ToList();

            PlanningDto Planning = new PlanningDto
            {
                DayOff = DaysEnumFlag.DaysOfWeek.Saturday | DaysEnumFlag.DaysOfWeek.Sunday,
                CommandId = expectedDeviceModelCommandDto[0].Id
            };
            ScheduleDto firstSchedule = new ScheduleDto
            {
                Start = "00:00",
                CommandId = expectedDeviceModelCommandDto[0].Id
            };

            List<ScheduleDto> ScheduleList = new List<ScheduleDto>
            {
                firstSchedule
            };

            _ = this.mockLayerClientService.Setup(service => service.GetLayers())
                .ReturnsAsync(new List<LayerDto>());

            _ = this.mockDeviceModelsClientService.Setup(service => service.GetDeviceModels(It.IsAny<DeviceModelFilter>()))
                .ReturnsAsync(new PaginationResult<DeviceModelDto>
                {
                    Items = expectedLayers
                });

            _ = this.mockLoRaWanDeviceModelsClientService.Setup(service => service.GetDeviceModelCommands(It.IsAny<string>()))
                .ReturnsAsync(expectedDeviceModelCommandDto);

            // Act
            var cut = RenderComponent<EditPlanning>(
                ComponentParameter.CreateParameter("mode", "New"),
                ComponentParameter.CreateParameter("planning", Planning),
                ComponentParameter.CreateParameter("scheduleList", ScheduleList )
            );

            var EndField = cut.FindComponents<MudTextField<string>>()[2];
            await cut.InvokeAsync(() => EndField.Instance.SetText("23:59"));

            var editPlanningAddLayers = cut.WaitForElement("#editPlanningAddLayers");
            editPlanningAddLayers.Click();

            Assert.AreEqual(cut.Instance.scheduleList.Count, 2);
            cut.WaitForAssertion(() => MockRepository.VerifyAll());
        }

        [Test]
        public async Task EditPlanningInit_DeleteSchedule()
        {
            var expectedLayers = Fixture.CreateMany<DeviceModelDto>(1).ToList();
            var expectedDeviceModelCommandDto = Fixture.CreateMany<DeviceModelCommandDto>(3).ToList();

            PlanningDto Planning = new PlanningDto
            {
                DayOff = DaysEnumFlag.DaysOfWeek.Saturday | DaysEnumFlag.DaysOfWeek.Sunday,
                CommandId = expectedDeviceModelCommandDto[0].Id
            };
            ScheduleDto firstSchedule = new ScheduleDto
            {
                Start = "00:00",
                CommandId = expectedDeviceModelCommandDto[0].Id
            };

            List<ScheduleDto> ScheduleList = new List<ScheduleDto>
            {
                firstSchedule
            };

            _ = this.mockLayerClientService.Setup(service => service.GetLayers())
                .ReturnsAsync(new List<LayerDto>());

            _ = this.mockDeviceModelsClientService.Setup(service => service.GetDeviceModels(It.IsAny<DeviceModelFilter>()))
                .ReturnsAsync(new PaginationResult<DeviceModelDto>
                {
                    Items = expectedLayers
                });

            _ = this.mockLoRaWanDeviceModelsClientService.Setup(service => service.GetDeviceModelCommands(It.IsAny<string>()))
                .ReturnsAsync(expectedDeviceModelCommandDto);

            // Act
            var cut = RenderComponent<EditPlanning>(
                ComponentParameter.CreateParameter("mode", "New"),
                ComponentParameter.CreateParameter("planning", Planning),
                ComponentParameter.CreateParameter("scheduleList", ScheduleList )
            );

            var EndField = cut.FindComponents<MudTextField<string>>()[2];
            await cut.InvokeAsync(() => EndField.Instance.SetText("23:59"));

            var editPlanningAddLayers = cut.WaitForElement("#editPlanningAddLayers");
            editPlanningAddLayers.Click();

            var editPlanningDeleteLayers = cut.FindAll("#editPlanningDeleteLayers")[1];
            editPlanningDeleteLayers.Click();

            Assert.AreEqual(cut.Instance.scheduleList.Count, 1);
            cut.WaitForAssertion(() => MockRepository.VerifyAll());
        }

        [Test]
        public void EditPlanningInit_ProblemDetailsException()
        {
            PlanningDto Planning = new PlanningDto
            {
                DayOff = DaysEnumFlag.DaysOfWeek.Saturday | DaysEnumFlag.DaysOfWeek.Sunday
            };
            ScheduleDto firstSchedule = new ScheduleDto
            {
                Start = "00:00",
            };

            List<ScheduleDto> ScheduleList = new List<ScheduleDto>
            {
                firstSchedule
            };

            _ = this.mockLayerClientService.Setup(service => service.GetLayers())
                .ReturnsAsync(new List<LayerDto>());

            _ = this.mockDeviceModelsClientService.Setup(service => service.GetDeviceModels(It.IsAny<DeviceModelFilter>()))
                .ThrowsAsync(new ProblemDetailsException(new ProblemDetailsWithExceptionDetails()));

            // Act
            var cut = RenderComponent<EditPlanning>(
                ComponentParameter.CreateParameter("mode", "New"),
                ComponentParameter.CreateParameter("planning", Planning),
                ComponentParameter.CreateParameter("scheduleList", ScheduleList )
            );

            cut.WaitForAssertion(() => MockRepository.VerifyAll());
        }

        [Test]
        public void EditPlanningInit_ChangeOnDay()
        {
            var expectedLayers = Fixture.CreateMany<DeviceModelDto>(1).ToList();
            var expectedDeviceModelCommandDto = Fixture.CreateMany<DeviceModelCommandDto>(3).ToList();

            PlanningDto Planning = new PlanningDto
            {
                DayOff = DaysEnumFlag.DaysOfWeek.Saturday | DaysEnumFlag.DaysOfWeek.Sunday,
                CommandId = expectedDeviceModelCommandDto[0].Id
            };
            ScheduleDto firstSchedule = new ScheduleDto
            {
                Start = "00:00"
            };

            List<ScheduleDto> ScheduleList = new List<ScheduleDto>
            {
                firstSchedule
            };

            _ = this.mockLayerClientService.Setup(service => service.GetLayers())
                .ReturnsAsync(new List<LayerDto>());

            _ = this.mockDeviceModelsClientService.Setup(service => service.GetDeviceModels(It.IsAny<DeviceModelFilter>()))
                .ReturnsAsync(new PaginationResult<DeviceModelDto>
                {
                    Items = expectedLayers
                });

            _ = this.mockLoRaWanDeviceModelsClientService.Setup(service => service.GetDeviceModelCommands(It.IsAny<string>()))
                .ReturnsAsync(expectedDeviceModelCommandDto);

            // Act
            var cut = RenderComponent<EditPlanning>(
                ComponentParameter.CreateParameter("mode", "New"),
                ComponentParameter.CreateParameter("planning", Planning),
                ComponentParameter.CreateParameter("scheduleList", ScheduleList )
            );

            var editPlanningChangeOnDayLayers = cut.FindAll("#editPlanningChangeOnDayLayers")[0];
            editPlanningChangeOnDayLayers.Click();

            Assert.AreEqual(cut.Instance.planning.DayOff, DaysEnumFlag.DaysOfWeek.Monday | DaysEnumFlag.DaysOfWeek.Saturday | DaysEnumFlag.DaysOfWeek.Sunday);
            cut.WaitForAssertion(() => MockRepository.VerifyAll());
        }

        [Test]
        public void EditPlanningInit_ChangeOffDay()
        {
            var expectedLayers = Fixture.CreateMany<DeviceModelDto>(1).ToList();
            var expectedDeviceModelCommandDto = Fixture.CreateMany<DeviceModelCommandDto>(3).ToList();

            PlanningDto Planning = new PlanningDto
            {
                DayOff = DaysEnumFlag.DaysOfWeek.Saturday | DaysEnumFlag.DaysOfWeek.Sunday,
                CommandId = expectedDeviceModelCommandDto[0].Id
            };
            ScheduleDto firstSchedule = new ScheduleDto
            {
                Start = "00:00"
            };

            List<ScheduleDto> ScheduleList = new List<ScheduleDto>
            {
                firstSchedule
            };

            _ = this.mockLayerClientService.Setup(service => service.GetLayers())
                .ReturnsAsync(new List<LayerDto>());

            _ = this.mockDeviceModelsClientService.Setup(service => service.GetDeviceModels(It.IsAny<DeviceModelFilter>()))
                .ReturnsAsync(new PaginationResult<DeviceModelDto>
                {
                    Items = expectedLayers
                });

            _ = this.mockLoRaWanDeviceModelsClientService.Setup(service => service.GetDeviceModelCommands(It.IsAny<string>()))
                .ReturnsAsync(expectedDeviceModelCommandDto);

            // Act
            var cut = RenderComponent<EditPlanning>(
                ComponentParameter.CreateParameter("mode", "New"),
                ComponentParameter.CreateParameter("planning", Planning),
                ComponentParameter.CreateParameter("scheduleList", ScheduleList )
            );

            var editPlanningChangeOffDayLayers = cut.FindAll("#editPlanningChangeOffDayLayers")[5];
            editPlanningChangeOffDayLayers.Click();

            Assert.AreEqual(cut.Instance.planning.DayOff, DaysEnumFlag.DaysOfWeek.Sunday);
            cut.WaitForAssertion(() => MockRepository.VerifyAll());
        }

        [Test]
        public void EditPlanningInit_SaveNewLayers()
        {
            var expectedId = Fixture.Create<string>();

            var expectedLayers = Fixture.CreateMany<DeviceModelDto>(1).ToList();
            var expectedDeviceModelCommandDto = Fixture.CreateMany<DeviceModelCommandDto>(3).ToList();

            PlanningDto Planning = new PlanningDto
            {
                DayOff = DaysEnumFlag.DaysOfWeek.Saturday | DaysEnumFlag.DaysOfWeek.Sunday,
                CommandId = expectedDeviceModelCommandDto[0].Id
            };
            ScheduleDto firstSchedule = new ScheduleDto
            {
                Start = "00:00"
            };

            List<ScheduleDto> ScheduleList = new List<ScheduleDto>
            {
                firstSchedule
            };

            _ = this.mockLayerClientService.Setup(service => service.GetLayers())
                .ReturnsAsync(new List<LayerDto>());

            _ = this.mockScheduleClientService.Setup(service => service.CreateSchedule(It.IsAny<ScheduleDto>()))
                .ReturnsAsync(expectedId);

            _ = this.mockPlanningClientService.Setup(service => service.CreatePlanning(It.IsAny<PlanningDto>()))
                .ReturnsAsync(expectedId);

            _ = this.mockDeviceModelsClientService.Setup(service => service.GetDeviceModels(It.IsAny<DeviceModelFilter>()))
                .ReturnsAsync(new PaginationResult<DeviceModelDto>
                {
                    Items = expectedLayers
                });

            _ = this.mockLoRaWanDeviceModelsClientService.Setup(service => service.GetDeviceModelCommands(It.IsAny<string>()))
                .ReturnsAsync(expectedDeviceModelCommandDto);

            // Act
            var cut = RenderComponent<EditPlanning>(
                ComponentParameter.CreateParameter("mode", "New"),
                ComponentParameter.CreateParameter("planning", Planning),
                ComponentParameter.CreateParameter("scheduleList", ScheduleList )
            );

            var editPlanningSaveLayers = cut.WaitForElement("#editPlanningSaveLayers");
            editPlanningSaveLayers.Click();

            cut.WaitForAssertion(() => this.mockNavigationManager.Uri.Should().EndWith("/planning"));
            cut.WaitForAssertion(() => MockRepository.VerifyAll());
        }

        [Test]
        public async Task EditPlanningInit_SaveUpdatedLayers()
        {
            var expectedId = Fixture.Create<string>();

            var expectedLayers = Fixture.CreateMany<DeviceModelDto>(1).ToList();
            var expectedDeviceModelCommandDto = Fixture.CreateMany<DeviceModelCommandDto>(3).ToList();

            PlanningDto Planning = new PlanningDto
            {
                DayOff = DaysEnumFlag.DaysOfWeek.Saturday | DaysEnumFlag.DaysOfWeek.Sunday,
                CommandId = expectedDeviceModelCommandDto[0].Id
            };
            ScheduleDto firstSchedule = new ScheduleDto
            {
                Start = "00:00",
                End = "23:59",
                CommandId = expectedDeviceModelCommandDto[0].Id
            };
            ScheduleDto secondSchedule = new ScheduleDto
            {
                Start = "00:00",
                End = "23:59",
                CommandId = expectedDeviceModelCommandDto[0].Id
            };

            _ = this.mockLayerClientService.Setup(service => service.GetLayers())
                .ReturnsAsync(new List<LayerDto>());

            _ = this.mockScheduleClientService.Setup(service => service.CreateSchedule(It.IsAny<ScheduleDto>()))
                .ReturnsAsync(expectedId);

            _ = this.mockScheduleClientService.Setup(service => service.DeleteSchedule(It.IsAny<string>()))
                .Returns(Task.CompletedTask);

            _ = this.mockScheduleClientService.Setup(service => service.UpdateSchedule(It.IsAny<ScheduleDto>()))
                .Returns(Task.CompletedTask);

            _ = this.mockPlanningClientService.Setup(service => service.UpdatePlanning(It.IsAny<PlanningDto>()))
                .Returns(Task.CompletedTask);

            _ = this.mockDeviceModelsClientService.Setup(service => service.GetDeviceModels(It.IsAny<DeviceModelFilter>()))
                .ReturnsAsync(new PaginationResult<DeviceModelDto>
                {
                    Items = expectedLayers
                });

            _ = this.mockLoRaWanDeviceModelsClientService.Setup(service => service.GetDeviceModelCommands(It.IsAny<string>()))
                .ReturnsAsync(expectedDeviceModelCommandDto);

            // Act
            var cut = RenderComponent<EditPlanning>(
                ComponentParameter.CreateParameter("mode", "Edit"),
                ComponentParameter.CreateParameter("planning", Planning),
                ComponentParameter.CreateParameter("scheduleList", new List<ScheduleDto>{firstSchedule, secondSchedule}),
                ComponentParameter.CreateParameter("initScheduleList", new List<ScheduleDto>{firstSchedule, secondSchedule})
            );

            var editPlanningAddLayers = cut.WaitForElement("#editPlanningAddLayers");
            editPlanningAddLayers.Click();

            var editPlanningDeleteLayers = cut.FindAll("#editPlanningDeleteLayers")[1];
            editPlanningDeleteLayers.Click();

            var editPlanningSaveLayers = cut.WaitForElement("#editPlanningSaveLayers");
            editPlanningSaveLayers.Click();

            cut.WaitForAssertion(() => this.mockNavigationManager.Uri.Should().EndWith("/planning"));
            cut.WaitForAssertion(() => MockRepository.VerifyAll());
        }

        [Test]
        public void EditPlanningInit_SaveNewLayers_ProblemDetailsException()
        {
            var expectedId = Fixture.Create<string>();

            var expectedLayers = Fixture.CreateMany<DeviceModelDto>(1).ToList();
            var expectedDeviceModelCommandDto = Fixture.CreateMany<DeviceModelCommandDto>(3).ToList();

            PlanningDto Planning = new PlanningDto
            {
                DayOff = DaysEnumFlag.DaysOfWeek.Saturday | DaysEnumFlag.DaysOfWeek.Sunday,
                CommandId = expectedDeviceModelCommandDto[0].Id
            };
            ScheduleDto firstSchedule = new ScheduleDto
            {
                Start = "00:00"
            };

            List<ScheduleDto> ScheduleList = new List<ScheduleDto>
            {
                firstSchedule
            };

            _ = this.mockLayerClientService.Setup(service => service.GetLayers())
                .ReturnsAsync(new List<LayerDto>());

            _ = this.mockPlanningClientService.Setup(service => service.CreatePlanning(It.IsAny<PlanningDto>()))
                .ThrowsAsync(new ProblemDetailsException(new ProblemDetailsWithExceptionDetails()));

            _ = this.mockDeviceModelsClientService.Setup(service => service.GetDeviceModels(It.IsAny<DeviceModelFilter>()))
                .ReturnsAsync(new PaginationResult<DeviceModelDto>
                {
                    Items = expectedLayers
                });

            _ = this.mockLoRaWanDeviceModelsClientService.Setup(service => service.GetDeviceModelCommands(It.IsAny<string>()))
                .ReturnsAsync(expectedDeviceModelCommandDto);

            // Act
            var cut = RenderComponent<EditPlanning>(
                ComponentParameter.CreateParameter("mode", "New"),
                ComponentParameter.CreateParameter("planning", Planning),
                ComponentParameter.CreateParameter("scheduleList", ScheduleList )
            );

            var editPlanningSaveLayers = cut.WaitForElement("#editPlanningSaveLayers");
            editPlanningSaveLayers.Click();

            cut.WaitForAssertion(() => MockRepository.VerifyAll());
        }
    }
}
