// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace AzureIoTHub.Portal.Tests.Unit.Infrastructure.Repositories
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using AzureIoTHub.Portal.Domain.Entities;
    using AzureIoTHub.Portal.Infrastructure.Repositories;
    using AzureIoTHub.Portal.Tests.Unit.UnitTests.Bases;
    using FluentAssertions;
    using Microsoft.EntityFrameworkCore;
    using Moq;
    using NUnit.Framework;

    [TestFixture]
    public class GenericRepositoryTests : RepositoryTestBase
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



        [Test]
        public async Task GetPaginatedListAsync_CustomFilter_ExpectedPageReturned()
        {
            // Arrange
            var context = SetupDbContext();

            await context.AddRangeAsync(new List<DeviceTag>
            {
                new()
                {
                    Id = "device_tag_01",
                    Label = "Location 1",
                    Required = false,
                    Searchable = true
                },
                new()
                {
                    Id = "device_tag_02",
                    Label = "Location 2",
                    Required = true,
                    Searchable = true
                },
                new()
                {
                    Id = "device_tag_03",
                    Label = "Machine",
                    Required = true,
                    Searchable = true
                },
                new()
                {
                    Id = "device_tag_04",
                    Label = "Location",
                    Required = false,
                    Searchable = false
                },
            });

            _ = await context.SaveChangesAsync();

            var instance = new GenericRepository<DeviceTag>(context);

            // Order by label desc
            var orderBy = new[] { $"{nameof(DeviceTag.Label)} desc" };

            // Filter only tags with labels containing "location" and are searchable
            var deviceTagPredicate = PredicateBuilder.True<DeviceTag>()
                .And(tag => tag.Label.ToLowerInvariant().Contains("location"))
                .And(tag => tag.Searchable);

            // Act
            var result = await instance.GetPaginatedListAsync(0, 10, orderBy, deviceTagPredicate);

            // Assert
            _ = result.Data.Count.Should().Be(2);
            _ = result.TotalCount.Should().Be(2);
            _ = result.PageSize.Should().Be(10);
            _ = result.HasNextPage.Should().BeFalse();
            _ = result.HasPreviousPage.Should().BeFalse();
            _ = result.CurrentPage.Should().Be(0);
            _ = result.TotalPages.Should().Be(1);

            var firstDeviceTag = result.Data.First();

            _ = firstDeviceTag.Id.Should().Be("device_tag_02");
            _ = firstDeviceTag.Label.Should().Be("Location 2");
            _ = firstDeviceTag.Required.Should().BeTrue();
            _ = firstDeviceTag.Searchable.Should().BeTrue();

            var secondDeviceTag = result.Data.Skip(1).First();

            _ = secondDeviceTag.Id.Should().Be("device_tag_01");
            _ = secondDeviceTag.Label.Should().Be("Location 1");
            _ = secondDeviceTag.Required.Should().BeFalse();
            _ = secondDeviceTag.Searchable.Should().BeTrue();
        }
    }
}
