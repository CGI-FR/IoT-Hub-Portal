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

    public class ThingTypeTagRepositoryTests : BackendUnitTest
    {
        private ThingTypeTagRepository thingTypeTagRepository;

        public override void Setup()
        {
            base.Setup();

            this.thingTypeTagRepository = new ThingTypeTagRepository(DbContext);
        }

        [Test]
        public async Task GetAllShouldReturnExpectedThingTypelCommands()
        {
            // Arrange
            var expectedThingTypeTag = Fixture.CreateMany<ThingTypeTag>(5).ToList();

            await DbContext.AddRangeAsync(expectedThingTypeTag);

            _ = await DbContext.SaveChangesAsync();

            // Act
            var result = this.thingTypeTagRepository.GetAll().ToList();

            // Assert
            _ = result.Should().BeEquivalentTo(expectedThingTypeTag);
        }
    }
}
