// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace IoTHub.Portal.Tests.Unit.Client.Pages.Layer
{
    using Bunit;
    using IoTHub.Portal.Client.Pages.Layer;
    using IoTHub.Portal.Client.Services;
    using IoTHub.Portal.Tests.Unit.UnitTests.Bases;
    using Microsoft.Extensions.DependencyInjection;
    using Moq;
    using NUnit.Framework;
    using IoTHub.Portal.Models.v10;
    using System.Linq;
    using IoTHub.Portal.Shared.Models.v10;
    using AutoFixture;
    using System.Collections.Generic;
    using IoTHub.Portal.Client.Models;
    using IoTHub.Portal.Client.Exceptions;
    using Bunit.TestDoubles;
    using FluentAssertions;

    [TestFixture]
    internal class LayerListPageTest : BlazorUnitTest
    {
        private Mock<ILayerClientService> mockLayerClientService;
        private Mock<IPlanningClientService> mockPlanningClientService;
        private FakeNavigationManager mockNavigationManager;
        public override void Setup()
        {
            base.Setup();

            this.mockLayerClientService = MockRepository.Create<ILayerClientService>();
            this.mockPlanningClientService = MockRepository.Create<IPlanningClientService>();

            _ = Services.AddSingleton(this.mockLayerClientService.Object);
            _ = Services.AddSingleton(this.mockPlanningClientService.Object);
            _ = Services.AddSingleton(new PortalSettings { IsLoRaSupported = true });

            this.mockNavigationManager = Services.GetRequiredService<FakeNavigationManager>();
        }

        [Test]
        public void OnInitializedAsync()
        {
            List<LayerDto> expectedLayers = new List<LayerDto>();

            var expectedLayerDto = Fixture.Create<LayerDto>();
            expectedLayerDto.Father = "Init";

            var expectedLayerDtoChild1 = Fixture.Create<LayerDto>();
            expectedLayerDtoChild1.Father = expectedLayerDto.Id;

            var expectedLayerDtoChild2 = Fixture.Create<LayerDto>();
            expectedLayerDtoChild2.Father = expectedLayerDto.Id;

            expectedLayers.Add(expectedLayerDto);
            expectedLayers.Add(expectedLayerDtoChild1);
            expectedLayers.Add(expectedLayerDtoChild2);

            int expectedPlanningsNumber = 10;
            var expectedPlannings = Fixture.CreateMany<PlanningDto>(expectedPlanningsNumber).ToList();

            _ = this.mockLayerClientService.Setup(service => service.GetLayers())
                .ReturnsAsync(expectedLayers);

            _ = this.mockPlanningClientService.Setup(service => service.GetPlannings())
                .ReturnsAsync(expectedPlannings);

            // Act
            var cut = RenderComponent<LayerListPage>();

            // Assert
            Assert.AreEqual(cut.Instance.MainLayers.Count, 1);
            Assert.AreEqual(cut.Instance.Layers.Count, 1);
            Assert.AreEqual(cut.Instance.Layers.ElementAt(0).LayerData, expectedLayerDto);
            Assert.AreEqual(cut.Instance.Layers.ElementAt(0).Children.ElementAt(0).LayerData, expectedLayerDtoChild1);
            Assert.AreEqual(cut.Instance.Layers.ElementAt(0).Children.ElementAt(1).LayerData, expectedLayerDtoChild2);
            cut.WaitForAssertion(() => MockRepository.VerifyAll());
        }

        [Test]
        public void OnInitializedAsync_Refresh()
        {
            List<LayerDto> expectedLayers = new List<LayerDto>();

            int expectedPlanningsNumber = 10;
            var expectedPlannings = Fixture.CreateMany<PlanningDto>(expectedPlanningsNumber).ToList();

            _ = this.mockLayerClientService.Setup(service => service.GetLayers())
                .ReturnsAsync(expectedLayers);

            _ = this.mockPlanningClientService.Setup(service => service.GetPlannings())
                .ReturnsAsync(expectedPlannings);

            // Act
            var cut = RenderComponent<LayerListPage>();

            var layerListDetailRefresh = cut.WaitForElement("#layerListDetailRefresh");
            layerListDetailRefresh.Click();

            // Assert
            Assert.AreEqual(cut.Instance.MainLayers.Count, 0);
            Assert.AreEqual(cut.Instance.Layers.Count, 0);
            cut.WaitForAssertion(() => MockRepository.VerifyAll());
        }

        [Test]
        public void OnInitializedAsync_ProblemDetailsException()
        {
            int expectedPlanningsNumber = 10;
            var expectedPlannings = Fixture.CreateMany<PlanningDto>(expectedPlanningsNumber).ToList();

            _ = this.mockLayerClientService.Setup(service => service.GetLayers())
                .ThrowsAsync(new ProblemDetailsException(new ProblemDetailsWithExceptionDetails()));

            _ = this.mockPlanningClientService.Setup(service => service.GetPlannings())
                .ReturnsAsync(expectedPlannings);

            // Act
            var cut = RenderComponent<LayerListPage>();

            // Assert
            Assert.AreEqual(cut.Instance.MainLayers.Count, 0);
            Assert.AreEqual(cut.Instance.Layers.Count, 0);
            cut.WaitForAssertion(() => MockRepository.VerifyAll());
        }

        [Test]
        public void OnInitializedAsync_DetailFirstLayer()
        {
            List<LayerDto> expectedLayers = new List<LayerDto>();

            var expectedLayerDto = Fixture.Create<LayerDto>();
            expectedLayerDto.Father = "Init";
            expectedLayers.Add(expectedLayerDto);

            int expectedPlanningsNumber = 10;
            var expectedPlannings = Fixture.CreateMany<PlanningDto>(expectedPlanningsNumber).ToList();

            _ = this.mockLayerClientService.Setup(service => service.GetLayers())
                .ReturnsAsync(expectedLayers);

            _ = this.mockPlanningClientService.Setup(service => service.GetPlannings())
                .ReturnsAsync(expectedPlannings);

            // Act
            var cut = RenderComponent<LayerListPage>();

            var layerListDetailDetail = cut.WaitForElement("#layerListDetailDetail");
            layerListDetailDetail.Click();

            // Assert
            cut.WaitForAssertion(() => this.mockNavigationManager.Uri.Should().EndWith($"/layer/{expectedLayerDto.Id}"));
            cut.WaitForAssertion(() => MockRepository.VerifyAll());
        }

        [Test]
        public void OnInitializedAsync_FirstLayer_AddLayer()
        {
            List<LayerDto> expectedLayers = new List<LayerDto>();

            var expectedLayerDto = Fixture.Create<LayerDto>();
            expectedLayerDto.Father = "Init";
            expectedLayers.Add(expectedLayerDto);

            int expectedPlanningsNumber = 10;
            var expectedPlannings = Fixture.CreateMany<PlanningDto>(expectedPlanningsNumber).ToList();

            _ = this.mockLayerClientService.Setup(service => service.GetLayers())
                .ReturnsAsync(expectedLayers);

            _ = this.mockPlanningClientService.Setup(service => service.GetPlannings())
                .ReturnsAsync(expectedPlannings);

            // Act
            var cut = RenderComponent<LayerListPage>();

            var layerListAddLayer = cut.WaitForElement("#layerListAddLayer");
            layerListAddLayer.Click();

            // Assert
            cut.WaitForAssertion(() => this.mockNavigationManager.Uri.Should().EndWith("/layer/new"));
            cut.WaitForAssertion(() => MockRepository.VerifyAll());
        }

        [Test]
        public void OnInitializedAsync_WrongOrder()
        {
            List<LayerDto> expectedLayers = new List<LayerDto>();

            var expectedLayerDto = Fixture.Create<LayerDto>();
            expectedLayerDto.Father = "Init";

            var expectedLayerDto2 = Fixture.Create<LayerDto>();
            expectedLayerDto2.Father = "Init";

            var expectedLayerDtoChild1 = Fixture.Create<LayerDto>();
            expectedLayerDtoChild1.Father = expectedLayerDto.Id;

            var expectedLayerDtoChild1_0 = Fixture.Create<LayerDto>();
            expectedLayerDtoChild1_0.Father = expectedLayerDtoChild1.Id;

            var expectedLayerDtoChild2 = Fixture.Create<LayerDto>();
            expectedLayerDtoChild2.Father = expectedLayerDto2.Id;

            expectedLayers.Add(expectedLayerDto);
            expectedLayers.Add(expectedLayerDtoChild1);
            expectedLayers.Add(expectedLayerDtoChild1_0);
            expectedLayers.Add(expectedLayerDtoChild2);
            expectedLayers.Add(expectedLayerDto2);

            int expectedPlanningsNumber = 10;
            var expectedPlannings = Fixture.CreateMany<PlanningDto>(expectedPlanningsNumber).ToList();

            _ = this.mockLayerClientService.Setup(service => service.GetLayers())
                .ReturnsAsync(expectedLayers);

            _ = this.mockPlanningClientService.Setup(service => service.GetPlannings())
                .ReturnsAsync(expectedPlannings);

            // Act
            var cut = RenderComponent<LayerListPage>();

            // Assert
            Assert.AreEqual(cut.Instance.MainLayers.Count, 2);
            Assert.AreEqual(cut.Instance.Layers.Count, 2);
            Assert.AreEqual(cut.Instance.Layers.ElementAt(0).LayerData, expectedLayerDto);
            Assert.AreEqual(cut.Instance.Layers.ElementAt(1).LayerData, expectedLayerDto2);
            Assert.AreEqual(cut.Instance.Layers.ElementAt(0).Children.ElementAt(0).LayerData, expectedLayerDtoChild1);
            Assert.AreEqual(cut.Instance.Layers.ElementAt(1).Children.ElementAt(0).LayerData, expectedLayerDtoChild2);
            Assert.AreEqual(cut.Instance.Layers.ElementAt(0).Children.ElementAt(0).Children.ElementAt(0).LayerData, expectedLayerDtoChild1_0);
            cut.WaitForAssertion(() => MockRepository.VerifyAll());
        }
    }
}
