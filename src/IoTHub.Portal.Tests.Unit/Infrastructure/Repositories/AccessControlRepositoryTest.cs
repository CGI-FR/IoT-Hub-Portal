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
    using System.Collections.Generic;
    using System;
    using Action = Portal.Domain.Entities.Action;

    public class AccessControlRepositoryTest : BackendUnitTest
    {
        private AccessControlRepository accessControlRepository;

        public override void Setup()
        {
            base.Setup();

            this.accessControlRepository = new AccessControlRepository(DbContext);
        }

        [Test]
        public async Task GetAllShouldReturnExpectedAccessControls()
        {
            // Arrange
            var expectedAccessControls = new List<AccessControl>
            {
                new AccessControl
                {
                    Id = Guid.NewGuid().ToString(),
                    Role = new Role()
                    {
                        Name = "Role 1",
                        Color = "#0000000",
                        Description = "This is a test role",
                        Actions = new List<Action>()
                    },
                    Principal = new Principal(),
                    Scope = "example/test"
                },
                new AccessControl
                {
                    Id = Guid.NewGuid().ToString(),
                    Role = new Role()
                    {
                        Name = "Role 2",
                        Color = "#0000000",
                        Description = "This is a test role",
                        Actions = new List<Action>()
                    },
                    Principal = new Principal(),
                    Scope = "example/test"
                }
            };

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
            var expectedAccessControl = new AccessControl
            {
                Id = Guid.NewGuid().ToString(),
                Role = new Role()
                {
                    Name = "Role 1",
                    Color = "#0000000",
                    Description = "This is a test role",
                    Actions = new List<Action>()
                },
                Principal = new Principal(),
                Scope = "example/test"
            };

            _ = DbContext.Add(expectedAccessControl);

            _ = await DbContext.SaveChangesAsync();

            //Act
            var result = await this.accessControlRepository.GetByIdAsync(expectedAccessControl.Id);

            // Assert
            _ = result.Should().BeEquivalentTo(expectedAccessControl);
        }
    }
}
