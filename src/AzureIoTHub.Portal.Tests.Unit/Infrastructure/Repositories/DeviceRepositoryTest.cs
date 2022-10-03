// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace AzureIoTHub.Portal.Tests.Unit.Infrastructure.Repositories
{
    using System.Linq;
    using System.Threading.Tasks;
    using AutoFixture;
    using AzureIoTHub.Portal.Domain.Entities;
    using AzureIoTHub.Portal.Infrastructure.Repositories;
    using AzureIoTHub.Portal.Tests.Unit.UnitTests.Bases;
    using FluentAssertions;
    using NUnit.Framework;

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
