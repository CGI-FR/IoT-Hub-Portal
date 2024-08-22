// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace IoTHub.Portal.Tests.Unit.Client.Pages
{
    using IoTHub.Portal.Client.Services;
    using IoTHub.Portal.Shared.Models.v1._0;
    using UnitTests.Bases;
    using Bunit;
    using FluentAssertions;
    using Microsoft.Extensions.DependencyInjection;
    using Moq;
    using NUnit.Framework;
    using Index = Portal.Client.Pages.Index;

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
        public void IndexShouldRenderCorrectlyForAzure()
        {
            // Arrange
            _ = Services.AddSingleton(new PortalSettings { CloudProvider = "Azure", IsLoRaSupported = true });

            var portalMetric = new PortalMetric();

            _ = this.mockDashboardMetricsClientService.Setup(c => c.GetPortalMetrics())
                .ReturnsAsync(portalMetric);

            // Act
            var cut = RenderComponent<Index>();

            // Assert
            cut.WaitForAssertion(() => cut.FindAll("#dashboard-metric-counter-icon").Count.Should().Be(6));
            cut.WaitForAssertion(() => cut.FindAll("#dashboard-metric-counter-title").Count.Should().Be(6));
            cut.WaitForAssertion(() => cut.FindAll("#dashboard-metric-counter-value").Count.Should().Be(6));

            cut.WaitForAssertion(() => MockRepository.VerifyAll());
        }

        [Test]
        public void IndexShouldRenderCorrectlyForAWS()
        {
            // Arrange
            _ = Services.AddSingleton(new PortalSettings { CloudProvider = "AWS", IsLoRaSupported = false });

            var portalMetric = new PortalMetric();

            _ = this.mockDashboardMetricsClientService.Setup(c => c.GetPortalMetrics())
                .ReturnsAsync(portalMetric);

            // Act
            var cut = RenderComponent<Index>();

            // Assert
            cut.WaitForAssertion(() => cut.FindAll("#dashboard-metric-counter-icon").Count.Should().Be(4));
            cut.WaitForAssertion(() => cut.FindAll("#dashboard-metric-counter-title").Count.Should().Be(4));
            cut.WaitForAssertion(() => cut.FindAll("#dashboard-metric-counter-value").Count.Should().Be(4));

            cut.WaitForAssertion(() => MockRepository.VerifyAll());
        }
    }
}
