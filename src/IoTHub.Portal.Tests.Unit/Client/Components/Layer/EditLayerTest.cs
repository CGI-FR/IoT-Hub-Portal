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
    using IoTHub.Portal.Client.Components.Layers;
    using IoTHub.Portal.Client.Models;
    using IoTHub.Portal.Client.Services;
    using IoTHub.Portal.Models.v10;
    using IoTHub.Portal.Shared.Models.v10;
    using IoTHub.Portal.Tests.Unit.UnitTests.Bases;
    using Microsoft.Extensions.DependencyInjection;
    using Moq;
    using MudBlazor;
    using NUnit.Framework;

    internal class EditLayerTest : BlazorUnitTest
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
        public void EditLayerOnClickAddLayer()
        {
            var expectedLayerDto = Fixture.Create<LayerDto>();
            expectedLayerDto.Name = "Main Layer";
            expectedLayerDto.Father = "Init";
            HashSet<LayerHash> Layers = new HashSet<LayerHash> { new LayerHash(expectedLayerDto, 1, true) };

            List<LayerDto> InitLayers = new List<LayerDto>();

            // Act
            var cut = RenderComponent<EditLayer>(
                ComponentParameter.CreateParameter("mode", "New"),
                ComponentParameter.CreateParameter("layers", Layers),
                ComponentParameter.CreateParameter("initLayers", new List<LayerDto>() )
            );

            cut.WaitForAssertion(() => cut.FindAll("#editLayerElement").Count.Should().Be(1));
            var editLayerMouseOver = cut.WaitForElement("#editLayerElement");
            editLayerMouseOver.MouseOver();

            var editLayerAddLayers = cut.WaitForElement("#editLayerAddLayer");
            editLayerAddLayers.Click();
            cut.WaitForAssertion(() => cut.FindAll("#editLayerElement").Count.Should().Be(2));

            cut.WaitForAssertion(() => MockRepository.VerifyAll());
        }

        [Test]
        public void EditLayerOnClickDeleteSubChildLayer()
        {
            var expectedLayerDto = Fixture.Create<LayerDto>();
            expectedLayerDto.Name = "Main Layer";
            expectedLayerDto.Father = "Init";

            var expectedChildrenLayerDto1 = Fixture.Create<LayerDto>();
            expectedChildrenLayerDto1.Father = expectedLayerDto.Id;

            var expectedChildrenLayerDto2 = Fixture.Create<LayerDto>();
            expectedChildrenLayerDto2.Father = expectedChildrenLayerDto1.Id;

            HashSet<LayerHash> Layers = new HashSet<LayerHash> { new LayerHash(expectedLayerDto, 1, true) };
            _ = Layers.First().Children.Add(new LayerHash(expectedChildrenLayerDto1, 2, true));
            _ = Layers.First().Children.First().Children.Add(new LayerHash(expectedChildrenLayerDto2, 3, true));

            // Act
            var cut = RenderComponent<EditLayer>(
                ComponentParameter.CreateParameter("mode", "New"),
                ComponentParameter.CreateParameter("layers", Layers),
                ComponentParameter.CreateParameter("initLayers", new List<LayerDto>() )
            );

            cut.WaitForAssertion(() => cut.FindAll("#editLayerElement").Count.Should().Be(3));

            var editLayerMouseOver = cut.FindAll("#editLayerElement");
            editLayerMouseOver[2].MouseOver();

            var editLayerDeleteLayer = cut.WaitForElement("#editLayerDeleteLayer");
            editLayerDeleteLayer.Click();

            cut.WaitForAssertion(() => cut.FindAll("#editLayerElement").Count.Should().Be(2));

            cut.WaitForAssertion(() => MockRepository.VerifyAll());
        }

        [Test]
        public void EditLayerOnClickDeleteChildLayer()
        {
            var expectedLayerDto = new LayerDto()
            {
                Id = Guid.NewGuid().ToString(),
                Name = "Main Layer",
                Father = "Init"
            };

            var expectedChildrenLayerDto1 = Fixture.Create<LayerDto>();
            expectedChildrenLayerDto1.Father = expectedLayerDto.Id;

            var expectedChildrenLayerDto2 = Fixture.Create<LayerDto>();
            expectedChildrenLayerDto2.Father = expectedChildrenLayerDto1.Id;

            HashSet<LayerHash> Layers = new HashSet<LayerHash> { new LayerHash(expectedLayerDto, 1, true) };
            _ = Layers.First().Children.Add(new LayerHash(expectedChildrenLayerDto1, 2, true));
            _ = Layers.First().Children.First().Children.Add(new LayerHash(expectedChildrenLayerDto2, 3, true));

            // Act
            var cut = RenderComponent<EditLayer>(
                ComponentParameter.CreateParameter("mode", "New"),
                ComponentParameter.CreateParameter("layers", Layers),
                ComponentParameter.CreateParameter("initLayers", new List<LayerDto>() )
            );

            cut.WaitForAssertion(() => cut.FindAll("#editLayerElement").Count.Should().Be(3));

            var editLayerMouseOver = cut.FindAll("#editLayerElement");
            editLayerMouseOver[1].MouseOver();

            var editLayerDeleteLayer = cut.WaitForElement("#editLayerDeleteLayer");
            editLayerDeleteLayer.Click();

            cut.WaitForAssertion(() => cut.FindAll("#editLayerElement").Count.Should().Be(1));

            cut.WaitForAssertion(() => MockRepository.VerifyAll());
        }

        [Test]
        public async Task EditLayerOnClickEditLayerAsync()
        {
            var expectedRename = Fixture.Create<string>();

            var expectedLayerDto = new LayerDto()
            {
                Id = Guid.NewGuid().ToString(),
                Name = "Main Layer",
                Father = "Init"
            };
            HashSet<LayerHash> Layers = new HashSet<LayerHash> { new LayerHash(expectedLayerDto, 1, true) };

            List<LayerDto> InitLayers = new List<LayerDto>();

            // Act
            var cut = RenderComponent<EditLayer>(
                ComponentParameter.CreateParameter("mode", "New"),
                ComponentParameter.CreateParameter("layers", Layers),
                ComponentParameter.CreateParameter("initLayers", new List<LayerDto>() )
            );

            var editLayerMouseOver = cut.WaitForElement("#editLayerElement");
            editLayerMouseOver.MouseOver();

            var editLayerEditLayer = cut.WaitForElement("#editLayerEditLayer");
            editLayerEditLayer.Click();

            var titleField = cut.FindComponents<MudTextField<string>>()[0];
            await cut.InvokeAsync(() => titleField.Instance.SetText(expectedRename));

            Assert.AreEqual(cut.Instance.layers.First().LayerData.Name, expectedRename);
            cut.WaitForAssertion(() => MockRepository.VerifyAll());
        }

        [Test]
        public async Task EditLayerOnClickCreateSaveLayerAsync()
        {
            var expectedLayerId = Fixture.Create<string>();

            var expectedLayerDto = new LayerDto()
            {
                Id = Guid.NewGuid().ToString(),
                Name = "Main Layer",
                Father = "Init"
            };
            HashSet<LayerHash> Layers = new HashSet<LayerHash> { new LayerHash(expectedLayerDto, 1, true) };

            _ = this.mockLayerClientService.Setup(service => service.CreateLayer(It.IsAny<LayerDto>()))
                .ReturnsAsync(expectedLayerId);

            // Act
            var cut = RenderComponent<EditLayer>(
                ComponentParameter.CreateParameter("mode", "New"),
                ComponentParameter.CreateParameter("layers", Layers),
                ComponentParameter.CreateParameter("initLayers", new List<LayerDto>() )
            );

            var editLayerMouseOver = cut.WaitForElement("#editLayerElement");
            editLayerMouseOver.MouseOver();

            var editLayerAddLayers = cut.WaitForElement("#editLayerAddLayer");
            editLayerAddLayers.Click();

            var editLayerSaveLayers = cut.WaitForElement("#editLayerSaveLayer");
            editLayerSaveLayers.Click();

            cut.WaitForAssertion(() => this.mockNavigationManager.Uri.Should().EndWith("/layer"));
            cut.WaitForAssertion(() => MockRepository.VerifyAll());
        }

        [Test]
        public async Task EditLayerOnModifyClickSaveLayerAsync()
        {
            var expectedLayerId = Fixture.Create<string>();

            var expectedLayerDto = new LayerDto()
            {
                Id = Guid.NewGuid().ToString(),
                Name = "Main Layer",
                Father = "Init"
            };
            HashSet<LayerHash> Layers = new HashSet<LayerHash> { new LayerHash(expectedLayerDto, 1, true) };

            var expectedLayerDtoChild1 = Fixture.Create<LayerDto>();
            expectedLayerDtoChild1.Name = "Main Layer";
            expectedLayerDtoChild1.Father = "Init";
            _ = Layers.First().Children.Add(new LayerHash(expectedLayerDtoChild1, 2, true));

            var expectedLayerDtoChild2 = Fixture.Create<LayerDto>();
            expectedLayerDtoChild2.Name = "Main Layer";
            expectedLayerDtoChild2.Father = "Init";
            _ = Layers.First().Children.Add(new LayerHash(expectedLayerDtoChild2, 2, true));

            List<LayerDto> InitLayers = new List<LayerDto> { expectedLayerDto, expectedLayerDtoChild1, expectedLayerDtoChild2 };

            _ = this.mockLayerClientService.Setup(service => service.UpdateLayer(It.IsAny<LayerDto>()))
                .Returns(Task.CompletedTask);

            _ = this.mockLayerClientService.Setup(service => service.DeleteLayer(It.IsAny<string>()))
                .Returns(Task.CompletedTask);

            // Act
            var cut = RenderComponent<EditLayer>(
                ComponentParameter.CreateParameter("mode", "New"),
                ComponentParameter.CreateParameter("layers", Layers),
                ComponentParameter.CreateParameter("initLayers", InitLayers )
            );

            var editLayerMouseOvers = cut.WaitForElements("#editLayerElement");
            editLayerMouseOvers[1].MouseOver();

            var editLayerAddLayers = cut.WaitForElement("#editLayerDeleteLayer");
            editLayerAddLayers.Click();

            var editLayerSaveLayers = cut.WaitForElement("#editLayerSaveLayer");
            editLayerSaveLayers.Click();

            cut.WaitForAssertion(() => this.mockNavigationManager.Uri.Should().EndWith("/layer"));
            cut.WaitForAssertion(() => MockRepository.VerifyAll());
        }

        [Test]
        public async Task EditLayerOnDeviceClickSaveLayerAsync()
        {
            var expectedLayerId = Fixture.Create<string>();

            var expectedLayerDto = Fixture.Create<LayerDto>();
            expectedLayerDto.Name = "Main Layer";
            expectedLayerDto.Father = "Init";
            HashSet<LayerHash> Layers = new HashSet<LayerHash> { new LayerHash(expectedLayerDto, 1, true) };

            _ = this.mockLayerClientService.Setup(service => service.CreateLayer(It.IsAny<LayerDto>()))
                .ReturnsAsync(expectedLayerId);

            // Act
            var cut = RenderComponent<EditLayer>(
                ComponentParameter.CreateParameter("mode", "New"),
                ComponentParameter.CreateParameter("layers", Layers),
                ComponentParameter.CreateParameter("initLayers", new List<LayerDto>() )
            );

            var editLayerMouseOver = cut.WaitForElement("#editLayerElement");
            editLayerMouseOver.MouseOver();

            var editLayerDevice = cut.WaitForElement("#editLayerDevice");
            editLayerDevice.Click();

            var editLayerSaveLayers = cut.WaitForElement("#editLayerSaveLayer");
            editLayerSaveLayers.Click();

            cut.WaitForAssertion(() => this.mockNavigationManager.Uri.Should().EndWith("/layer"));
            cut.WaitForAssertion(() => MockRepository.VerifyAll());
        }
    }
}
