// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace AzureIoTHub.Portal.Server.Tests.Unit.Pages.Dashboard
{
    using System.Globalization;
    using AzureIoTHub.Portal.Client.Pages.Dashboard;
    using Bunit;
    using Client.Exceptions;
    using Client.Models;
    using Client.Services;
    using FluentAssertions;
    using Microsoft.Extensions.DependencyInjection;
    using Moq;
    using MudBlazor.Services;
    using NUnit.Framework;
    using Portal.Shared.Models.v1._0;

    [TestFixture]
    public class DashboardMetricsTests : TestContextWrapper
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
                ConcentratorCount = 6,
                ConnectedConcentratorCount = 7
            };

            _ = this.mockDashboardMetricsClientService.Setup(c => c.GetPortalMetrics())
                .ReturnsAsync(portalMetric);

            // Act
            var cut = RenderComponent<DashboardMetrics>();

            // Assert
            cut.WaitForAssertion(() => cut.FindAll("#dashboard-metric-counter-icon").Count.Should().Be(7));
            cut.WaitForAssertion(() => cut.FindAll("#dashboard-metric-counter-title").Count.Should().Be(7));
            cut.WaitForAssertion(() => cut.FindAll("#dashboard-metric-counter-value").Count.Should().Be(7));

            cut.WaitForAssertion(() => cut.Find("#dashboard-metric-counter-title").TextContent.Should().Be("Devices"));
            cut.WaitForAssertion(() => cut.Find("#dashboard-metric-counter-value").TextContent.Should().Be(portalMetric.DeviceCount.ToString(CultureInfo.InvariantCulture)));

            cut.WaitForAssertion(() => this.mockRepository.VerifyAll());
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
            cut.WaitForAssertion(() => cut.FindAll("#dashboard-metric-counter-icon").Count.Should().Be(7));
            cut.WaitForAssertion(() => cut.FindAll("#dashboard-metric-counter-title").Count.Should().Be(7));
            cut.WaitForAssertion(() => cut.FindAll("#dashboard-metric-counter-value").Count.Should().Be(7));

            cut.WaitForAssertion(() => cut.Find("#dashboard-metric-counter-title").TextContent.Should().Be("Devices"));
            cut.WaitForAssertion(() => cut.Find("#dashboard-metric-counter-value").TextContent.Should().Be("0"));

            cut.WaitForAssertion(() => this.mockRepository.VerifyAll());
        }
    }
}
