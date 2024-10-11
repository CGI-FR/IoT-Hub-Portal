// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace IoTHub.Portal.Tests.Unit.UnitTests.Bases
{
    using Helpers;
    using Bunit;
    using MudBlazor.Services;
    using System.Globalization;

    public abstract class BlazorUnitTest : TestContextWrapper, IDisposable
    {
        protected virtual MockRepository MockRepository { get; set; }

        protected virtual MockHttpMessageHandler MockHttpClient { get; set; }

        protected virtual AutoFixture.Fixture Fixture { get; } = new();

        [SetUp]
        public virtual void Setup()
        {
            // Configure Mockups
            MockRepository = new MockRepository(MockBehavior.Strict);

            // Configure TestContext
            TestContext = new Bunit.TestContext();
            _ = Services.AddMudServices();
            _ = JSInterop.Mode = JSRuntimeMode.Loose;

            // Add Mock Http Client
            MockHttpClient = Services.AddMockHttpClient();

            CultureInfo.CurrentCulture = CultureInfo.InvariantCulture;
            CultureInfo.CurrentUICulture = CultureInfo.InvariantCulture;
        }

        [TearDown]
        public void TearDown() => TestContext?.Dispose();

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
