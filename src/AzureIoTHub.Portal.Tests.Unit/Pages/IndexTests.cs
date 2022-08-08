// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace AzureIoTHub.Portal.Server.Tests.Unit.Pages
{
    using Bunit;
    using Client.Services;
    using FluentAssertions;
    using Microsoft.Extensions.DependencyInjection;
    using Moq;
    using NUnit.Framework;
    using Portal.Shared.Models.v1._0;
    using Index = Client.Pages.Index;

    [TestFixture]
    public class IndexTests : BlazorUnitTest
    {
        private Mock<IDashboardMetricsClientService> mockDashboardMetricsClientService;
        public override void Setup()
        {
            base.Setup();

            this.mockDashboardMetricsClientService = MockRepository.Create<IDashboardMetricsClientService>();

            _ = Services.AddScoped<IDashboardLayoutService, DashboardLayoutService>();

            _ = Services.AddSingleton(this.mockDashboardMetricsClientService.Object);
        }

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

            cut.WaitForAssertion(() => MockRepository.VerifyAll());
        }
    }
}
