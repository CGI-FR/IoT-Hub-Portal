// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace AzureIoTHub.Portal.Tests.Unit.UnitTests.Bases
{
    using System;
    using AutoMapper;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Diagnostics;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Logging.Abstractions;
    using Moq;
    using NUnit.Framework;
    using Portal.Infrastructure;
    using Portal.Server.Mappers;
    using RichardSzalay.MockHttp;

    public abstract class BackendUnitTest : IDisposable
    {
        protected virtual ServiceCollection ServiceCollection { get; set; }

        protected virtual ServiceProvider Services { get; set; }

        protected virtual MockRepository MockRepository { get; set; }

        protected virtual MockHttpMessageHandler MockHttpClient { get; set; }

        protected virtual AutoFixture.Fixture Fixture { get; } = new();

        protected virtual IMapper Mapper { get; set; }

        protected virtual PortalDbContext DbContext { get; set; }

        [SetUp]
        public virtual void Setup()
        {
            ServiceCollection = new ServiceCollection();
            _ = ServiceCollection.AddSingleton(typeof(ILogger<>), typeof(NullLogger<>));

            // Configure Mockups
            MockRepository = new MockRepository(MockBehavior.Strict);

            // Add Mock Http Client
            MockHttpClient = new MockHttpMessageHandler();
            var httpClient = MockHttpClient.ToHttpClient();
            httpClient.BaseAddress = new Uri("http://fake.local");
            _ = ServiceCollection.AddSingleton(httpClient);

            // Add Mapper Configuration
            var mappingConfig = new MapperConfiguration(mc =>
            {
                mc.AddProfile(new DeviceTagProfile());
                mc.AddProfile(new DeviceModelProfile());
                mc.AddProfile(new DeviceModelCommandProfile());
            });
            Mapper = mappingConfig.CreateMapper();
            _ = ServiceCollection.AddSingleton(Mapper);

            // Add InMemory Database
            var contextOptions = new DbContextOptionsBuilder<PortalDbContext>()
                .UseInMemoryDatabase("TestContext")
                .ConfigureWarnings(b => b.Ignore(InMemoryEventId.TransactionIgnoredWarning))
                .Options;
            DbContext = new PortalDbContext(contextOptions);
            _ = DbContext.Database.EnsureDeleted();
            _ = DbContext.Database.EnsureCreated();
        }

        [TearDown]
        public void TearDown()
        {
            Services?.Dispose();
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
        }
    }
}
