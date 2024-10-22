// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace IoTHub.Portal.Tests.Unit.Server.Controllers.v1._0
{
    [TestFixture]
    public class LayersControllerTests : BackendUnitTest
    {
        private Mock<ILayerService> mockLayerService;

        private LayersController layersController;

        public override void Setup()
        {
            base.Setup();

            this.mockLayerService = MockRepository.Create<ILayerService>();

            _ = ServiceCollection.AddSingleton(this.mockLayerService.Object);

            Services = ServiceCollection.BuildServiceProvider();

            this.layersController = new LayersController(Services.GetRequiredService<ILayerService>());
        }

        [Test]
        public async Task GetLayerShouldReturnAValueList()
        {
            // Arrange
            var expectedLayer = Fixture.Create<Layer>();

            _ = this.mockLayerService.Setup(x => x.GetLayers())
                .ReturnsAsync(new List<LayerDto>()
                {
                    new LayerDto()
                });

            // Act
            var response = await this.layersController.GetLayers();

            // Assert
            Assert.IsNotNull(response);
            MockRepository.VerifyAll();
        }

        [Test]
        public async Task GetLayerShouldReturnAValue()
        {
            // Arrange
            var expectedLayer = Fixture.Create<Layer>();

            _ = this.mockLayerService.Setup(service => service.GetLayer(expectedLayer.Id))
                .ReturnsAsync(expectedLayer);

            // Act
            var response = await this.layersController.GetLayer(expectedLayer.Id);

            // Assert
            Assert.IsNotNull(response);
            MockRepository.VerifyAll();
        }

        [Test]
        public async Task GetLayerShouldReturnError()
        {
            // Arrange
            var expectedLayer = Fixture.Create<Layer>();

            _ = this.mockLayerService.Setup(service => service.GetLayer(expectedLayer.Id))
                .ThrowsAsync(new DeviceNotFoundException(""));

            // Act
            var response = await this.layersController.GetLayer(expectedLayer.Id);

            // Assert
            Assert.IsNotNull(response);
            Assert.IsAssignableFrom<NotFoundObjectResult>(response);
            MockRepository.VerifyAll();
        }

        [Test]
        public async Task PostShouldCreateANewEntity()
        {
            // Arrange
            var layer = Fixture.Create<LayerDto>();

            _ = this.mockLayerService.Setup(service => service.CreateLayer(layer))
                .ReturnsAsync(layer);

            // Act
            _ = await this.layersController.CreateLayerAsync(layer);

            // Assert
            MockRepository.VerifyAll();
        }

        [Test]
        public async Task PutShouldUpdateTheLayer()
        {
            // Arrange
            var layer = Fixture.Create<LayerDto>();

            _ = this.mockLayerService.Setup(service => service.UpdateLayer(layer))
                .Returns(Task.CompletedTask);

            // Act
            _ = await this.layersController.UpdateLayer(layer);

            // Assert
            MockRepository.VerifyAll();
        }

        [Test]
        public async Task DeleteShouldRemoveTheEntityCommandsAndAvatar()
        {
            // Arrange
            var layer = Fixture.Create<LayerDto>();

            _ = this.mockLayerService.Setup(service => service.DeleteLayer(layer.Id))
                .Returns(Task.CompletedTask);

            // Act
            _ = await this.layersController.DeleteLayer(layer.Id);

            // Assert
            MockRepository.VerifyAll();
        }
    }
}
