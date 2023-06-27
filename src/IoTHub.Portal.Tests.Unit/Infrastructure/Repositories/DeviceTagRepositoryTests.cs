// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace IoTHub.Portal.Tests.Unit.Infrastructure.Repositories
{
    using System.Linq;
    using System.Threading.Tasks;
    using AutoFixture;
    using FluentAssertions;
    using IoTHub.Portal.Domain.Entities;
    using IoTHub.Portal.Infrastructure.Common.Repositories;
    using NUnit.Framework;
    using UnitTests.Bases;

    public class DeviceTagRepositoryTests : BackendUnitTest
    {
        private DeviceTagRepository deviceTagRepository;

        public override void Setup()
        {
            base.Setup();

            this.deviceTagRepository = new DeviceTagRepository(DbContext);
        }

        [Test]
        public async Task GetAllShouldReturnExpectedDeviceTags()
        {
            // Arrange
            var expectedDeviceTags = Fixture.CreateMany<DeviceTag>(5).ToList();

            await DbContext.AddRangeAsync(expectedDeviceTags);

            _ = await DbContext.SaveChangesAsync();

            // Act
            var result = this.deviceTagRepository.GetAll().ToList();

            // Assert
            _ = result.Should().BeEquivalentTo(expectedDeviceTags);
        }
    }
}
