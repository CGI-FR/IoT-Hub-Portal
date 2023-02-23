// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace AzureIoTHub.Portal.Tests.Unit.Client.Components.DevicesModels
{
    using AutoFixture;
    using AzureIoTHub.Portal.Client.Models;
    using System.Collections.Generic;
    using AzureIoTHub.Portal.Tests.Unit.UnitTests.Bases;
    using Bunit;
    using NUnit.Framework;
    using AzureIoTHub.Portal.Client.Components.DeviceModels;
    using FluentAssertions;
    using System.Linq;

    [TestFixture]
    public class DeviceModelSearchTests : BlazorUnitTest
    {
        public override void Setup()
        {
            base.Setup();
        }

        [Test]
        public void SearchDeviceModels_ClickOnSearch_SearchIsFired()
        {
            // Arrange
            var searchText = Fixture.Create<string>();
            var receivedEvents = new List<DeviceModelSearchInfo>();
            var expectedDeviceModelSearchInfo = new DeviceModelSearchInfo
            {
                SearchText = searchText
            };

            var cut = RenderComponent<DeviceModelSearch>(parameters => parameters.Add(p => p.OnSearch, (searchInfo) =>
            {
                receivedEvents.Add(searchInfo);
            }));

            cut.WaitForElement("#searchText").Change(searchText);

            // Act
            cut.WaitForElement("#searchButton").Click();

            // Assert
            cut.WaitForAssertion(() => receivedEvents.Count.Should().Be(1));
            _ = receivedEvents.First().Should().BeEquivalentTo(expectedDeviceModelSearchInfo);
            cut.WaitForAssertion(() => MockRepository.VerifyAll());
        }

        [Test]
        public void SearchDeviceModels_ClickOnReset_SearchTextIsSetToEmptyAndSearchIsFired()
        {
            // Arrange
            var searchText = Fixture.Create<string>();
            var receivedEvents = new List<DeviceModelSearchInfo>();
            var expectedDeviceModelSearchInfo = new DeviceModelSearchInfo
            {
                SearchText = string.Empty
            };

            var cut = RenderComponent<DeviceModelSearch>(parameters => parameters.Add(p => p.OnSearch, (searchInfo) =>
            {
                receivedEvents.Add(searchInfo);
            }));

            cut.WaitForElement("#searchText").Input(searchText);

            // Act
            cut.WaitForElement("#resetSearch").Click();

            // Assert
            cut.WaitForAssertion(() => receivedEvents.Count.Should().Be(1));
            _ = receivedEvents.First().Should().BeEquivalentTo(expectedDeviceModelSearchInfo);
            _ = cut.Find("#searchText").TextContent.Should().Be(string.Empty);
            cut.WaitForAssertion(() => MockRepository.VerifyAll());
        }
    }
}
