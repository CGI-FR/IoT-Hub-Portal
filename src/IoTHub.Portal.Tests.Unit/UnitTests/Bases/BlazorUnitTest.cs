// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace IoTHub.Portal.Tests.Unit.UnitTests.Bases
{
    public abstract class BlazorUnitTest : TestContextWrapper, IDisposable
    {
        protected virtual MockRepository MockRepository { get; set; }
        protected virtual MockHttpMessageHandler MockHttpClient { get; set; }
        protected virtual Fixture Fixture { get; } = new();
        protected Mock<IPermissionsService> MockPermissionsService;
        protected TestAuthorizationContext AuthContext;

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

            // Add authentication Context
            this.AuthContext = TestContext?.AddTestAuthorization();
            _ = this.AuthContext?.SetAuthorized(Guid.NewGuid().ToString());

            // Setup mock Permissions service
            this.MockPermissionsService = MockRepository.Create<IPermissionsService>();
            _ = Services.AddSingleton(this.MockPermissionsService.Object);

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
