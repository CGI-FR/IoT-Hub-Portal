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
    using IoTHub.Portal.Tests.Unit.UnitTests.Bases;
    using NUnit.Framework;

    public class LoRaDeviceTelemetryRepositoryTests : BackendUnitTest
    {
        private LoRaDeviceTelemetryRepository loRaDeviceTelemetryRepository;

        public override void Setup()
        {
            base.Setup();

            this.loRaDeviceTelemetryRepository = new LoRaDeviceTelemetryRepository(DbContext);
        }

        [Test]
        public async Task GetAllAsync_ExistingLoRaDeviceTelemetry_ReturnsExpectedLoRaDeviceTelemetry()
        {
            // Arrange
            var expectedLoRaDeviceTelemetry = Fixture.CreateMany<LoRaDeviceTelemetry>(5).ToList();

            await DbContext.AddRangeAsync(expectedLoRaDeviceTelemetry);

            _ = await DbContext.SaveChangesAsync();

            //Act
            var result = await this.loRaDeviceTelemetryRepository.GetAllAsync();

            // Assert
            _ = result.Should().BeEquivalentTo(expectedLoRaDeviceTelemetry);
        }
    }
}
