// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace IoTHub.Portal.Tests.Unit.Client.Components.Dashboard
{
    using System.Globalization;
    using Bunit;
    using FluentAssertions;
    using IoTHub.Portal.Client.Components.Dashboard;
    using IoTHub.Portal.Tests.Unit.UnitTests.Bases;
    using MudBlazor;
    using NUnit.Framework;

    [TestFixture]
    public class DashboardMetricCounterTests : BlazorUnitTest
    {
        [Test]
        public void DashboardMetricCounterShouldRenderCorrectly()
        {
            // Arrange
            const string expectedTitle = "test";
            const int expectedCounter = 15;
            const Color expectedColor = Color.Dark;
            var expectedIcon = Icons.Material.Filled.Devices;

            // Act
            var cut = RenderComponent<DashboardMetricCounter>(
                ComponentParameter.CreateParameter(nameof(DashboardMetricCounter.Title), expectedTitle),
                ComponentParameter.CreateParameter(nameof(DashboardMetricCounter.Counter), expectedCounter),
                ComponentParameter.CreateParameter(nameof(DashboardMetricCounter.Color), expectedColor),
                ComponentParameter.CreateParameter(nameof(DashboardMetricCounter.Icon), expectedIcon));

            // Assert
            cut.WaitForAssertion(() => cut.Find("#dashboard-metric-counter-icon.mud-dark-text"));
            cut.WaitForAssertion(() => cut.Markup.Should().Contain(expectedIcon));
            cut.WaitForAssertion(() => cut.Find("#dashboard-metric-counter-title").TextContent.Should().Be(expectedTitle));
            cut.WaitForAssertion(() => cut.Find("#dashboard-metric-counter-value").TextContent.Should().Be(expectedCounter.ToString(CultureInfo.InvariantCulture)));
        }

        [Test]
        public void DashboardMetricCounterShouldRenderCorrectlyWithoutParameters()
        {
            // Act
            var cut = RenderComponent<DashboardMetricCounter>();

            // Assert
            cut.WaitForAssertion(() => cut.Find("#dashboard-metric-counter-icon"));
            cut.WaitForAssertion(() => cut.FindAll("svg").Count.Should().Be(0));
            cut.WaitForAssertion(() => cut.Find("#dashboard-metric-counter-title").TextContent.Should().BeEmpty());
            cut.WaitForAssertion(() => cut.Find("#dashboard-metric-counter-value").TextContent.Should().Be("0"));
        }
    }
}
