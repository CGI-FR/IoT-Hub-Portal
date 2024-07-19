// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace IoTHub.Portal.Tests.Unit.Infrastructure.Repositories
{
    using System.Linq;
    using System.Threading.Tasks;
    using IoTHub.Portal.Domain.Entities;
    using IoTHub.Portal.Infrastructure.Repositories;
    using IoTHub.Portal.Tests.Unit.UnitTests.Bases;
    using FluentAssertions;
    using NUnit.Framework;
    using AutoFixture;

    public class GroupRepositoryTest : BackendUnitTest
    {
        private GroupRepository groupRepository;

        public override void Setup()
        {
            base.Setup();

            Fixture.Behaviors.OfType<ThrowingRecursionBehavior>().ToList()
                .ForEach(b => Fixture.Behaviors.Remove(b));
            Fixture.Behaviors.Add(new OmitOnRecursionBehavior());

            this.groupRepository = new GroupRepository(DbContext);
        }

        [Test]
        public async Task GetAllShouldReturnExpectedGroups()
        {
            // Arrange
            var expectedGroups = Fixture.CreateMany<Group>(2).ToList();

            await DbContext.AddRangeAsync(expectedGroups);
            _ = await DbContext.SaveChangesAsync();

            //Act
            var result = this.groupRepository.GetAll().ToList();

            // Assert
            _ = result.Should().BeEquivalentTo(expectedGroups);
        }

        [Test]
        public async Task GetByIdAsync_ExistingGroup_ReturnsExpectedGroup()
        {
            // Arrange
            var expectedGroup = Fixture.Create<Group>();

            _ = DbContext.Add(expectedGroup);
            _ = await DbContext.SaveChangesAsync();

            //Act
            var result = await this.groupRepository.GetByIdAsync(expectedGroup.Id);

            // Assert
            _ = result.Should().BeEquivalentTo(expectedGroup);
        }
    }
}
