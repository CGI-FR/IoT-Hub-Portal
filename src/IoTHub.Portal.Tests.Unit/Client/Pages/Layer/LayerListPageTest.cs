// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace IoTHub.Portal.Tests.Unit.Client.Pages.Layer
{
    using Portal.Shared.Security;

    internal class LayerListPageTest : BlazorUnitTest
    {
        private Mock<ILayerClientService> mockLayerClientService;
        private Mock<IDeviceClientService> mockDeviceClientService;
        private Mock<IPlanningClientService> mockPlanningClientService;
        private FakeNavigationManager mockNavigationManager;

        public override void Setup()
        {
            base.Setup();

            this.mockLayerClientService = MockRepository.Create<ILayerClientService>();
            this.mockDeviceClientService = MockRepository.Create<IDeviceClientService>();
            this.mockPlanningClientService = MockRepository.Create<IPlanningClientService>();

            _ = Services.AddSingleton(this.mockLayerClientService.Object);
            _ = Services.AddSingleton(this.mockDeviceClientService.Object);
            _ = Services.AddSingleton(this.mockPlanningClientService.Object);
            _ = Services.AddSingleton(new PortalSettings { IsLoRaSupported = true });

            this.mockNavigationManager = Services.GetRequiredService<FakeNavigationManager>();

            _ = this.mockPermissionsService.Setup(service => service.GetUserPermissions())
                .ReturnsAsync(new[] { PortalPermissions.LayerRead, PortalPermissions.LayerWrite });
        }

        [Test]
        public void LayerListPageOnClickAddLayer()
        {
            var expectedLayerDto = Fixture.Create<LayerDto>();
            expectedLayerDto.Name = "Main Layer";
            expectedLayerDto.Father = null;
            expectedLayerDto.Planning = "None";

            var expectedLayerDtoChild = Fixture.Create<LayerDto>();

            _ = this.mockLayerClientService.Setup(service => service.GetLayers())
                .ReturnsAsync(new List<LayerDto>(new List<LayerDto> { expectedLayerDto }));

            _ = this.mockLayerClientService.Setup(service => service.CreateLayer(It.IsAny<LayerDto>()))
                .ReturnsAsync(expectedLayerDtoChild.Id);

            _ = this.mockDeviceClientService.Setup(service => service.GetDevices(It.IsAny<string>()))
                .ReturnsAsync(new PaginationResult<DeviceListItem>());

            _ = this.mockPlanningClientService.Setup(service => service.GetPlannings())
                .ReturnsAsync(new List<PlanningDto>());

            // Act
            var cut = RenderComponent<LayersListPage>();

            cut.WaitForAssertion(() => cut.FindAll("li[id^='layer-']").Count.Should().Be(1));
            var editLayerMouseOver = cut.WaitForElement($"#layer-{expectedLayerDto.Id}");
            editLayerMouseOver.MouseOver();

            var editLayerAddLayers = cut.WaitForElement($"#layer-{expectedLayerDto.Id}-AddLayer");
            editLayerAddLayers.Click();
            cut.WaitForAssertion(() => cut.FindAll("li[id^='layer-']").Count.Should().Be(2));

            cut.WaitForAssertion(() => MockRepository.VerifyAll());
        }

        [Test]
        public void LayerListPageOnClickDeleteSubChildLayer()
        {
            var expectedLayerDto = Fixture.Create<LayerDto>();
            expectedLayerDto.Name = "Main Layer";
            expectedLayerDto.Father = null;
            expectedLayerDto.Planning = "None";

            var expectedChildrenLayerDto1 = Fixture.Create<LayerDto>();
            expectedChildrenLayerDto1.Father = expectedLayerDto.Id;
            expectedChildrenLayerDto1.Planning = "None";

            var expectedChildrenLayerDto2 = Fixture.Create<LayerDto>();
            expectedChildrenLayerDto2.Father = expectedChildrenLayerDto1.Id;
            expectedChildrenLayerDto2.Planning = "None";

            _ = this.mockLayerClientService.Setup(service => service.GetLayers())
                .ReturnsAsync(new List<LayerDto>(new List<LayerDto> { expectedLayerDto, expectedChildrenLayerDto1, expectedChildrenLayerDto2 }));

            _ = this.mockLayerClientService.Setup(service => service.DeleteLayer(It.IsAny<string>()))
                .Returns(Task.CompletedTask);

            _ = this.mockDeviceClientService.Setup(service => service.GetDevices(It.IsAny<string>()))
                .ReturnsAsync(new PaginationResult<DeviceListItem>());

            _ = this.mockPlanningClientService.Setup(service => service.GetPlannings())
                .ReturnsAsync(new List<PlanningDto>());

            // Act
            var cut = RenderComponent<LayersListPage>();

            cut.WaitForAssertion(() => cut.FindAll("li[id^='layer-']").Count.Should().Be(3));

            var editLayerMouseOver = cut.FindAll("li[id^='layer-']");
            editLayerMouseOver[2].MouseOver();

            var editLayerDeleteLayer = cut.WaitForElement($"#layer-{expectedChildrenLayerDto2.Id}-DeleteLayer");
            editLayerDeleteLayer.Click();

            cut.WaitForAssertion(() => cut.FindAll("li[id^='layer-']").Count.Should().Be(2));

            cut.WaitForAssertion(() => MockRepository.VerifyAll());
        }

        [Test]
        public void LayerListPageOnClickDeleteChildLayer()
        {
            var expectedLayerDto = new LayerDto()
            {
                Id = Guid.NewGuid().ToString(),
                Name = "Main Layer",
                Father = null,
                Planning = "None"
            };

            var expectedChildrenLayerDto1 = Fixture.Create<LayerDto>();
            expectedChildrenLayerDto1.Father = expectedLayerDto.Id;
            expectedChildrenLayerDto1.Planning = "None";

            var expectedChildrenLayerDto2 = Fixture.Create<LayerDto>();
            expectedChildrenLayerDto2.Father = expectedChildrenLayerDto1.Id;
            expectedChildrenLayerDto2.Planning = "None";

            _ = this.mockLayerClientService.Setup(service => service.GetLayers())
                .ReturnsAsync(new List<LayerDto>(new List<LayerDto> { expectedLayerDto, expectedChildrenLayerDto1, expectedChildrenLayerDto2 }));

            _ = this.mockLayerClientService.Setup(service => service.DeleteLayer(It.IsAny<string>()))
                .Returns(Task.CompletedTask);

            _ = this.mockDeviceClientService.Setup(service => service.GetDevices(It.IsAny<string>()))
                .ReturnsAsync(new PaginationResult<DeviceListItem>());

            _ = this.mockPlanningClientService.Setup(service => service.GetPlannings())
                .ReturnsAsync(new List<PlanningDto>());

            // Act
            var cut = RenderComponent<LayersListPage>();

            cut.WaitForAssertion(() => cut.FindAll("li[id^='layer-']").Count.Should().Be(3));

            var editLayerMouseOver = cut.FindAll("li[id^='layer-']");
            editLayerMouseOver[1].MouseOver();

            var editLayerDeleteLayer = cut.WaitForElement($"#layer-{expectedChildrenLayerDto1.Id}-DeleteLayer");
            editLayerDeleteLayer.Click();

            cut.WaitForAssertion(() => cut.FindAll("li[id^='layer-']").Count.Should().Be(1));

            cut.WaitForAssertion(() => MockRepository.VerifyAll());
        }
    }
}
