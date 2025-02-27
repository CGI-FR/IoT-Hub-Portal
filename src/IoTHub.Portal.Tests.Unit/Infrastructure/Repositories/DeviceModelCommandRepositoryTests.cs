// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace IoTHub.Portal.Tests.Unit.Infrastructure.Repositories
{
    public class DeviceModelCommandRepositoryTests : BackendUnitTest
    {
        private DeviceModelCommandRepository deviceModelCommandRepository;

        public override void Setup()
        {
            base.Setup();

            this.deviceModelCommandRepository = new DeviceModelCommandRepository(DbContext);
        }

        [Test]
        public async Task GetAllShouldReturnExpectedDeviceModelCommands()
        {
            // Arrange
            var expectedDeviceModelCommands = Fixture.CreateMany<DeviceModelCommand>(5).ToList();

            await DbContext.AddRangeAsync(expectedDeviceModelCommands);

            _ = await DbContext.SaveChangesAsync();

            // Act
            var result = this.deviceModelCommandRepository.GetAll().ToList();

            // Assert
            _ = result.Should().BeEquivalentTo(expectedDeviceModelCommands);
        }
    }
}
