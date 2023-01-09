// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace AzureIoTHub.Portal.Tests.Unit.Client.Components.EdgeModels
{
    using System.Collections.Generic;
    using System.Linq;
    using AutoFixture;
    using AzureIoTHub.Portal.Client.Components.EdgeModels;
    using Bunit;
    using FluentAssertions;
    using Moq;
    using NUnit.Framework;
    using UnitTests.Bases;

    [TestFixture]
    public class EdgeModelSearchTests : BlazorUnitTest
    {
        public override void Setup()
        {
            base.Setup();
        }

        [Test]
        public void SearchEdgeModels_ClickOnSearch_SearchIsFired()
        {
            // Arrange
            var searchKeyword = Fixture.Create<string>();
            var receivedEvents = new List<string>();

            var cut = RenderComponent<EdgeModelSearch>(parameters => parameters.Add(p => p.OnSearch, (s) =>
            {
                receivedEvents.Add(s);
            }));

            cut.WaitForElement("#edge-model-search-keyword").Change(searchKeyword);

            // Act
            cut.WaitForElement("#edge-model-search-button").Click();

            // Assert
            cut.WaitForAssertion(() => receivedEvents.Count.Should().Be(1));
            _ = receivedEvents.First().Should().Be(searchKeyword);
            cut.WaitForAssertion(() => MockRepository.VerifyAll());
        }

        [Test]
        public void SearchEdgeModels_ClickOnReset_SearchKeyworkIsSetToEmptyAndSearchIsFired()
        {
            // Arrange
            var searchKeyword = Fixture.Create<string>();
            var receivedEvents = new List<string>();

            var cut = RenderComponent<EdgeModelSearch>(parameters => parameters.Add(p => p.OnSearch, (s) =>
            {
                receivedEvents.Add(s);
            }));

            cut.WaitForElement("#edge-model-search-keyword").Input(searchKeyword);

            // Act
            cut.WaitForElement("#edge-model-search-reset-button").Click();

            // Assert
            cut.WaitForAssertion(() => receivedEvents.Count.Should().Be(1));
            _ = receivedEvents.First().Should().Be(string.Empty);
            _ = cut.Find("#edge-model-search-keyword").TextContent.Should().Be(string.Empty);
            cut.WaitForAssertion(() => MockRepository.VerifyAll());
        }
    }
}
