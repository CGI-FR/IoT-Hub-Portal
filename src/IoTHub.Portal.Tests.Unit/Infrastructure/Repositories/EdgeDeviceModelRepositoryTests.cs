// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace IoTHub.Portal.Tests.Unit.Infrastructure.Repositories
{
    using IoTHub.Portal.Infrastructure.Repositories;

    public class EdgeDeviceModelRepositoryTests : BackendUnitTest
    {
        private EdgeDeviceModelRepository edgeDeviceModelRepository;

        public override void Setup()
        {
            base.Setup();

            this.edgeDeviceModelRepository = new EdgeDeviceModelRepository(DbContext);
        }

        [Test]
        public async Task GetAll_ExitingEdgeDeviceModels_EdgeDeviceModelsReturned()
        {
            // Arrange
            var expectedEdgeDeviceModels = Fixture.CreateMany<EdgeDeviceModel>(5).ToList();

            await DbContext.AddRangeAsync(expectedEdgeDeviceModels);

            _ = await DbContext.SaveChangesAsync();

            // Act
            var result = this.edgeDeviceModelRepository.GetAll().ToList();

            // Assert
            _ = result.Should().BeEquivalentTo(expectedEdgeDeviceModels);
        }

        [Test]
        public async Task GetByNameShouldReturnExpectedEdgeDeviceModel()
        {
            // Arrange
            var edgeDeviceModels = Fixture.CreateMany<EdgeDeviceModel>(5).ToList();
            var searchEdgeDeviceModel = Fixture.Create<EdgeDeviceModel>();
            edgeDeviceModels.Add(searchEdgeDeviceModel);

            await DbContext.AddRangeAsync(edgeDeviceModels);

            _ = await DbContext.SaveChangesAsync();

            // Act
            var result = await this.edgeDeviceModelRepository.GetByNameAsync(searchEdgeDeviceModel.Name);

            // Assert
            _ = result.Should().BeEquivalentTo(searchEdgeDeviceModel);
        }
    }
}
