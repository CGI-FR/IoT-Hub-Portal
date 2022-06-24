// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace AzureIoTHub.Portal.Server.Tests.Unit.Pages
{
    using Bunit;
    using Client.Services;
    using FluentAssertions;
    using Microsoft.Extensions.DependencyInjection;
    using Moq;
    using MudBlazor.Services;
    using NUnit.Framework;
    using Portal.Shared.Models.v1._0;
    using Index = Client.Pages.Index;

    [TestFixture]
    public class IndexTests : TestContextWrapper
    {
        private MockRepository mockRepository;
        private Mock<IDashboardMetricsClientService> mockDashboardMetricsClientService;

        [SetUp]
        public void Setup()
        {
            this.mockRepository = new MockRepository(MockBehavior.Strict);
            this.mockDashboardMetricsClientService = this.mockRepository.Create<IDashboardMetricsClientService>();

            TestContext = new Bunit.TestContext();
            _ = TestContext.Services.AddMudServices();
            _ = TestContext.Services.AddScoped<IDashboardLayoutService, DashboardLayoutService>();

            _ = TestContext.Services.AddSingleton(this.mockDashboardMetricsClientService.Object);

            TestContext.JSInterop.Mode = JSRuntimeMode.Loose;
        }

        [TearDown]
        public void TearDown() => TestContext?.Dispose();

        [Test]
        public void IndexShouldRenderCorrectly()
        {
            // Arrange
            var portalMetric = new PortalMetric();

            _ = this.mockDashboardMetricsClientService.Setup(c => c.GetPortalMetrics())
                .ReturnsAsync(portalMetric);

            // Act
            var cut = RenderComponent<Index>();

            // Assert
            cut.WaitForAssertion(() => cut.FindAll("#dashboard-metric-counter-icon").Count.Should().Be(7));
            cut.WaitForAssertion(() => cut.FindAll("#dashboard-metric-counter-title").Count.Should().Be(7));
            cut.WaitForAssertion(() => cut.FindAll("#dashboard-metric-counter-value").Count.Should().Be(7));

            cut.WaitForAssertion(() => this.mockRepository.VerifyAll());
        }
    }
}
