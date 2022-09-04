// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace AzureIoTHub.Portal.Tests.Unit.Infrastructure.Repositories
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;
    using AzureIoTHub.Portal.Domain.Entities;
    using AzureIoTHub.Portal.Infrastructure.Repositories;
    using AzureIoTHub.Portal.Tests.Unit.UnitTests.Bases;
    using Moq;
    using NUnit.Framework;

    public class DeviceModelPropertiesRepositoryTests : RepositoryTestBase
    {
        private MockRepository mockRepository;

        [SetUp]
        public void SetUp()
        {
            this.mockRepository = new MockRepository(MockBehavior.Strict);

            var context = SetupDbContext();

            _ = context.Database.EnsureDeleted();
            _ = context.Database.EnsureCreated();
        }

        [Test]
        public async Task GetModelPropertiesTests()
        {
            // Arrange
            var context = SetupDbContext();
            var modelId = Guid.NewGuid().ToString();

            _ = context.Add(new DeviceModelProperty()
            {
                Id = Guid.NewGuid().ToString(),
                ModelId = modelId,
                DisplayName = Guid.NewGuid().ToString(),
                Name = Guid.NewGuid().ToString()
            });

            _ = context.Add(new DeviceModelProperty()
            {
                Id = Guid.NewGuid().ToString(),
                ModelId = Guid.NewGuid().ToString(),
                DisplayName = Guid.NewGuid().ToString(),
                Name = Guid.NewGuid().ToString()
            });

            _ = await context.SaveChangesAsync();

            var repository = new DeviceModelPropertiesRepository(context);

            // Act
            var result = await repository.GetModelProperties(modelId);

            // Assert
            Assert.AreEqual(1, result.Count());
        }

        [Test]
        public async Task SavePropertiesForModel()
        {
            // Arrange
            var context = SetupDbContext();
            var modelId = Guid.NewGuid().ToString();
            var initialPropertyId = Guid.NewGuid().ToString();
            _ = context.Add(new DeviceModelProperty()
            {
                Id = initialPropertyId,
                ModelId = modelId,
                DisplayName = Guid.NewGuid().ToString(),
                Name = Guid.NewGuid().ToString()
            });

            _ = context.Add(new DeviceModelProperty()
            {
                Id = Guid.NewGuid().ToString(),
                ModelId = Guid.NewGuid().ToString(),
                DisplayName = Guid.NewGuid().ToString(),
                Name = Guid.NewGuid().ToString()
            });

            _ = await context.SaveChangesAsync();

            var repositoryContext = SetupDbContext();
            var repository = new DeviceModelPropertiesRepository(repositoryContext);

            // Act
            await repository.SavePropertiesForModel(modelId, new[]
            {
                new DeviceModelProperty()
                {
                    Id = initialPropertyId,
                    ModelId = Guid.NewGuid().ToString(),
                    DisplayName = Guid.NewGuid().ToString(),
                    Name = Guid.NewGuid().ToString()
                },
                new DeviceModelProperty()
                {
                    Id = Guid.NewGuid().ToString(),
                    ModelId = Guid.NewGuid().ToString(),
                    DisplayName = Guid.NewGuid().ToString(),
                    Name = Guid.NewGuid().ToString()
                },
                new DeviceModelProperty()
                {
                    Id = Guid.NewGuid().ToString(),
                    ModelId = Guid.NewGuid().ToString(),
                    DisplayName = Guid.NewGuid().ToString(),
                    Name = Guid.NewGuid().ToString()
                }
            });

            _ = await repositoryContext.SaveChangesAsync();

            // Assert
            var finalContext = SetupDbContext();
            Assert.AreEqual(4, finalContext.Set<DeviceModelProperty>().Count());
            Assert.AreEqual(3, finalContext.Set<DeviceModelProperty>().Where(c => c.ModelId == modelId).Count());
        }
    }
}
