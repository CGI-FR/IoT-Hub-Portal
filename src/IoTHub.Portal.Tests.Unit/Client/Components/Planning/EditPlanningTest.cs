// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace IoTHub.Portal.Tests.Unit.Client.Components.Planning
{
    using Bunit;
    using IoTHub.Portal.Client.Dialogs.Planning;
    using Moq;
    using MudBlazor;

    internal class EditPlanningTest : BlazorUnitTest
    {
        private Mock<IPlanningClientService> mockPlanningClientService;
        private Mock<IScheduleClientService> mockScheduleClientService;
        private Mock<ILayerClientService> mockLayerClientService;
        private Mock<IDeviceModelsClientService> mockDeviceModelsClientService;
        private Mock<ILoRaWanDeviceModelsClientService> mockLoRaWanDeviceModelsClientService;
        private Mock<IDialogService> mockDialogService;
        private FakeNavigationManager mockNavigationManager;

        public override void Setup()
        {
            base.Setup();

            this.mockScheduleClientService = MockRepository.Create<IScheduleClientService>();
            this.mockPlanningClientService = MockRepository.Create<IPlanningClientService>();
            this.mockLayerClientService = MockRepository.Create<ILayerClientService>();
            this.mockDeviceModelsClientService = MockRepository.Create<IDeviceModelsClientService>();
            this.mockLoRaWanDeviceModelsClientService = MockRepository.Create<ILoRaWanDeviceModelsClientService>();
            this.mockDialogService = MockRepository.Create<IDialogService>();

            _ = Services.AddSingleton(this.mockScheduleClientService.Object);
            _ = Services.AddSingleton(this.mockPlanningClientService.Object);
            _ = Services.AddSingleton(this.mockLayerClientService.Object);
            _ = Services.AddSingleton(this.mockDeviceModelsClientService.Object);
            _ = Services.AddSingleton(this.mockLoRaWanDeviceModelsClientService.Object);
            _ = Services.AddSingleton(new PortalSettings { IsLoRaSupported = true });
            _ = Services.AddSingleton(this.mockDialogService.Object);

            this.mockNavigationManager = Services.GetRequiredService<FakeNavigationManager>();
        }

        [Test]
        public void EditPlanningInit()
        {
            var expectedDeviceModelDto = Fixture.Create<DeviceModelDto>();
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
                    Items = new List<DeviceModelDto> { expectedDeviceModelDto }
                });

            _ = this.mockLoRaWanDeviceModelsClientService.Setup(service => service.GetDeviceModelCommands(It.IsAny<string>()))
                .ReturnsAsync(expectedDeviceModelCommandDto);

            // Act
            var cut = RenderComponent<EditPlanning>(
                ComponentParameter.CreateParameter("mode", "New"),
                ComponentParameter.CreateParameter("planning", planning),
                ComponentParameter.CreateParameter("scheduleList", scheduleList),
                ComponentParameter.CreateParameter("SelectedModel", expectedDeviceModelDto.Name)
            );

            Assert.AreEqual(DaysEnumFlag.DaysOfWeek.Saturday | DaysEnumFlag.DaysOfWeek.Sunday, cut.Instance.planning.DayOff);
            Assert.AreEqual("00:00", cut.Instance.scheduleList[0].Start);
            cut.WaitForAssertion(() => MockRepository.VerifyAll());
        }

        [Test]
        public void EditPlanningInit_AddScheduleShouldNotWork()
        {
            var expectedDeviceModelDto = Fixture.CreateMany<DeviceModelDto>(1).ToList();
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
                    Items = expectedDeviceModelDto
                });

            _ = this.mockLoRaWanDeviceModelsClientService.Setup(service => service.GetDeviceModelCommands(It.IsAny<string>()))
                .ReturnsAsync(expectedDeviceModelCommandDto);

            // Act
            var cut = RenderComponent<EditPlanning>(
                ComponentParameter.CreateParameter("mode", "New"),
                ComponentParameter.CreateParameter("planning", planning),
                ComponentParameter.CreateParameter("scheduleList", scheduleList )
            );

            var editPlanningAddSchedule = cut.WaitForElement("#addScheduleButton");
            editPlanningAddSchedule.Click();

            Assert.AreEqual(1, cut.Instance.scheduleList.Count);
            cut.WaitForAssertion(() => MockRepository.VerifyAll());
        }

        [Test]
        public async Task EditPlanningInit_AddSchedule()
        {
            var expectedDeviceModelDto = Fixture.CreateMany<DeviceModelDto>(1).ToList();
            var expectedDeviceModelCommandDto = Fixture.CreateMany<DeviceModelCommandDto>(3).ToList();

            var planning = new PlanningDto
            {
                DayOff = DaysEnumFlag.DaysOfWeek.Saturday | DaysEnumFlag.DaysOfWeek.Sunday,
                CommandId = expectedDeviceModelCommandDto[0].Id
            };
            var firstSchedule = new ScheduleDto
            {
                Start = "00:00",
                CommandId = expectedDeviceModelCommandDto[0].Id
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
                    Items = expectedDeviceModelDto
                });

            _ = this.mockLoRaWanDeviceModelsClientService.Setup(service => service.GetDeviceModelCommands(It.IsAny<string>()))
                .ReturnsAsync(expectedDeviceModelCommandDto);

            // Act
            var cut = RenderComponent<EditPlanning>(
                ComponentParameter.CreateParameter("mode", "New"),
                ComponentParameter.CreateParameter("planning", planning),
                ComponentParameter.CreateParameter("scheduleList", scheduleList )
            );

            var endField = cut.FindComponents<MudTextField<string>>()[2];
            await cut.InvokeAsync(() => endField.Instance.SetText("23:59"));

            var editPlanningAddSchedule = cut.WaitForElement("#addScheduleButton");
            editPlanningAddSchedule.Click();

            Assert.AreEqual(2, cut.Instance.scheduleList.Count);
            cut.WaitForAssertion(() => MockRepository.VerifyAll());
        }

        [Test]
        public async Task EditPlanningInit_DeleteSchedule()
        {
            var expectedDeviceModelDto = Fixture.CreateMany<DeviceModelDto>(1).ToList();
            var expectedDeviceModelCommandDto = Fixture.CreateMany<DeviceModelCommandDto>(3).ToList();

            var planning = new PlanningDto
            {
                DayOff = DaysEnumFlag.DaysOfWeek.Saturday | DaysEnumFlag.DaysOfWeek.Sunday,
                CommandId = expectedDeviceModelCommandDto[0].Id
            };
            var firstSchedule = new ScheduleDto
            {
                Start = "00:00",
                CommandId = expectedDeviceModelCommandDto[0].Id
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
                    Items = expectedDeviceModelDto
                });

            _ = this.mockLoRaWanDeviceModelsClientService.Setup(service => service.GetDeviceModelCommands(It.IsAny<string>()))
                .ReturnsAsync(expectedDeviceModelCommandDto);

            // Act
            var cut = RenderComponent<EditPlanning>(
                ComponentParameter.CreateParameter("mode", "New"),
                ComponentParameter.CreateParameter("planning", planning),
                ComponentParameter.CreateParameter("scheduleList", scheduleList )
            );

            var endField = cut.FindComponents<MudTextField<string>>()[2];
            await cut.InvokeAsync(() => endField.Instance.SetText("23:59"));

            var editPlanningAddSchedule = cut.WaitForElement("#addScheduleButton");
            editPlanningAddSchedule.Click();

            var editPlanningDeleteSchedule = cut.FindAll("#deleteScheduleButton")[1];
            editPlanningDeleteSchedule.Click();

            Assert.AreEqual(1, cut.Instance.scheduleList.Count);
            cut.WaitForAssertion(() => MockRepository.VerifyAll());
        }

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

        [Test]
        public void ClickOnDeleteShouldDisplayConfirmationDialogAndReturnIfAborted()
        {
            var mockPlanning = new PlanningDto
            {
                Id = Guid.NewGuid().ToString(),
                Name = Guid.NewGuid().ToString()
            };

            var mockDeviceModel = new DeviceModelDto
            {
                ModelId = Guid.NewGuid().ToString(),
                Name = Guid.NewGuid().ToString()
            };

            var mockScheduleList = Fixture.CreateMany<ScheduleDto>(2).ToList();

            _ = this.mockLayerClientService.Setup(service =>
                    service.GetLayers())
                    .ReturnsAsync(new List<LayerDto>
                    {
                        new LayerDto { Id = Guid.NewGuid().ToString(), Name = Guid.NewGuid().ToString() }
                    });

            _ = this.mockDeviceModelsClientService.Setup(service => service.GetDeviceModelsAsync(It.IsAny<DeviceModelFilter>()))
                .ReturnsAsync(new PaginationResult<DeviceModelDto>
                {
                    Items = new List<DeviceModelDto>()
                });

            var mockDialogReference = MockRepository.Create<IDialogReference>();
            _ = mockDialogReference.Setup(c => c.Result).ReturnsAsync(DialogResult.Cancel);
            _ = this.mockDialogService.Setup(c => c.Show<DeletePlanningDialog>(It.IsAny<string>(), It.IsAny<DialogParameters>()))
                .Returns(mockDialogReference.Object);

            var cut = RenderComponent<EditPlanning>(parameters => parameters.Add(p => p.mode, "Edit")
                                                                            .Add(p => p.planning, mockPlanning)
                                                                            .Add(p => p.scheduleList, mockScheduleList)
                                                                            .Add(p => p.initScheduleList, new List<ScheduleDto>(mockScheduleList))
                                                                            .Add(p => p.SelectedModel, mockDeviceModel.Name));

            var deleteButton = cut.WaitForElement("#deleteButton");
            deleteButton.Click();

            cut.WaitForAssertion(() => MockRepository.VerifyAll());
        }


        [Test]
        public void ClickOnDeleteShouldDisplayConfirmationDialogAndRedirectIfConfirmed()
        {
            var mockPlanning = new PlanningDto
            {
                Id = Guid.NewGuid().ToString(),
                Name = Guid.NewGuid().ToString()
            };

            var mockDeviceModel = new List<DeviceModelDto>
            {
                new DeviceModelDto
                {
                    ModelId = Guid.NewGuid().ToString(), Name = Guid.NewGuid().ToString()
                },
                new DeviceModelDto
                {
                    ModelId = Guid.NewGuid().ToString(), Name = Guid.NewGuid().ToString()
                }
            };

            var mockScheduleList = Fixture.CreateMany<ScheduleDto>(2).ToList();

            var deviceModels = Fixture.CreateMany<DeviceModelDto>(2).ToList();

            var expectedPaginatedDeviceModels = new PaginationResult<DeviceModelDto>()
            {
                Items = mockDeviceModel.ToList(),
                TotalItems = mockDeviceModel.Count
            };

            _ = this.mockLayerClientService.Setup(service =>
                    service.GetLayers())
                    .ReturnsAsync(new List<LayerDto>
                    {
                        new LayerDto { Id = Guid.NewGuid().ToString(), Name = Guid.NewGuid().ToString() }
                    });

            _ = this.mockDeviceModelsClientService.Setup(service => service.GetDeviceModelsAsync(It.IsAny<DeviceModelFilter>()))
                .ReturnsAsync(new PaginationResult<DeviceModelDto>
                {
                    Items = new List<DeviceModelDto>()
                });

            var mockDialogReference = MockRepository.Create<IDialogReference>();
            _ = mockDialogReference.Setup(c => c.Result).ReturnsAsync(DialogResult.Ok("Ok"));
            _ = this.mockDialogService.Setup(c => c.Show<DeletePlanningDialog>(It.IsAny<string>(), It.IsAny<DialogParameters>()))
                .Returns(mockDialogReference.Object);

            var cut = RenderComponent<EditPlanning>(parameters => parameters.Add(p => p.mode, "Edit")
                                                                            .Add(p => p.planning, mockPlanning)
                                                                            .Add(p => p.scheduleList, mockScheduleList)
                                                                            .Add(p => p.initScheduleList, new List<ScheduleDto>(mockScheduleList))
                                                                            .Add(p => p.SelectedModel, mockDeviceModel[0].Name));

            var deleteButton = cut.WaitForElement("#deleteButton");
            deleteButton.Click();

            cut.WaitForState(() => this.mockNavigationManager.Uri.EndsWith("/planning", StringComparison.OrdinalIgnoreCase));
            cut.WaitForAssertion(() => MockRepository.VerifyAll());
        }

        [Test]
        public async Task DisplayLayersRenderCorrectly()
        {
            var mockPlanning = new PlanningDto
            {
                Id = Guid.NewGuid().ToString(),
                Name = Guid.NewGuid().ToString()
            };

            var mockDeviceModel = new DeviceModelDto
            {
                ModelId = Guid.NewGuid().ToString(),
                Name = Guid.NewGuid().ToString()
            };

            var mockScheduleList = Fixture.CreateMany<ScheduleDto>(2).ToList();

            var mockPrincipalLayer = new LayerDto
            {
                Id = Guid.NewGuid().ToString(),
                Name = Guid.NewGuid().ToString(),
                Planning = null,
                Father = null
            };

            var mockSubLayer1 = new LayerDto
            {
                Id = Guid.NewGuid().ToString(),
                Name = Guid.NewGuid().ToString(),
                Planning = mockPlanning.Id,
                Father = mockPrincipalLayer.Id
            };

            var mockSubLayer2 = new LayerDto
            {
                Id = Guid.NewGuid().ToString(),
                Name = Guid.NewGuid().ToString(),
                Planning = Guid.NewGuid().ToString(),
                Father = mockPrincipalLayer.Id
            };

            _ = this.mockLayerClientService.Setup(service =>
                    service.GetLayers())
                    .ReturnsAsync(new List<LayerDto>
                    {
                        mockPrincipalLayer,
                        mockSubLayer1,
                        mockSubLayer2
                    });

            _ = this.mockDeviceModelsClientService.Setup(service => service.GetDeviceModelsAsync(It.IsAny<DeviceModelFilter>()))
                .ReturnsAsync(new PaginationResult<DeviceModelDto>
                {
                    Items = new List<DeviceModelDto>()
                });

            var cut = RenderComponent<EditPlanning>(parameters => parameters.Add(p => p.mode, "Edit")
                                                                            .Add(p => p.planning, mockPlanning)
                                                                            .Add(p => p.scheduleList, mockScheduleList)
                                                                            .Add(p => p.initScheduleList, new List<ScheduleDto>(mockScheduleList))
                                                                            .Add(p => p.SelectedModel, mockDeviceModel.Name));

            var tooltips = cut.FindComponents<MudTooltip>();
            _ = tooltips[0].Instance.Text.Should().Be("Add layer");
            _ = tooltips[1].Instance.Text.Should().Be("Already registered");
            _ = tooltips[2].Instance.Text.Should().Be("Registered on other planning");
            cut.WaitForAssertion(() => MockRepository.VerifyAll());
        }
    }
}
