// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace IoTHub.Portal.Tests.Unit.Server.Controllers.v10
{
    using IoTHub.Portal.Server.Controllers.v10;
    using IoTHub.Portal.Shared.Models.v10;
    using FluentAssertions;
    using NUnit.Framework;

    [TestFixture]
    public class DashboardControllerTests
    {
        [Test]
        public void GetPortalMetricsShouldReturnMetrics()
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

            var dashboardController = new DashboardController(portalMetric);

            // Act
            var response = dashboardController.GetPortalMetrics();

            // Assert
            _ = response.Should().NotBeNull();
            _ = response.Value.Should().BeEquivalentTo(portalMetric);
        }
    }
}
