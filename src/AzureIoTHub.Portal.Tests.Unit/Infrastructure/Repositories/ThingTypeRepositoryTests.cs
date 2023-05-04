// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace AzureIoTHub.Portal.Tests.Unit.Infrastructure.Repositories
{
    using System.Linq;
    using System.Threading.Tasks;
    using AutoFixture;
    using AzureIoTHub.Portal.Domain.Entities.AWS;
    using AzureIoTHub.Portal.Infrastructure.Repositories;
    using AzureIoTHub.Portal.Tests.Unit.UnitTests.Bases;
    using FluentAssertions;
    using NUnit.Framework;

    public class ThingTypeRepositoryTests : BackendUnitTest
    {
        private ThingTypeRepository thingTypeRepository;

        public override void Setup()
        {
            base.Setup();

            this.thingTypeRepository = new ThingTypeRepository(DbContext);
        }

        [Test]
        public async Task GetAllShouldReturnExpectedThingTypelCommands()
        {
            // Arrange
            var expectedThingType = Fixture.CreateMany<ThingType>(5).ToList();

            await DbContext.AddRangeAsync(expectedThingType);

            _ = await DbContext.SaveChangesAsync();

            // Act
            var result = this.thingTypeRepository.GetAll().ToList();

            // Assert
            _ = result.Should().BeEquivalentTo(expectedThingType);
        }
    }
}
