// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace AzureIoTHub.Portal.Tests.Unit.Infrastructure.Repositories
{
    using System;
    using System.Threading.Tasks;
    using AzureIoTHub.Portal.Domain.Base;
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
            var instance = new GenericRepository<MockEntity>(SetupDbContext());

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
            var instance = new GenericRepository<MockEntity>(context);

            _ = context.Add(new MockEntity()
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
            var instance = new GenericRepository<MockEntity>(context);

            // Act
            await instance.InsertAsync(new MockEntity()
            {
                Id = entityId
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

            _ = firstContext.Add(new MockEntity()
            {
                Id = entityId,
                Name = "Name1"
            });

            _ = firstContext.SaveChanges();

            var context = SetupDbContext();
            var instance = new GenericRepository<MockEntity>(context);

            // Act
            var updated = new MockEntity()
            {
                Id = entityId,
                Name = "Name2"
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

            _ = firstContext.Add(new MockEntity()
            {
                Id = entityId,
                Name = "Name1"
            });

            _ = firstContext.SaveChanges();

            var context = SetupDbContext();
            var instance = new GenericRepository<MockEntity>(context);

            // Act
            instance.Delete(entityId);
            _ = context.SaveChanges();

            // Assert
            Assert.IsNull(context.Set<MockEntity>().Find(entityId));
        }

        public class MockEntity : EntityBase
        {
            public string Name { get; set; }
        }

        public class MockContext : DbContext
        {
            public DbSet<MockEntity> Entities { get; set; }

            public MockContext(DbContextOptions<MockContext> options)
                : base(options)
            { }
        }

        private static MockContext SetupDbContext()
        {
            var contextOptions = new DbContextOptionsBuilder<MockContext>()
                   .UseInMemoryDatabase("TestContext")
                   .ConfigureWarnings(b => b.Ignore(InMemoryEventId.TransactionIgnoredWarning))
                   .Options;

            return new MockContext(contextOptions);
        }
    }
}
