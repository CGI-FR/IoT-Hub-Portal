// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace IoTHub.Portal.Tests.Unit.Client.Components.Dashboard
{
    using System.Globalization;
    using Bunit;
    using FluentAssertions;
    using IoTHub.Portal.Client.Components.Dashboard;
    using IoTHub.Portal.Client.Exceptions;
    using IoTHub.Portal.Client.Models;
    using IoTHub.Portal.Client.Services;
    using IoTHub.Portal.Tests.Unit.UnitTests.Bases;
    using Microsoft.Extensions.DependencyInjection;
    using Moq;
    using NUnit.Framework;
    using Shared.Models.v1._0;

    [TestFixture]
    public class DashboardMetricsTests : BlazorUnitTest
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
        public void DashboardMetricShouldRenderCorrectlyForAzure()
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
            var cut = RenderComponent<DashboardMetrics>();

            // Assert
            cut.WaitForAssertion(() => cut.FindAll("#dashboard-metric-counter-icon").Count.Should().Be(6));
            cut.WaitForAssertion(() => cut.FindAll("#dashboard-metric-counter-title").Count.Should().Be(6));
            cut.WaitForAssertion(() => cut.FindAll("#dashboard-metric-counter-value").Count.Should().Be(6));

            cut.WaitForAssertion(() => cut.Find("#dashboard-metric-counter-title").TextContent.Should().Be("Devices"));
            cut.WaitForAssertion(() => cut.Find("#dashboard-metric-counter-value").TextContent.Should().Be(portalMetric.DeviceCount.ToString(CultureInfo.InvariantCulture)));

            cut.WaitForAssertion(() => MockRepository.VerifyAll());
        }

        [Test]
        public void DashboardMetricShouldRenderCorrectlyForAWS()
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
            var cut = RenderComponent<DashboardMetrics>();

            // Assert
            cut.WaitForAssertion(() => cut.FindAll("#dashboard-metric-counter-icon").Count.Should().Be(4));
            cut.WaitForAssertion(() => cut.FindAll("#dashboard-metric-counter-title").Count.Should().Be(4));
            cut.WaitForAssertion(() => cut.FindAll("#dashboard-metric-counter-value").Count.Should().Be(4));

            cut.WaitForAssertion(() => cut.Find("#dashboard-metric-counter-title").TextContent.Should().Be("Devices"));
            cut.WaitForAssertion(() => cut.Find("#dashboard-metric-counter-value").TextContent.Should().Be(portalMetric.DeviceCount.ToString(CultureInfo.InvariantCulture)));

            cut.WaitForAssertion(() => MockRepository.VerifyAll());
        }

        [Test]
        public void OnRefreshDashboardEventDashboardMetricShouldRefreshMetricsForAzure()
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

            var cut = RenderComponent<DashboardMetrics>();

            cut.WaitForAssertion(() => cut.FindAll("#dashboard-metric-counter-icon").Count.Should().Be(6));
            cut.WaitForAssertion(() => cut.FindAll("#dashboard-metric-counter-title").Count.Should().Be(6));
            cut.WaitForAssertion(() => cut.FindAll("#dashboard-metric-counter-value").Count.Should().Be(6));

            cut.WaitForAssertion(() => cut.Find("#dashboard-metric-counter-title").TextContent.Should().Be("Devices"));
            cut.WaitForAssertion(() => cut.Find("#dashboard-metric-counter-value").TextContent.Should().Be(portalMetric.DeviceCount.ToString(CultureInfo.InvariantCulture)));

            // Act
            TestContext?.Services.GetRequiredService<IDashboardLayoutService>().RefreshDashboard();

            // Assert
            cut.WaitForAssertion(() => cut.FindAll("#dashboard-metric-counter-icon").Count.Should().Be(6));
            cut.WaitForAssertion(() => cut.FindAll("#dashboard-metric-counter-title").Count.Should().Be(6));
            cut.WaitForAssertion(() => cut.FindAll("#dashboard-metric-counter-value").Count.Should().Be(6));

            cut.WaitForAssertion(() => cut.Find("#dashboard-metric-counter-title").TextContent.Should().Be("Devices"));
            cut.WaitForAssertion(() => cut.Find("#dashboard-metric-counter-value").TextContent.Should().Be(portalMetric.DeviceCount.ToString(CultureInfo.InvariantCulture)));

            cut.WaitForAssertion(() => this.mockDashboardMetricsClientService.Verify(service => service.GetPortalMetrics(), Times.Exactly(2)));
            cut.WaitForAssertion(() => MockRepository.VerifyAll());
        }

        [Test]
        public void OnRefreshDashboardEventDashboardMetricShouldRefreshMetricsForAWS()
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

            var cut = RenderComponent<DashboardMetrics>();

            cut.WaitForAssertion(() => cut.FindAll("#dashboard-metric-counter-icon").Count.Should().Be(4));
            cut.WaitForAssertion(() => cut.FindAll("#dashboard-metric-counter-title").Count.Should().Be(4));
            cut.WaitForAssertion(() => cut.FindAll("#dashboard-metric-counter-value").Count.Should().Be(4));

            cut.WaitForAssertion(() => cut.Find("#dashboard-metric-counter-title").TextContent.Should().Be("Devices"));
            cut.WaitForAssertion(() => cut.Find("#dashboard-metric-counter-value").TextContent.Should().Be(portalMetric.DeviceCount.ToString(CultureInfo.InvariantCulture)));

            // Act
            TestContext?.Services.GetRequiredService<IDashboardLayoutService>().RefreshDashboard();

            // Assert
            cut.WaitForAssertion(() => cut.FindAll("#dashboard-metric-counter-icon").Count.Should().Be(4));
            cut.WaitForAssertion(() => cut.FindAll("#dashboard-metric-counter-title").Count.Should().Be(4));
            cut.WaitForAssertion(() => cut.FindAll("#dashboard-metric-counter-value").Count.Should().Be(4));

            cut.WaitForAssertion(() => cut.Find("#dashboard-metric-counter-title").TextContent.Should().Be("Devices"));
            cut.WaitForAssertion(() => cut.Find("#dashboard-metric-counter-value").TextContent.Should().Be(portalMetric.DeviceCount.ToString(CultureInfo.InvariantCulture)));

            cut.WaitForAssertion(() => this.mockDashboardMetricsClientService.Verify(service => service.GetPortalMetrics(), Times.Exactly(2)));
            cut.WaitForAssertion(() => MockRepository.VerifyAll());
        }

        [Test]
        public void DashboardMetricShouldProcessProblemDetailsExceptionWhenIssueOccursAzure()
        {
            // Arrange
            _ = Services.AddSingleton(new PortalSettings { CloudProvider = "Azure", IsLoRaSupported = true });

            _ = this.mockDashboardMetricsClientService.Setup(c => c.GetPortalMetrics())
                .ThrowsAsync(new ProblemDetailsException(new ProblemDetailsWithExceptionDetails()));

            // Act
            var cut = RenderComponent<DashboardMetrics>();

            // Assert
            cut.WaitForAssertion(() => cut.FindAll("#dashboard-metric-counter-icon").Count.Should().Be(6));
            cut.WaitForAssertion(() => cut.FindAll("#dashboard-metric-counter-title").Count.Should().Be(6));
            cut.WaitForAssertion(() => cut.FindAll("#dashboard-metric-counter-value").Count.Should().Be(6));

            cut.WaitForAssertion(() => cut.Find("#dashboard-metric-counter-title").TextContent.Should().Be("Devices"));
            cut.WaitForAssertion(() => cut.Find("#dashboard-metric-counter-value").TextContent.Should().Be("0"));

            cut.WaitForAssertion(() => MockRepository.VerifyAll());
        }

        [Test]
        public void DashboardMetricShouldProcessProblemDetailsExceptionWhenIssueOccursForAWS()
        {
            // Arrange
            _ = Services.AddSingleton(new PortalSettings { CloudProvider = "AWS" });

            _ = this.mockDashboardMetricsClientService.Setup(c => c.GetPortalMetrics())
                .ThrowsAsync(new ProblemDetailsException(new ProblemDetailsWithExceptionDetails()));

            // Act
            var cut = RenderComponent<DashboardMetrics>();

            // Assert
            cut.WaitForAssertion(() => cut.FindAll("#dashboard-metric-counter-icon").Count.Should().Be(4));
            cut.WaitForAssertion(() => cut.FindAll("#dashboard-metric-counter-title").Count.Should().Be(4));
            cut.WaitForAssertion(() => cut.FindAll("#dashboard-metric-counter-value").Count.Should().Be(4));

            cut.WaitForAssertion(() => cut.Find("#dashboard-metric-counter-title").TextContent.Should().Be("Devices"));
            cut.WaitForAssertion(() => cut.Find("#dashboard-metric-counter-value").TextContent.Should().Be("0"));

            cut.WaitForAssertion(() => MockRepository.VerifyAll());
        }
    }
}
