// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace AzureIoTHub.Portal.Tests.Unit.Infrastructure.Repositories
{
    using System.Linq;
    using System.Threading.Tasks;
    using AutoFixture;
    using AzureIoTHub.Portal.Domain.Entities;
    using AzureIoTHub.Portal.Infrastructure.Repositories;
    using FluentAssertions;
    using UnitTests.Bases;
    using NUnit.Framework;

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
    }
}
