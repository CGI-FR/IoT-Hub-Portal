// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace AzureIoTHub.Portal.Server.Tests.Unit
{
    using System;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Logging.Abstractions;
    using Moq;
    using NUnit.Framework;
    using RichardSzalay.MockHttp;

    public abstract class BackendUnitTest : IDisposable
    {
        protected virtual ServiceCollection ServiceCollection { get; set; }

        protected virtual ServiceProvider Services { get; set; }

        protected virtual MockRepository MockRepository { get; set; }

        protected virtual MockHttpMessageHandler MockHttpClient { get; set; }

        protected virtual AutoFixture.Fixture Fixture { get; } = new();

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
