// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace AzureIoTHub.Portal.Tests.Unit.Client.Components.Concentrators
{
    using AutoFixture;
    using System.Collections.Generic;
    using AzureIoTHub.Portal.Tests.Unit.UnitTests.Bases;
    using Bunit;
    using NUnit.Framework;
    using AzureIoTHub.Portal.Client.Components.Concentrators;
    using FluentAssertions;
    using System.Linq;
    using AzureIoTHub.Portal.Client.Models;

    [TestFixture]
    public class ConcentratorSearchTests : BlazorUnitTest
    {
        public override void Setup()
        {
            base.Setup();
        }

        [Test]
        public void SearchConcentrators_ClickOnSearch_SearchIsFired()
        {
            // Arrange
            var searchKeyword = Fixture.Create<string>();
            var receivedEvents = new List<ConcentratorSearchInfo>();
            var expectedConcentratorSearchInfo = new ConcentratorSearchInfo
            {
                SearchKeyword = searchKeyword,
                SearchState = string.Empty,
                SearchStatus = string.Empty
            };

            var cut = RenderComponent<ConcentratorSearch>(parameters => parameters.Add(p => p.OnSearch, (searchInfo) =>
            {
                receivedEvents.Add(searchInfo);
            }));

            cut.WaitForElement("#searchKeyword").Change(searchKeyword);
            cut.WaitForElement("#searchStatusAll").Click();
            cut.WaitForElement("#searchStateAll").Click();

            // Act
            cut.WaitForElement("#searchButton").Click();

            // Assert
            cut.WaitForAssertion(() => receivedEvents.Count.Should().Be(1));
            _ = receivedEvents.First().Should().BeEquivalentTo(expectedConcentratorSearchInfo);
            cut.WaitForAssertion(() => MockRepository.VerifyAll());
        }

        [Test]
        public void SearchConcentrators_ClickOnReset_SearchKeyworkIsSetToEmptyAndSearchIsFired()
        {
            // Arrange
            var searchKeyword = Fixture.Create<string>();
            var receivedEvents = new List<ConcentratorSearchInfo>();
            var expectedConcentratorSearchInfo = new ConcentratorSearchInfo
            {
                SearchKeyword = string.Empty,
                SearchState = string.Empty,
                SearchStatus = string.Empty
            };

            var cut = RenderComponent<ConcentratorSearch>(parameters => parameters.Add(p => p.OnSearch, (searchInfo) =>
            {
                receivedEvents.Add(searchInfo);
            }));

            cut.WaitForElement("#searchKeyword").Input(searchKeyword);
            cut.WaitForElement("#searchStatusAll").Click();
            cut.WaitForElement("#searchStateAll").Click();

            // Act
            cut.WaitForElement("#resetSearch").Click();

            // Assert
            cut.WaitForAssertion(() => receivedEvents.Count.Should().Be(1));
            _ = receivedEvents.First().Should().BeEquivalentTo(expectedConcentratorSearchInfo);
            _ = cut.Find("#searchKeyword").TextContent.Should().Be(string.Empty);
            cut.WaitForAssertion(() => MockRepository.VerifyAll());
        }
    }
}
