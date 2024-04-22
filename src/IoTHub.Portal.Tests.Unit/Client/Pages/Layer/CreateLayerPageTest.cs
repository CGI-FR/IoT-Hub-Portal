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

    [TestFixture]
    internal class CreateLayerPageTest : BlazorUnitTest
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
        public void OnInitializedAsync_CreateLayerPage()
        {
            // Act
            var cut = RenderComponent<CreateLayerPage>();

            // Assert
            Assert.AreEqual(cut.Instance.InitLayers.Count, 0);
            Assert.AreEqual(cut.Instance.Layers.Count, 1);
            Assert.AreEqual(cut.Instance.Layers.First().LayerData.Name, "Main Layer");
            Assert.AreEqual(cut.Instance.Layers.First().LayerData.Father, "Init");
            cut.WaitForAssertion(() => MockRepository.VerifyAll());
        }
    }
}
