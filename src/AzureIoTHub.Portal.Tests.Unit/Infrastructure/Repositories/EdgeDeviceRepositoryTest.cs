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

    public class EdgeDeviceRepositoryTest : BackendUnitTest
    {
        private EdgeDeviceRepository edgeDeviceRepository;

        public override void Setup()
        {
            base.Setup();

            this.edgeDeviceRepository = new EdgeDeviceRepository(DbContext);
        }

        [Test]
        public async Task GetAllShouldReturnExpectedEdgeDevices()
        {
            // Arrange
            var expectedEdgeDevices = Fixture.CreateMany<EdgeDevice>(5).ToList();

            await DbContext.AddRangeAsync(expectedEdgeDevices);

            _ = await DbContext.SaveChangesAsync();

            //Act
            var result = this.edgeDeviceRepository.GetAll().ToList();

            // Assert
            _ = result.Should().BeEquivalentTo(expectedEdgeDevices);
        }

        [Test]
        public async Task GetByIdAsyncExistingDeviceReturnsExpectedEdgeDevice()
        {
            // Arrange
            var expectedEdgeDevices = Fixture.Create<EdgeDevice>();

            _ = DbContext.Add(expectedEdgeDevices);

            _ = await DbContext.SaveChangesAsync();

            //Act
            var result = await this.edgeDeviceRepository.GetByIdAsync(expectedEdgeDevices.Id);

            // Assert
            _ = result.Should().BeEquivalentTo(expectedEdgeDevices);
        }
    }
}
