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

    public class AccessControlRepositoryTest : BackendUnitTest
    {
        private AccessControlRepository accessControlRepository;

        public override void Setup()
        {
            base.Setup();

            Fixture.Behaviors.OfType<ThrowingRecursionBehavior>().ToList()
                .ForEach(b => Fixture.Behaviors.Remove(b));
            Fixture.Behaviors.Add(new OmitOnRecursionBehavior());

            this.accessControlRepository = new AccessControlRepository(DbContext);
        }

        [Test]
        public async Task GetAllShouldReturnExpectedAccessControls()
        {
            // Arrange
            var expectedAccessControls = Fixture.CreateMany<AccessControl>(2).ToList();

            await DbContext.AddRangeAsync(expectedAccessControls);
            _ = await DbContext.SaveChangesAsync();

            //Act
            var result = this.accessControlRepository.GetAll().ToList();

            // Assert
            _ = result.Should().BeEquivalentTo(expectedAccessControls);
        }

        [Test]
        public async Task GetByIdAsync_ExistingAccessControl_ReturnsExpectedAccessControl()
        {
            // Arrange
            var expectedAccessControl = Fixture.Create<AccessControl>();

            _ = DbContext.Add(expectedAccessControl);
            _ = await DbContext.SaveChangesAsync();

            //Act
            var result = await this.accessControlRepository.GetByIdAsync(expectedAccessControl.Id);

            // Assert
            _ = result.Should().BeEquivalentTo(expectedAccessControl);
        }
    }
}
