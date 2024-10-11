// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace IoTHub.Portal.Tests.Unit.Infrastructure.Repositories
{
    using Device = Portal.Domain.Entities.Device;

    public class DeviceRepositoryTest : BackendUnitTest
    {
        private DeviceRepository deviceRepository;

        public override void Setup()
        {
            base.Setup();

            this.deviceRepository = new DeviceRepository(DbContext);
        }

        [Test]
        public async Task GetAllShouldReturnExpectedDevices()
        {
            // Arrange
            var expectedDevices = Fixture.CreateMany<Device>(5).ToList();

            await DbContext.AddRangeAsync(expectedDevices);

            _ = await DbContext.SaveChangesAsync();

            //Act
            var result = this.deviceRepository.GetAll().ToList();

            // Assert
            _ = result.Should().BeEquivalentTo(expectedDevices);
        }

        [Test]
        public async Task GetByIdAsync_ExistingDevice_ReturnsExpectedDevice()
        {
            // Arrange
            var expectedDevice = Fixture.Create<Device>();

            _ = DbContext.Add(expectedDevice);

            _ = await DbContext.SaveChangesAsync();

            //Act
            var result = await this.deviceRepository.GetByIdAsync(expectedDevice.Id);

            // Assert
            _ = result.Should().BeEquivalentTo(expectedDevice);
        }
    }
}
