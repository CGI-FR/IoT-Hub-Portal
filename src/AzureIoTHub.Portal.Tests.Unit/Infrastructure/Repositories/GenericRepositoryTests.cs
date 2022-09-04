// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace AzureIoTHub.Portal.Tests.Unit.Infrastructure.Repositories
{
    using System;
    using System.Threading.Tasks;
    using AzureIoTHub.Portal.Domain.Entities;
    using AzureIoTHub.Portal.Infrastructure;
    using AzureIoTHub.Portal.Infrastructure.Repositories;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Diagnostics;
    using Moq;
    using NUnit.Framework;

    [TestFixture]
    public class GenericRepositoryTests
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
        public void GetAllTest()
        {
            // Arrange
            var instance = new GenericRepository<DeviceModelProperty>(SetupDbContext());

            // Act
            var result = instance.GetAll();

            // Assert
            Assert.IsNotNull(result);
            Assert.IsEmpty(result);
        }

        [Test]
        public async Task GetByIdAsyncTest()
        {
            // Arrange
            var entityId = Guid.NewGuid().ToString();

            var context = SetupDbContext();
            var instance = new GenericRepository<DeviceModelProperty>(context);

            _ = context.Add(new DeviceModelProperty()
            {
                Id = entityId
            });

            // Act
            var result = await instance.GetByIdAsync(entityId);

            // Assert
            Assert.IsNotNull(result);
        }

        [Test]
        public async Task InsertAsyncTest()
        {
            // Arrange
            var entityId = Guid.NewGuid().ToString();

            var context = SetupDbContext();
            var instance = new GenericRepository<DeviceModelProperty>(context);

            // Act
            await instance.InsertAsync(new DeviceModelProperty()
            {
                Id = entityId,
                Name = string.Empty,
                DisplayName = string.Empty,
                ModelId = string.Empty
            });

            // Assert
            var result = await context.SaveChangesAsync();
            Assert.AreEqual(1, result);
        }

        [Test]
        public async Task UpdateAsyncTest()
        {
            // Arrange
            var entityId = Guid.NewGuid().ToString();

            var firstContext = SetupDbContext();

            _ = firstContext.Add(new DeviceModelProperty()
            {
                Id = entityId,
                Name = "Name1",
                DisplayName = string.Empty,
                ModelId = string.Empty
            });

            _ = firstContext.SaveChanges();

            var context = SetupDbContext();
            var instance = new GenericRepository<DeviceModelProperty>(context);

            // Act
            var updated = new DeviceModelProperty()
            {
                Id = entityId,
                Name = "Name2",
                DisplayName = string.Empty,
                ModelId = string.Empty
            };

            instance.Update(updated);

            // Assert
            Assert.AreEqual(EntityState.Modified, context.Entry(updated).State);
        }

        [Test]
        public async Task DeleteTest()
        {
            // Arrange
            var entityId = Guid.NewGuid().ToString();

            var firstContext = SetupDbContext();

            _ = firstContext.Add(new DeviceModelProperty()
            {
                Id = entityId,
                Name = "Name1",
                DisplayName = string.Empty,
                ModelId = string.Empty
            });

            _ = firstContext.SaveChanges();

            var context = SetupDbContext();
            var instance = new GenericRepository<DeviceModelProperty>(context);

            // Act
            instance.Delete(entityId);
            _ = context.SaveChanges();

            // Assert
            Assert.IsNull(context.Set<DeviceModelProperty>().Find(entityId));
        }

        private static PortalDbContext SetupDbContext()
        {
            var contextOptions = new DbContextOptionsBuilder<PortalDbContext>()
                   .UseInMemoryDatabase("TestContext")
                   .ConfigureWarnings(b => b.Ignore(InMemoryEventId.TransactionIgnoredWarning))
                   .Options;

            return new PortalDbContext(contextOptions);
        }
    }
}
