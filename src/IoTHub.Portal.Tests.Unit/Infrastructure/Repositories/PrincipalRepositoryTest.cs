// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace IoTHub.Portal.Tests.Unit.Infrastructure.Repositories
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using FluentAssertions;
    using IoTHub.Portal.Domain.Entities;
    using IoTHub.Portal.Infrastructure.Repositories;
    using IoTHub.Portal.Tests.Unit.UnitTests.Bases;
    using NUnit.Framework;

    public class PrincipalRepositoryTest : BackendUnitTest
    {
        private PrincipalRepository principalRepository;

        public override void Setup()
        {
            base.Setup();

            this.principalRepository = new PrincipalRepository(DbContext);
        }

        [Test]
        public async Task GetAllShouldReturnExpectedPrincipals()
        {
            // Arrange
            var expectedPrincipals  = new List<Principal>
            {
                new Principal
                {
                    Id = Guid.NewGuid().ToString(),
                    AccessControls = new List<AccessControl>()
                },
                new Principal
                {
                    Id = Guid.NewGuid().ToString(),
                    AccessControls = new List<AccessControl>()
                }
            };

            await DbContext.AddRangeAsync(expectedPrincipals);

            _ = await DbContext.SaveChangesAsync();

            //Act
            var result = this.principalRepository.GetAll().ToList();

            // Assert
            _ = result.Should().BeEquivalentTo(expectedPrincipals);
        }

        [Test]
        public async Task GetByIdAsync_ExistingPrincipal_ReturnsExpectedPrincipal()
        {
            // Arrange
            var expectedPrincipal = new Principal
            {
                Id = Guid.NewGuid().ToString(),
                AccessControls = new List<AccessControl>()
            };

            _ = DbContext.Add(expectedPrincipal);

            _ = await DbContext.SaveChangesAsync();

            //Act
            var result = await this.principalRepository.GetByIdAsync(expectedPrincipal.Id);

            // Assert
            _ = result.Should().BeEquivalentTo(expectedPrincipal);
        }
    }
}
