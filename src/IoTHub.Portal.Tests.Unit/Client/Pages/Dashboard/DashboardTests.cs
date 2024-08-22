// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace IoTHub.Portal.Tests.Unit.Client.Pages.Dashboard
{
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using IoTHub.Portal.Client.Pages.Dashboard;
    using IoTHub.Portal.Client.Services;
    using IoTHub.Portal.Shared.Models.v1._0;
    using UnitTests.Bases;
    using Bunit;
    using FluentAssertions;
    using Microsoft.Extensions.DependencyInjection;
    using Moq;
    using NUnit.Framework;

    [TestFixture]
    public class DashboardTests : BlazorUnitTest
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
        public void DashboardShouldRenderCorrectlyForAzure()
        {
            // Arrange
            _ = Services.AddSingleton(new PortalSettings { CloudProvider = "Azure", IsLoRaSupported = true });

            var portalMetric = new PortalMetric
            {
                DeviceCount = 1,
                ConnectedDeviceCount = 2,
                EdgeDeviceCount = 3,
                ConnectedEdgeDeviceCount = 4,
                FailedDeploymentCount = 5,
                ConcentratorCount = 6
            };

            _ = this.mockDashboardMetricsClientService.Setup(c => c.GetPortalMetrics())
                .ReturnsAsync(portalMetric);

            // Act
            var cut = RenderComponent<Dashboard>();

            // Assert
            cut.WaitForAssertion(() => cut.FindAll("#dashboard-metric-counter-icon").Count.Should().Be(6));
            cut.WaitForAssertion(() => cut.FindAll("#dashboard-metric-counter-title").Count.Should().Be(6));
            cut.WaitForAssertion(() => cut.FindAll("#dashboard-metric-counter-value").Count.Should().Be(6));

            cut.WaitForAssertion(() => cut.Find("#dashboard-metric-counter-title").TextContent.Should().Be("Devices"));
            cut.WaitForAssertion(() => cut.Find("#dashboard-metric-counter-value").TextContent.Should().Be(portalMetric.DeviceCount.ToString(CultureInfo.InvariantCulture)));

            cut.WaitForAssertion(() => MockRepository.VerifyAll());
        }

        [Test]
        public void DashboardShouldRenderCorrectlyForAWS()
        {
            // Arrange
            _ = Services.AddSingleton(new PortalSettings { CloudProvider = "AWS" });

            var portalMetric = new PortalMetric
            {
                DeviceCount = 1,
                EdgeDeviceCount = 3,
                ConnectedEdgeDeviceCount = 4,
                FailedDeploymentCount = 5,
            };

            _ = this.mockDashboardMetricsClientService.Setup(c => c.GetPortalMetrics())
                .ReturnsAsync(portalMetric);

            // Act
            var cut = RenderComponent<Dashboard>();

            // Assert
            cut.WaitForAssertion(() => cut.FindAll("#dashboard-metric-counter-icon").Count.Should().Be(4));
            cut.WaitForAssertion(() => cut.FindAll("#dashboard-metric-counter-title").Count.Should().Be(4));
            cut.WaitForAssertion(() => cut.FindAll("#dashboard-metric-counter-value").Count.Should().Be(4));

            cut.WaitForAssertion(() => cut.Find("#dashboard-metric-counter-title").TextContent.Should().Be("Devices"));
            cut.WaitForAssertion(() => cut.Find("#dashboard-metric-counter-value").TextContent.Should().Be(portalMetric.DeviceCount.ToString(CultureInfo.InvariantCulture)));

            cut.WaitForAssertion(() => MockRepository.VerifyAll());
        }

        [Test]
        public void OnCLickOnRefreshShouldRaiseRefreshDashboardEventForAzure()
        {
            // Arrange

            _ = Services.AddSingleton(new PortalSettings { CloudProvider = "Azure" });

            var portalMetric = new PortalMetric
            {
                DeviceCount = 1,
                ConnectedDeviceCount = 2,
                EdgeDeviceCount = 3,
                ConnectedEdgeDeviceCount = 4,
                FailedDeploymentCount = 5,
                ConcentratorCount = 6
            };

            var receivedEvents = new List<string>();

            var dashboardLayoutService = TestContext?.Services.GetRequiredService<IDashboardLayoutService>();

            dashboardLayoutService.RefreshDashboardOccurred += (sender, _) =>
            {
                receivedEvents.Add(sender?.GetType().ToString());
            };

            _ = this.mockDashboardMetricsClientService.Setup(c => c.GetPortalMetrics())
                .ReturnsAsync(portalMetric);

            var cut = RenderComponent<Dashboard>();
            cut.WaitForAssertion(() => cut.Find("#refresh-dashboard"));

            // Act
            cut.Find("#refresh-dashboard").Click();

            // Assert
            cut.WaitForAssertion(() => receivedEvents.Count.Should().Be(1));
            cut.WaitForAssertion(() => receivedEvents.First().Should().Be(typeof(DashboardLayoutService).ToString()));

            cut.WaitForAssertion(() => this.mockDashboardMetricsClientService.Verify(service => service.GetPortalMetrics(), Times.Exactly(2)));
            cut.WaitForAssertion(() => MockRepository.VerifyAll());
        }

        [Test]
        public void OnCLickOnRefreshShouldRaiseRefreshDashboardEventForAWS()
        {
            // Arrange

            _ = Services.AddSingleton(new PortalSettings { CloudProvider = "AWS" });

            var portalMetric = new PortalMetric
            {
                DeviceCount = 1,
                EdgeDeviceCount = 3,
                ConnectedEdgeDeviceCount = 4,
                FailedDeploymentCount = 5,
            };

            var receivedEvents = new List<string>();

            var dashboardLayoutService = TestContext?.Services.GetRequiredService<IDashboardLayoutService>();

            dashboardLayoutService.RefreshDashboardOccurred += (sender, _) =>
            {
                receivedEvents.Add(sender?.GetType().ToString());
            };

            _ = this.mockDashboardMetricsClientService.Setup(c => c.GetPortalMetrics())
                .ReturnsAsync(portalMetric);

            var cut = RenderComponent<Dashboard>();
            cut.WaitForAssertion(() => cut.Find("#refresh-dashboard"));

            // Act
            cut.Find("#refresh-dashboard").Click();

            // Assert
            cut.WaitForAssertion(() => receivedEvents.Count.Should().Be(1));
            cut.WaitForAssertion(() => receivedEvents.First().Should().Be(typeof(DashboardLayoutService).ToString()));

            cut.WaitForAssertion(() => this.mockDashboardMetricsClientService.Verify(service => service.GetPortalMetrics(), Times.Exactly(2)));
            cut.WaitForAssertion(() => MockRepository.VerifyAll());
        }
    }
}
