// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace IoTHub.Portal.Tests.Unit.Infrastructure.Repositories
{
    public class LayerRepositoryTests : BackendUnitTest
    {
        private LayerRepository layerRepository;

        public override void Setup()
        {
            base.Setup();

            this.layerRepository = new LayerRepository(DbContext);
        }

        [Test]
        public async Task GetByNameShouldReturnExpectedLayer()
        {
            // Arrange
            var layers = Fixture.CreateMany<Layer>(5).ToList();
            var searchLayer = Fixture.Create<Layer>();
            layers.Add(searchLayer);

            await DbContext.AddRangeAsync(layers);

            _ = await DbContext.SaveChangesAsync();

            // Act
            var result = await this.layerRepository.GetByNameAsync(searchLayer.Id);

            // Assert
            _ = result.Should().BeEquivalentTo(searchLayer);
        }
    }
}
