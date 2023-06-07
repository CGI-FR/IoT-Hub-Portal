// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace IoTHub.Portal.Tests.Unit.Infrastructure.Repositories
{
    using System.Linq;
    using System.Threading.Tasks;
    using AutoFixture;
    using IoTHub.Portal.Domain.Entities;
    using IoTHub.Portal.Infrastructure.Repositories;
    using IoTHub.Portal.Tests.Unit.UnitTests.Bases;
    using FluentAssertions;
    using NUnit.Framework;

    public class LabelRepositoryTests : BackendUnitTest
    {
        private LabelRepository labelRepository;

        public override void Setup()
        {
            base.Setup();

            this.labelRepository = new LabelRepository(DbContext);
        }

        [Test]
        public async Task GetAllAsync_ExistingLabels_ReturnsExpectedLabels()
        {
            // Arrange
            var expectedLabels = Fixture.CreateMany<Label>(5).ToList();

            await DbContext.AddRangeAsync(expectedLabels);

            _ = await DbContext.SaveChangesAsync();

            //Act
            var result = await this.labelRepository.GetAllAsync();

            // Assert
            _ = result.Should().BeEquivalentTo(expectedLabels);
        }
    }
}
