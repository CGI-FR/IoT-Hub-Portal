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
    public class LayoutServiceTests
    {
        [Test]
        public void ToggleDrawerShouldUpdateTheValueOfIsDrawerOpenAndRaiseMajorUpdateOccurredEvent()
        {
            // Arrange
            var receivedEvents = new List<string>();
            var layoutService = new LayoutService
            {
                IsDrawerOpen = false
            };
            layoutService.MajorUpdateOccurred += (sender, args) =>
            {
                receivedEvents.Add(sender?.GetType().ToString());
            };

            // Act
            layoutService.ToggleDrawer();

            // Assert
            _ = layoutService.IsDrawerOpen.Should().BeTrue();
            _ = receivedEvents.Count.Should().Be(1);
            _ = receivedEvents.First().Should().Be(typeof(LayoutService).ToString());
        }
    }
}
