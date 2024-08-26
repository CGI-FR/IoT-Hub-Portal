// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace IoTHub.Portal.Tests.Unit.Client.Components.Layer
{
    using System;
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

        // TODO: To fix
        //[Test]
        //public void EditPlanningInit()
        //{
        //    var expectedDeviceModelCommandDto = Fixture.CreateMany<DeviceModelCommandDto>(3).ToList();

        //    var planning = new PlanningDto
        //    {
        //        DayOff = DaysEnumFlag.DaysOfWeek.Saturday | DaysEnumFlag.DaysOfWeek.Sunday,
        //        CommandId = expectedDeviceModelCommandDto[0].Id
        //    };
        //    var firstSchedule = new ScheduleDto
        //    {
        //        Start = "00:00"
        //    };

        //    var scheduleList = new List<ScheduleDto>
        //    {
        //        firstSchedule
        //    };

        //    _ = this.mockLayerClientService.Setup(service => service.GetLayers())
        //        .ReturnsAsync(new List<LayerDto>());

        //    _ = this.mockDeviceModelsClientService.Setup(service => service.GetDeviceModels(It.IsAny<DeviceModelFilter>()))
        //        .ReturnsAsync(new PaginationResult<DeviceModelDto>
        //        {
        //            Items = new List<DeviceModelDto>()
        //        });

        //    // Act
        //    var cut = RenderComponent<EditPlanning>(
        //        ComponentParameter.CreateParameter("mode", "New"),
        //        ComponentParameter.CreateParameter("planning", planning),
        //        ComponentParameter.CreateParameter("scheduleList", scheduleList )
        //    );

        //    Assert.AreEqual(DaysEnumFlag.DaysOfWeek.Saturday | DaysEnumFlag.DaysOfWeek.Sunday, cut.Instance.planning.DayOff);
        //    Assert.AreEqual("00:00", cut.Instance.scheduleList[0].Start);
        //    cut.WaitForAssertion(() => MockRepository.VerifyAll());
        //}

        // TODO: To fix
        //[Test]
        //public void EditPlanningInit_AddScheduleShouldNotWork()
        //{
        //    var expectedLayers = Fixture.CreateMany<DeviceModelDto>(1).ToList();
        //    var expectedDeviceModelCommandDto = Fixture.CreateMany<DeviceModelCommandDto>(3).ToList();

        //    var planning = new PlanningDto
        //    {
        //        DayOff = DaysEnumFlag.DaysOfWeek.Saturday | DaysEnumFlag.DaysOfWeek.Sunday,
        //        CommandId = expectedDeviceModelCommandDto[0].Id
        //    };
        //    var firstSchedule = new ScheduleDto
        //    {
        //        Start = "00:00"
        //    };

        //    var scheduleList = new List<ScheduleDto>
        //    {
        //        firstSchedule
        //    };

        //    _ = this.mockLayerClientService.Setup(service => service.GetLayers())
        //        .ReturnsAsync(new List<LayerDto>());

        //    _ = this.mockDeviceModelsClientService.Setup(service => service.GetDeviceModels(It.IsAny<DeviceModelFilter>()))
        //        .ReturnsAsync(new PaginationResult<DeviceModelDto>
        //        {
        //            Items = expectedLayers
        //        });

        //    _ = this.mockLoRaWanDeviceModelsClientService.Setup(service => service.GetDeviceModelCommands(It.IsAny<string>()))
        //        .ReturnsAsync(expectedDeviceModelCommandDto);

        //    // Act
        //    var cut = RenderComponent<EditPlanning>(
        //        ComponentParameter.CreateParameter("mode", "New"),
        //        ComponentParameter.CreateParameter("planning", planning),
        //        ComponentParameter.CreateParameter("scheduleList", scheduleList )
        //    );

        //    var editPlanningAddLayers = cut.WaitForElement("#editPlanningAddLayers");
        //    editPlanningAddLayers.Click();

        //    Assert.AreEqual(1, cut.Instance.scheduleList.Count);
        //    cut.WaitForAssertion(() => MockRepository.VerifyAll());
        //}

        // TODO: To fix
        //[Test]
        //public async Task EditPlanningInit_AddSchedule()
        //{
        //    var expectedLayers = Fixture.CreateMany<DeviceModelDto>(1).ToList();
        //    var expectedDeviceModelCommandDto = Fixture.CreateMany<DeviceModelCommandDto>(3).ToList();

        //    var planning = new PlanningDto
        //    {
        //        DayOff = DaysEnumFlag.DaysOfWeek.Saturday | DaysEnumFlag.DaysOfWeek.Sunday,
        //        CommandId = expectedDeviceModelCommandDto[0].Id
        //    };
        //    var firstSchedule = new ScheduleDto
        //    {
        //        Start = "00:00",
        //        CommandId = expectedDeviceModelCommandDto[0].Id
        //    };

        //    var scheduleList = new List<ScheduleDto>
        //    {
        //        firstSchedule
        //    };

        //    _ = this.mockLayerClientService.Setup(service => service.GetLayers())
        //        .ReturnsAsync(new List<LayerDto>());

        //    _ = this.mockDeviceModelsClientService.Setup(service => service.GetDeviceModels(It.IsAny<DeviceModelFilter>()))
        //        .ReturnsAsync(new PaginationResult<DeviceModelDto>
        //        {
        //            Items = expectedLayers
        //        });

        //    _ = this.mockLoRaWanDeviceModelsClientService.Setup(service => service.GetDeviceModelCommands(It.IsAny<string>()))
        //        .ReturnsAsync(expectedDeviceModelCommandDto);

        //    // Act
        //    var cut = RenderComponent<EditPlanning>(
        //        ComponentParameter.CreateParameter("mode", "New"),
        //        ComponentParameter.CreateParameter("planning", planning),
        //        ComponentParameter.CreateParameter("scheduleList", scheduleList )
        //    );

        //    var endField = cut.FindComponents<MudTextField<string>>()[2];
        //    await cut.InvokeAsync(() => endField.Instance.SetText("23:59"));

        //    var editPlanningAddLayers = cut.WaitForElement("#editPlanningAddLayers");
        //    editPlanningAddLayers.Click();

        //    Assert.AreEqual(2, cut.Instance.scheduleList.Count);
        //    cut.WaitForAssertion(() => MockRepository.VerifyAll());
        //}

        // TODO: To fix
        //[Test]
        //public async Task EditPlanningInit_DeleteSchedule()
        //{
        //    var expectedLayers = Fixture.CreateMany<DeviceModelDto>(1).ToList();
        //    var expectedDeviceModelCommandDto = Fixture.CreateMany<DeviceModelCommandDto>(3).ToList();

        //    var planning = new PlanningDto
        //    {
        //        DayOff = DaysEnumFlag.DaysOfWeek.Saturday | DaysEnumFlag.DaysOfWeek.Sunday,
        //        CommandId = expectedDeviceModelCommandDto[0].Id
        //    };
        //    var firstSchedule = new ScheduleDto
        //    {
        //        Start = "00:00",
        //        CommandId = expectedDeviceModelCommandDto[0].Id
        //    };

        //    var scheduleList = new List<ScheduleDto>
        //    {
        //        firstSchedule
        //    };

        //    _ = this.mockLayerClientService.Setup(service => service.GetLayers())
        //        .ReturnsAsync(new List<LayerDto>());

        //    _ = this.mockDeviceModelsClientService.Setup(service => service.GetDeviceModels(It.IsAny<DeviceModelFilter>()))
        //        .ReturnsAsync(new PaginationResult<DeviceModelDto>
        //        {
        //            Items = expectedLayers
        //        });

        //    _ = this.mockLoRaWanDeviceModelsClientService.Setup(service => service.GetDeviceModelCommands(It.IsAny<string>()))
        //        .ReturnsAsync(expectedDeviceModelCommandDto);

        //    // Act
        //    var cut = RenderComponent<EditPlanning>(
        //        ComponentParameter.CreateParameter("mode", "New"),
        //        ComponentParameter.CreateParameter("planning", planning),
        //        ComponentParameter.CreateParameter("scheduleList", scheduleList )
        //    );

        //    var endField = cut.FindComponents<MudTextField<string>>()[2];
        //    await cut.InvokeAsync(() => endField.Instance.SetText("23:59"));

        //    var editPlanningAddLayers = cut.WaitForElement("#editPlanningAddLayers");
        //    editPlanningAddLayers.Click();

        //    var editPlanningDeleteLayers = cut.FindAll("#editPlanningDeleteLayers")[1];
        //    editPlanningDeleteLayers.Click();

        //    Assert.AreEqual(1, cut.Instance.scheduleList.Count);
        //    cut.WaitForAssertion(() => MockRepository.VerifyAll());
        //}

        [Test]
        public void EditPlanningInit_ProblemDetailsException()
        {
            var planning = new PlanningDto
            {
                DayOff = DaysEnumFlag.DaysOfWeek.Saturday | DaysEnumFlag.DaysOfWeek.Sunday
            };
            var firstSchedule = new ScheduleDto
            {
                Start = "00:00",
            };

            var scheduleList = new List<ScheduleDto>
            {
                firstSchedule
            };

            _ = this.mockLayerClientService.Setup(service => service.GetLayers())
                .ReturnsAsync(new List<LayerDto>());

            _ = this.mockDeviceModelsClientService.Setup(service => service.GetDeviceModelsAsync(It.IsAny<DeviceModelFilter>()))
                .ThrowsAsync(new ProblemDetailsException(new ProblemDetailsWithExceptionDetails()));

            // Act
            var cut = RenderComponent<EditPlanning>(
                ComponentParameter.CreateParameter("mode", "New"),
                ComponentParameter.CreateParameter("planning", planning),
                ComponentParameter.CreateParameter("scheduleList", scheduleList )
            );

            cut.WaitForAssertion(() => MockRepository.VerifyAll());
        }

        [Test]
        public void EditPlanningInit_ChangeOnDay()
        {
            var expectedLayers = Fixture.CreateMany<DeviceModelDto>(1).ToList();
            var expectedDeviceModelCommandDto = Fixture.CreateMany<DeviceModelCommandDto>(3).ToList();

            var planning = new PlanningDto
            {
                DayOff = DaysEnumFlag.DaysOfWeek.Saturday | DaysEnumFlag.DaysOfWeek.Sunday,
                CommandId = expectedDeviceModelCommandDto[0].Id
            };
            var firstSchedule = new ScheduleDto
            {
                Start = "00:00"
            };

            var scheduleList = new List<ScheduleDto>
            {
                firstSchedule
            };

            _ = this.mockLayerClientService.Setup(service => service.GetLayers())
                .ReturnsAsync(new List<LayerDto>());

            _ = this.mockDeviceModelsClientService.Setup(service => service.GetDeviceModelsAsync(It.IsAny<DeviceModelFilter>()))
                .ReturnsAsync(new PaginationResult<DeviceModelDto>
                {
                    Items = expectedLayers
                });

            _ = this.mockLoRaWanDeviceModelsClientService.Setup(service => service.GetDeviceModelCommands(It.IsAny<string>()))
                .ReturnsAsync(expectedDeviceModelCommandDto);

            // Act
            var cut = RenderComponent<EditPlanning>(
                ComponentParameter.CreateParameter("mode", "New"),
                ComponentParameter.CreateParameter("planning", planning),
                ComponentParameter.CreateParameter("scheduleList", scheduleList )
            );

            var editPlanningChangeOnDayLayers = cut.FindAll("#editPlanningChangeOnDayLayers")[0];
            editPlanningChangeOnDayLayers.Click();

            Assert.AreEqual(DaysEnumFlag.DaysOfWeek.Monday | DaysEnumFlag.DaysOfWeek.Saturday | DaysEnumFlag.DaysOfWeek.Sunday, cut.Instance.planning.DayOff);
            cut.WaitForAssertion(() => MockRepository.VerifyAll());
        }

        [Test]
        public void EditPlanningInit_ChangeOffDay()
        {
            var expectedLayers = Fixture.CreateMany<DeviceModelDto>(1).ToList();
            var expectedDeviceModelCommandDto = Fixture.CreateMany<DeviceModelCommandDto>(3).ToList();

            var planning = new PlanningDto
            {
                DayOff = DaysEnumFlag.DaysOfWeek.Saturday | DaysEnumFlag.DaysOfWeek.Sunday,
                CommandId = expectedDeviceModelCommandDto[0].Id
            };
            var firstSchedule = new ScheduleDto
            {
                Start = "00:00"
            };

            var scheduleList = new List<ScheduleDto>
            {
                firstSchedule
            };

            _ = this.mockLayerClientService.Setup(service => service.GetLayers())
                .ReturnsAsync(new List<LayerDto>());

            _ = this.mockDeviceModelsClientService.Setup(service => service.GetDeviceModelsAsync(It.IsAny<DeviceModelFilter>()))
                .ReturnsAsync(new PaginationResult<DeviceModelDto>
                {
                    Items = expectedLayers
                });

            _ = this.mockLoRaWanDeviceModelsClientService.Setup(service => service.GetDeviceModelCommands(It.IsAny<string>()))
                .ReturnsAsync(expectedDeviceModelCommandDto);

            // Act
            var cut = RenderComponent<EditPlanning>(
                ComponentParameter.CreateParameter("mode", "New"),
                ComponentParameter.CreateParameter("planning", planning),
                ComponentParameter.CreateParameter("scheduleList", scheduleList )
            );

            var editPlanningChangeOffDayLayers = cut.FindAll("#editPlanningChangeOffDayLayers")[5];
            editPlanningChangeOffDayLayers.Click();

            Assert.AreEqual(DaysEnumFlag.DaysOfWeek.Sunday, cut.Instance.planning.DayOff);
            cut.WaitForAssertion(() => MockRepository.VerifyAll());
        }

        // TODO: To fix
        //[Test]
        //public void EditPlanningInit_SaveNewLayers()
        //{
        //    var expectedId = Fixture.Create<string>();
        //    var expectedLayers = Fixture.CreateMany<DeviceModelDto>(1).ToList();
        //    var expectedDeviceModelCommandDto = Fixture.CreateMany<DeviceModelCommandDto>(3).ToList();

        //    var planning = new PlanningDto
        //    {
        //        Id = Fixture.Create<string>(),
        //        Name = Fixture.Create<string>(),
        //        Start = DateTime.Now.AddDays(-1).ToString(),
        //        End = DateTime.Now.AddDays(1).ToString(),
        //        DayOff = DaysEnumFlag.DaysOfWeek.Saturday | DaysEnumFlag.DaysOfWeek.Sunday,
        //        CommandId = expectedDeviceModelCommandDto[0].Id
        //    };

        //    var firstSchedule = new ScheduleDto
        //    {
        //        Id = Fixture.Create<string>(),
        //        Start = "00:00",
        //        End = "24:00",
        //        PlanningId = planning.Id,
        //        CommandId = Fixture.Create<string>(),
        //    };

        //    var scheduleList = new List<ScheduleDto>
        //    {
        //        firstSchedule
        //    };

        //    _ = this.mockLayerClientService.Setup(service => service.GetLayers())
        //        .ReturnsAsync(new List<LayerDto>());

        //    _ = this.mockScheduleClientService.Setup(service => service.CreateSchedule(It.IsAny<ScheduleDto>()))
        //        .ReturnsAsync(expectedId);

        //    _ = this.mockPlanningClientService.Setup(service => service.CreatePlanning(It.IsAny<PlanningDto>()))
        //        .ReturnsAsync(expectedId);

        //    _ = this.mockDeviceModelsClientService.Setup(service => service.GetDeviceModels(It.IsAny<DeviceModelFilter>()))
        //        .ReturnsAsync(new PaginationResult<DeviceModelDto>
        //        {
        //            Items = expectedLayers
        //        });

        //    _ = this.mockLoRaWanDeviceModelsClientService.Setup(service => service.GetDeviceModelCommands(It.IsAny<string>()))
        //        .ReturnsAsync(expectedDeviceModelCommandDto);

        //    // Act
        //    var cut = RenderComponent<EditPlanning>(
        //        ComponentParameter.CreateParameter("mode", "New"),
        //        ComponentParameter.CreateParameter("planning", planning),
        //        ComponentParameter.CreateParameter("scheduleList", scheduleList )
        //    );

        //    var editPlanningSaveLayers = cut.WaitForElement("#editPlanningSaveLayers");
        //    editPlanningSaveLayers.Click();

        //    // Assert
        //    cut.WaitForAssertion(() => this.mockNavigationManager.Uri.Should().EndWith("/planning"));
        //    cut.WaitForAssertion(() => MockRepository.VerifyAll());
        //}

        // TODO: To fix
        //[Test]
        //public void EditPlanningInit_SaveNewLayers_ProblemDetailsException()
        //{
        //    var expectedLayers = Fixture.CreateMany<DeviceModelDto>(1).ToList();
        //    var expectedDeviceModelCommandDto = Fixture.CreateMany<DeviceModelCommandDto>(3).ToList();

        //    var planning = new PlanningDto
        //    {
        //        Id = Fixture.Create<string>(),
        //        Name = Fixture.Create<string>(),
        //        Start = DateTime.Now.AddDays(-1).ToString(),
        //        End = DateTime.Now.AddDays(1).ToString(),
        //        DayOff = DaysEnumFlag.DaysOfWeek.Saturday | DaysEnumFlag.DaysOfWeek.Sunday,
        //        CommandId = expectedDeviceModelCommandDto[0].Id
        //    };

        //    var firstSchedule = new ScheduleDto
        //    {
        //        Id = Fixture.Create<string>(),
        //        Start = "00:00",
        //        End = "24:00",
        //        PlanningId = planning.Id,
        //        CommandId = Fixture.Create<string>()
        //    };

        //    var scheduleList = new List<ScheduleDto>
        //    {
        //        firstSchedule
        //    };

        //    _ = this.mockLayerClientService.Setup(service => service.GetLayers())
        //        .ReturnsAsync(new List<LayerDto>());

        //    _ = this.mockPlanningClientService.Setup(service => service.CreatePlanning(It.IsAny<PlanningDto>()));

        //    _ = this.mockDeviceModelsClientService.Setup(service => service.GetDeviceModels(It.IsAny<DeviceModelFilter>()))
        //        .ReturnsAsync(new PaginationResult<DeviceModelDto>
        //        {
        //            Items = expectedLayers
        //        });

        //    _ = this.mockLoRaWanDeviceModelsClientService.Setup(service => service.GetDeviceModelCommands(It.IsAny<string>()))
        //        .ReturnsAsync(expectedDeviceModelCommandDto);


        //    _ = this.mockLoRaWanDeviceModelsClientService.Setup(service => service.GetDeviceModelCommands(It.IsAny<string>()))
        //        .ReturnsAsync(expectedDeviceModelCommandDto);

        //    // Act
        //    var cut = RenderComponent<EditPlanning>(
        //        ComponentParameter.CreateParameter("mode", "New"),
        //        ComponentParameter.CreateParameter("planning", planning),
        //        ComponentParameter.CreateParameter("scheduleList", scheduleList )
        //    );

        //    var editPlanningSaveLayers = cut.WaitForElement("#editPlanningSaveLayers");
        //    editPlanningSaveLayers.Click();

        //    cut.WaitForAssertion(() => MockRepository.VerifyAll());
        //}
    }
}
