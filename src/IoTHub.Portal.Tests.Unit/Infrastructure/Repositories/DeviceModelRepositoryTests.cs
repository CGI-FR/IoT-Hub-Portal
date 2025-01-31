// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace IoTHub.Portal.Tests.Unit.Infrastructure.Repositories
{
    public class DeviceModelRepositoryTests : BackendUnitTest
    {
        private DeviceModelRepository deviceModelRepository;

        public override void Setup()
        {
            base.Setup();

            this.deviceModelRepository = new DeviceModelRepository(DbContext);
        }

        [Test]
        public async Task GetAllShouldReturnExpectedDeviceModelCommands()
        {
            // Arrange
            var expectedDeviceModels = Fixture.CreateMany<DeviceModel>(5).ToList();

            await DbContext.AddRangeAsync(expectedDeviceModels);

            _ = await DbContext.SaveChangesAsync();

            // Act
            var result = this.deviceModelRepository.GetAll().ToList();

            // Assert
            _ = result.Should().BeEquivalentTo(expectedDeviceModels);
        }

        [Test]
        public async Task GetByNameShouldReturnExpectedDeviceModel()
        {
            // Arrange
            var deviceModels = Fixture.CreateMany<DeviceModel>(5).ToList();
            var searchDeviceModel = Fixture.Create<DeviceModel>();
            deviceModels.Add(searchDeviceModel);

            await DbContext.AddRangeAsync(deviceModels);

            _ = await DbContext.SaveChangesAsync();

            // Act
            var result = await this.deviceModelRepository.GetByNameAsync(searchDeviceModel.Name);

            // Assert
            _ = result.Should().BeEquivalentTo(searchDeviceModel);
        }
    }
}
