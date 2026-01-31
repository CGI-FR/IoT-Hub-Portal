// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace IoTHub.Portal.Tests.Unit.Infrastructure.Repositories
{
    using System.Linq;
    using System.Threading.Tasks;
    using AutoFixture;
    using FluentAssertions;
    using IoTHub.Portal.Domain.Entities;
    using IoTHub.Portal.Infrastructure.Repositories;
    using IoTHub.Portal.Tests.Unit.UnitTests.Bases;
    using NUnit.Framework;

    public class ActionRepositoryTest : BackendUnitTest
    {
        private ActionRepository actionRepository;

        public override void Setup()
        {
            base.Setup();

            this.actionRepository = new ActionRepository(DbContext);
        }

        [Test]
        public async Task GetAllShouldReturnExpectedActions()
        {
            // Arrange
            var expectedActions = Fixture.CreateMany<Action>(5).ToList();

            await DbContext.AddRangeAsync(expectedActions);

            _ = await DbContext.SaveChangesAsync();

            //Act
            var result = this.actionRepository.GetAll().ToList();

            // Assert
            _ = result.Should().BeEquivalentTo(expectedActions);
        }

        [Test]
        public async Task GetByIdAsync_ExistingAction_ReturnsExpectedAction()
        {
            // Arrange
            var expectedAction = Fixture.Create<Action>();

            _ = DbContext.Add(expectedAction);

            _ = await DbContext.SaveChangesAsync();

            //Act
            var result = await this.actionRepository.GetByIdAsync(expectedAction.Id);

            // Assert
            _ = result.Should().BeEquivalentTo(expectedAction);
        }
    }
}
