// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace AzureIoTHub.Portal.Tests.Unit.Components.Dashboard
{
    using System.Globalization;
    using AzureIoTHub.Portal.Client.Components.Dashboard;
    using AzureIoTHub.Portal.Client.Exceptions;
    using AzureIoTHub.Portal.Client.Models;
    using AzureIoTHub.Portal.Client.Services;
    using AzureIoTHub.Portal.Shared.Models.v1._0;
    using Bunit;
    using FluentAssertions;
    using Microsoft.Extensions.DependencyInjection;
    using Moq;
    using NUnit.Framework;
    using UnitTests.Bases;

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
        public void DashboardMetricShouldRenderCorrectly()
        {
            // Arrange
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
        public void OnRefreshDashboardEventDashboardMetricShouldRefreshMetrics()
        {
            // Arrange
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
        public void DashboardMetricShouldProcessProblemDetailsExceptionWhenIssueOccurs()
        {
            // Arrange
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
    }
}
