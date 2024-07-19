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

    public class UserRepositoryTest : BackendUnitTest
    {
        private UserRepository userRepository;

        public override void Setup()
        {
            base.Setup();

            Fixture.Behaviors.OfType<ThrowingRecursionBehavior>().ToList()
                .ForEach(b => Fixture.Behaviors.Remove(b));
            Fixture.Behaviors.Add(new OmitOnRecursionBehavior());

            this.userRepository = new UserRepository(DbContext);
        }

        [Test]
        public async Task GetAllShouldReturnExpectedUsers()
        {
            // Arrange
            var expectedUsers = Fixture.CreateMany<User>(2).ToList();

            await DbContext.AddRangeAsync(expectedUsers);
            _ = await DbContext.SaveChangesAsync();

            //Act
            var result = this.userRepository.GetAll().ToList();

            // Assert
            _ = result.Should().BeEquivalentTo(expectedUsers);
        }

        [Test]
        public async Task GetByIdAsync_ExistingUser_ReturnsExpectedUser()
        {
            // Arrange
            var expectedUser = Fixture.Create<User>();

            _ = DbContext.Add(expectedUser);
            _ = await DbContext.SaveChangesAsync();

            //Act
            var result = await this.userRepository.GetByIdAsync(expectedUser.Id);

            // Assert
            _ = result.Should().BeEquivalentTo(expectedUser);
        }
    }
}
