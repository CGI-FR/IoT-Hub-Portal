// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace AzureIoTHub.Portal.Server.Tests.Unit.Pages
{
    using Bunit;
    using Bunit.TestDoubles;
    using FluentAssertions;
    using Microsoft.Extensions.DependencyInjection;
    using MudBlazor.Services;
    using NUnit.Framework;
    using Index = Client.Pages.Index;

    [TestFixture]
    public class IndexTests : TestContextWrapper
    {
        private FakeNavigationManager mockNavigationManager;

        [SetUp]
        public void Setup()
        {
            TestContext = new Bunit.TestContext();
            _ = TestContext.Services.AddMudServices();

            TestContext.JSInterop.Mode = JSRuntimeMode.Loose;

            this.mockNavigationManager = TestContext.Services.GetRequiredService<FakeNavigationManager>();
        }

        [TearDown]
        public void TearDown() => TestContext?.Dispose();

        [Test]
        public void OnInitializedShouldRedirectToDashboard()
        {
            // Act
            var cut = RenderComponent<Index>();

            // Assert
            cut.WaitForAssertion(() => this.mockNavigationManager.Uri.Should().EndWith("/dashboard"));
        }
    }
}
