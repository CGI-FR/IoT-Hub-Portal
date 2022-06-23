// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace AzureIoTHub.Portal.Server.Tests.Unit.Services
{
    using System.Collections.Generic;
    using System.Linq;
    using Client.Services;
    using FluentAssertions;
    using NUnit.Framework;

    [TestFixture]
    public class DashboardLayoutServiceTests
    {
        [Test]
        public void RefreshDashboardShouldRaiseRefreshDashboardOccurredEvent()
        {
            // Arrange
            var receivedEvents = new List<string>();
            var layoutService = new DashboardLayoutService();
            layoutService.RefreshDashboardOccurred += (sender, _) =>
            {
                receivedEvents.Add(sender?.GetType().ToString());
            };

            // Act
            layoutService.RefreshDashboard();

            // Assert
            _ = receivedEvents.Count.Should().Be(1);
            _ = receivedEvents.First().Should().Be(typeof(DashboardLayoutService).ToString());
        }
    }
}
