// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace AzureIoTHub.Portal.Tests.Unit.Infrastructure.Repositories.AWS
{
    using System.Linq;
    using System.Threading.Tasks;
    using AutoFixture;
    using AzureIoTHub.Portal.Domain.Entities.AWS;
    using AzureIoTHub.Portal.Infrastructure.Repositories.AWS;
    using AzureIoTHub.Portal.Tests.Unit.UnitTests.Bases;
    using FluentAssertions;
    using NUnit.Framework;

    public class ThingTypeSearchableAttributesRepositoryTests : BackendUnitTest
    {
        private ThingTypeSearchableAttributeRepository thingTypeSearcahbleAttrRepository;

        public override void Setup()
        {
            base.Setup();

            this.thingTypeSearcahbleAttrRepository = new ThingTypeSearchableAttributeRepository(DbContext);
        }

        [Test]
        public async Task GetAllShouldReturnExpectedThingTypelCommands()
        {
            // Arrange
            var expectedThingTypeSearchableAttr = Fixture.CreateMany<ThingTypeSearchableAtt>(5).ToList();

            await DbContext.AddRangeAsync(expectedThingTypeSearchableAttr);

            _ = await DbContext.SaveChangesAsync();

            // Act
            var result = this.thingTypeSearcahbleAttrRepository.GetAll().ToList();

            // Assert
            _ = result.Should().BeEquivalentTo(expectedThingTypeSearchableAttr);
        }
    }
}
