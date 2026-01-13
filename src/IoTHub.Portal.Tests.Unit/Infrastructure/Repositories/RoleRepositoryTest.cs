// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace IoTHub.Portal.Tests.Unit.Infrastructure.Repositories
{
    public class RoleRepositoryTest : BackendUnitTest
    {
        private RoleRepository roleRepository;

        public override void Setup()
        {
            base.Setup();

            this.roleRepository = new RoleRepository(DbContext);
        }

        [Test]
        public async Task GetAllShouldReturnExpectedRoles()
        {
            // Arrange
            var expectedRoles = Fixture.CreateMany<Role>(5).ToList();

            await DbContext.AddRangeAsync(expectedRoles);

            _ = await DbContext.SaveChangesAsync();

            //Act
            var result = this.roleRepository.GetAll().ToList();

            // Assert
            _ = result.Should().BeEquivalentTo(expectedRoles);
        }

        [Test]
        public async Task GetByIdAsync_ExistingRole_ReturnsExpectedRole()
        {
            // Arrange
            var expectedRole = Fixture.Create<Role>();

            _ = DbContext.Add(expectedRole);

            _ = await DbContext.SaveChangesAsync();

            //Act
            var result = await this.roleRepository.GetByIdAsync(expectedRole.Id);

            // Assert
            _ = result.Should().BeEquivalentTo(expectedRole);
        }
    }
}
