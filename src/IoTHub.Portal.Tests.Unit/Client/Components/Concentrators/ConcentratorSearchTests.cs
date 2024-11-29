// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace IoTHub.Portal.Tests.Unit.Client.Components.Concentrators
{
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
                SearchText = searchKeyword,
                State = string.Empty,
                Status = string.Empty
            };

            var cut = RenderComponent<ConcentratorSearch>(parameters => parameters.Add(p => p.OnSearch, (searchInfo) =>
            {
                receivedEvents.Add(searchInfo);
            }));

            cut.WaitForElement("#searchKeyword").Change(searchKeyword);
            cut.WaitForElement("#searchStatusAll").Click();

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
                SearchText = string.Empty,
                State = string.Empty,
                Status = string.Empty
            };

            var cut = RenderComponent<ConcentratorSearch>(parameters => parameters.Add(p => p.OnSearch, (searchInfo) =>
            {
                receivedEvents.Add(searchInfo);
            }));

            cut.WaitForElement("#searchKeyword").Input(searchKeyword);
            cut.WaitForElement("#searchStatusAll").Click();

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
