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
    using AutoFixture;
    using IoTHub.Portal.Shared.Models.v10;
    using System.Collections.Generic;

    [TestFixture]
    internal class LayerDetailsPageTest : BlazorUnitTest
    {
        private Mock<ILayerClientService> mockLayerClientService;
        public override void Setup()
        {
            base.Setup();

            this.mockLayerClientService = MockRepository.Create<ILayerClientService>();

            _ = Services.AddSingleton(this.mockLayerClientService.Object);
            _ = Services.AddSingleton(new PortalSettings { IsLoRaSupported = true });
        }

        [Test]
        public void OnInitializedAsync_LayerDetailsPage_1Layer()
        {
            var expectedLayers = Fixture.CreateMany<LayerDto>(1).ToList();
            expectedLayers[0].Father = "Init";

            _ = this.mockLayerClientService.Setup(service => service.GetLayers())
                .ReturnsAsync(new List<LayerDto>(expectedLayers));

            // Act
            var cut = RenderComponent<LayerDetailsPage>(ComponentParameter.CreateParameter("LayerId", expectedLayers[0].Id));

            // Assert
            Assert.AreEqual(cut.Instance.InitLayers.Count, 1);
            Assert.AreEqual(cut.Instance.InitLayers[0].Name, expectedLayers[0].Name);
            Assert.AreEqual(cut.Instance.InitLayers[0].Father, expectedLayers[0].Father);

            Assert.AreEqual(cut.Instance.Layers.Count, 1);
            Assert.AreEqual(cut.Instance.Layers.First().LayerData.Name, expectedLayers[0].Name);
            Assert.AreEqual(cut.Instance.Layers.First().LayerData.Father, expectedLayers[0].Father);

            cut.WaitForAssertion(() => MockRepository.VerifyAll());
        }

        [Test]
        public void OnInitializedAsync_LayerDetailsPage_2LayersInWrongOrder()
        {
            var expectedLayers = Fixture.CreateMany<LayerDto>(3).ToList();
            expectedLayers[1].Father = "Init";
            expectedLayers[0].Father = expectedLayers[1].Id;
            expectedLayers[2].Father = expectedLayers[0].Id;

            _ = this.mockLayerClientService.Setup(service => service.GetLayers())
                .ReturnsAsync(expectedLayers);

            // Act
            var cut = RenderComponent<LayerDetailsPage>(ComponentParameter.CreateParameter("LayerId", expectedLayers[1].Id));

            // Assert
            Assert.AreEqual(cut.Instance.InitLayers.Count, 3);

            Assert.AreEqual(cut.Instance.Layers.Count, 1);

            var children = cut.Instance.Layers.First().Children;
            Assert.AreEqual(children.First().LayerData.Father, cut.Instance.Layers.First().LayerData.Id);
            Assert.AreEqual(children.First().Children.Count, 1);

            var childrenOfChildre = children.First().Children;
            Assert.AreEqual(childrenOfChildre.First().LayerData.Father, children.First().LayerData.Id);
            Assert.AreEqual(childrenOfChildre.First().Children.Count, 0);

            cut.WaitForAssertion(() => MockRepository.VerifyAll());
        }
    }
}
