// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace IoTHub.Portal.Tests.Unit.Infrastructure.Repositories
{
    public class LorawanDeviceRepositoryTest : BackendUnitTest
    {
        private LorawanDeviceRepository lorawanDeviceRepository;

        public override void Setup()
        {
            base.Setup();

            this.lorawanDeviceRepository = new LorawanDeviceRepository(DbContext);
        }

        [Test]
        public async Task GetAllShouldReturnExpectedDevices()
        {
            // Arrange
            var expectedDevices = Fixture.CreateMany<LorawanDevice>(5).ToList();

            await DbContext.AddRangeAsync(expectedDevices);

            _ = await DbContext.SaveChangesAsync();

            //Act
            var result = this.lorawanDeviceRepository.GetAll().ToList();

            // Assert
            _ = result.Should().BeEquivalentTo(expectedDevices);
        }

        [Test]
        public async Task GetByIdAsync_ExistingDevice_ReturnsExpectedDevice()
        {
            // Arrange
            var expectedDevice = Fixture.Create<LorawanDevice>();

            _ = DbContext.Add(expectedDevice);

            _ = await DbContext.SaveChangesAsync();

            //Act
            var result = await this.lorawanDeviceRepository.GetByIdAsync(expectedDevice.Id);

            // Assert
            _ = result.Should().BeEquivalentTo(expectedDevice);
        }
    }
}
