// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace IoTHub.Portal.Tests.Unit.Client.Components.Layer
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using AutoFixture;
    using Bunit;
    using Bunit.TestDoubles;
    using FluentAssertions;
    using IoTHub.Portal.Client.Pages.Layer;
    using IoTHub.Portal.Client.Services;
    using IoTHub.Portal.Models.v10;
    using IoTHub.Portal.Shared.Models.v10;
    using IoTHub.Portal.Tests.Unit.UnitTests.Bases;
    using Microsoft.Extensions.DependencyInjection;
    using Moq;
    using NUnit.Framework;

    internal class LayerListPageTest : BlazorUnitTest
    {
        private Mock<ILayerClientService> mockLayerClientService;
        private FakeNavigationManager mockNavigationManager;

        public override void Setup()
        {
            base.Setup();

            this.mockLayerClientService = MockRepository.Create<ILayerClientService>();

            _ = Services.AddSingleton(this.mockLayerClientService.Object);
            _ = Services.AddSingleton(new PortalSettings { IsLoRaSupported = true });

            this.mockNavigationManager = Services.GetRequiredService<FakeNavigationManager>();
        }

        [Test]
        public void LayerListPageOnClickAddLayer()
        {
            var expectedLayerDto = Fixture.Create<LayerDto>();
            expectedLayerDto.Name = "Main Layer";
            expectedLayerDto.Father = null;

            var expectedLayerDtoChild = Fixture.Create<LayerDto>();

            _ = this.mockLayerClientService.Setup(service => service.GetLayers())
                .ReturnsAsync(new List<LayerDto>(new List<LayerDto> { expectedLayerDto }));

            _ = this.mockLayerClientService.Setup(service => service.CreateLayer(It.IsAny<LayerDto>()))
                .ReturnsAsync(expectedLayerDtoChild.Id);

            // Act
            var cut = RenderComponent<LayerListPage>();

            cut.WaitForAssertion(() => cut.FindAll("#editLayerElement").Count.Should().Be(1));
            var editLayerMouseOver = cut.WaitForElement("#editLayerElement");
            editLayerMouseOver.MouseOver();

            var editLayerAddLayers = cut.WaitForElement("#editLayerAddLayer");
            editLayerAddLayers.Click();
            cut.WaitForAssertion(() => cut.FindAll("#editLayerElement").Count.Should().Be(2));

            cut.WaitForAssertion(() => MockRepository.VerifyAll());
        }

        [Test]
        public void LayerListPageOnClickDeleteSubChildLayer()
        {
            var expectedLayerDto = Fixture.Create<LayerDto>();
            expectedLayerDto.Name = "Main Layer";
            expectedLayerDto.Father = null;

            var expectedChildrenLayerDto1 = Fixture.Create<LayerDto>();
            expectedChildrenLayerDto1.Father = expectedLayerDto.Id;

            var expectedChildrenLayerDto2 = Fixture.Create<LayerDto>();
            expectedChildrenLayerDto2.Father = expectedChildrenLayerDto1.Id;

            _ = this.mockLayerClientService.Setup(service => service.GetLayers())
                .ReturnsAsync(new List<LayerDto>(new List<LayerDto> { expectedLayerDto, expectedChildrenLayerDto1, expectedChildrenLayerDto2 }));

            _ = this.mockLayerClientService.Setup(service => service.DeleteLayer(It.IsAny<string>()))
                .Returns(Task.CompletedTask);

            // Act
            var cut = RenderComponent<LayerListPage>();

            cut.WaitForAssertion(() => cut.FindAll("#editLayerElement").Count.Should().Be(3));

            var editLayerMouseOver = cut.FindAll("#editLayerElement");
            editLayerMouseOver[2].MouseOver();

            var editLayerDeleteLayer = cut.WaitForElement("#editLayerDeleteLayer");
            editLayerDeleteLayer.Click();

            cut.WaitForAssertion(() => cut.FindAll("#editLayerElement").Count.Should().Be(2));

            cut.WaitForAssertion(() => MockRepository.VerifyAll());
        }

        [Test]
        public void LayerListPageOnClickDeleteChildLayer()
        {
            var expectedLayerDto = new LayerDto()
            {
                Id = Guid.NewGuid().ToString(),
                Name = "Main Layer",
                Father = null
            };

            var expectedChildrenLayerDto1 = Fixture.Create<LayerDto>();
            expectedChildrenLayerDto1.Father = expectedLayerDto.Id;

            var expectedChildrenLayerDto2 = Fixture.Create<LayerDto>();
            expectedChildrenLayerDto2.Father = expectedChildrenLayerDto1.Id;

            _ = this.mockLayerClientService.Setup(service => service.GetLayers())
                .ReturnsAsync(new List<LayerDto>(new List<LayerDto> { expectedLayerDto, expectedChildrenLayerDto1, expectedChildrenLayerDto2 }));

            _ = this.mockLayerClientService.Setup(service => service.DeleteLayer(It.IsAny<string>()))
                .Returns(Task.CompletedTask);

            // Act
            var cut = RenderComponent<LayerListPage>();

            cut.WaitForAssertion(() => cut.FindAll("#editLayerElement").Count.Should().Be(3));

            var editLayerMouseOver = cut.FindAll("#editLayerElement");
            editLayerMouseOver[1].MouseOver();

            var editLayerDeleteLayer = cut.WaitForElement("#editLayerDeleteLayer");
            editLayerDeleteLayer.Click();

            cut.WaitForAssertion(() => cut.FindAll("#editLayerElement").Count.Should().Be(1));

            cut.WaitForAssertion(() => MockRepository.VerifyAll());
        }
    }
}
