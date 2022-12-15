// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace AzureIoTHub.Portal.Tests.Unit.Client.Services
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Blazored.LocalStorage;
    using FluentAssertions;
    using Moq;
    using NUnit.Framework;
    using Portal.Client.Constants;
    using Portal.Client.Services;

    [TestFixture]
    public class LayoutServiceTests
    {
        private MockRepository mockRepository;
        private Mock<ILocalStorageService> mockLocalStorageService;

        [SetUp]
        public void SetUp()
        {
            this.mockRepository = new MockRepository(MockBehavior.Strict);
            this.mockLocalStorageService = this.mockRepository.Create<ILocalStorageService>();
        }

        [Test]
        public void ToggleDrawerShouldUpdateTheValueOfIsDrawerOpenAndRaiseMajorUpdateOccurredEvent()
        {
            // Arrange
            var receivedEvents = new List<string>();
            var layoutService = new LayoutService(this.mockLocalStorageService.Object)
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

        [Test]
        public async Task ToggleDarkModeShouldUpdateTheValueOfIsDarkModeAndRaiseMajorUpdateOccurredEvent()
        {
            // Arrange
            _ = this.mockLocalStorageService.Setup(x => x.SetItemAsync(LocalStorageKey.DarkTheme, true, It.IsAny<CancellationToken>()))
                .Returns(ValueTask.CompletedTask);

            var receivedEvents = new List<string>();
            var layoutService = new LayoutService(this.mockLocalStorageService.Object)
            {
                IsDarkMode = false
            };
            layoutService.MajorUpdateOccurred += (sender, args) =>
            {
                receivedEvents.Add(sender?.GetType().ToString());
            };

            // Act
            await layoutService.ToggleDarkMode();

            // Assert
            _ = layoutService.IsDarkMode.Should().BeTrue();
            _ = receivedEvents.Count.Should().Be(1);
            _ = receivedEvents.First().Should().Be(typeof(LayoutService).ToString());
            this.mockRepository.VerifyAll();
        }

        [Test]
        public void SetDarkModeShouldUpdateIsDarkMode()
        {
            // Arrange
            var layoutService = new LayoutService(this.mockLocalStorageService.Object)
            {
                IsDarkMode = false
            };

            // Act
            layoutService.SetDarkMode(true);

            // Assert
            _ = layoutService.IsDarkMode.Should().BeTrue();
        }

        [Test]
        public void GetNavGroupExpandedMustReturnTrueWhenNavGroupKeyDoesNotExist()
        {
            // Arrange
            var layoutService = new LayoutService(this.mockLocalStorageService.Object);

            // Act
            var result = layoutService.GetNavGroupExpanded(Guid.NewGuid().ToString());

            // Assert
            _ = result.Should().BeTrue();
        }

        [Test]
        public void GetNavGroupExpandedMustReturnTrueWhenCollapsibleNavMenuIsNull()
        {
            // Arrange
            var layoutService = new LayoutService(this.mockLocalStorageService.Object)
            {
                CollapsibleNavMenu = null
            };

            // Act
            var result = layoutService.GetNavGroupExpanded(Guid.NewGuid().ToString());

            // Assert
            _ = result.Should().BeTrue();
        }

        [Test]
        public void GetNavGroupExpandedMustReturnCorrectValueOfExistingNavGroupKeys()
        {
            // Arrange
            var navGroupKey1 = Guid.NewGuid().ToString();
            var navGroupKey2 = Guid.NewGuid().ToString();

            var layoutService = new LayoutService(this.mockLocalStorageService.Object)
            {
                CollapsibleNavMenu = new Dictionary<string, bool>
                {
                    { navGroupKey1, false },
                    { navGroupKey2, true }
                }
            };

            // Act
            var result1 = layoutService.GetNavGroupExpanded(navGroupKey1);
            var result2 = layoutService.GetNavGroupExpanded(navGroupKey2);

            // Assert
            _ = result1.Should().BeFalse();
            _ = result2.Should().BeTrue();
        }

        [Test]
        public async Task SetNavGroupExpandedMustAddNewNavGroupKey()
        {
            // Arrange
            var navGroupKey = Guid.NewGuid().ToString();

            var layoutService = new LayoutService(this.mockLocalStorageService.Object);

            _ = this.mockLocalStorageService.Setup(x => x.SetItemAsync(LocalStorageKey.CollapsibleNavMenu, It.IsAny<Dictionary<string, bool>>(), It.IsAny<CancellationToken>()))
                .Returns(ValueTask.CompletedTask);

            // Act
            await layoutService.SetNavGroupExpanded(navGroupKey, true);

            // Assert
            _ = layoutService.CollapsibleNavMenu.Should().ContainKey(navGroupKey);
            _ = layoutService.CollapsibleNavMenu[navGroupKey].Should().BeTrue();
            this.mockRepository.VerifyAll();
        }

        [Test]
        public async Task SetNavGroupExpandedMustUpdateExistingNavGroupKey()
        {
            // Arrange
            var navGroupKey = Guid.NewGuid().ToString();

            var layoutService = new LayoutService(this.mockLocalStorageService.Object)
            {
                CollapsibleNavMenu = new Dictionary<string, bool>
                {
                    { navGroupKey, false }
                }
            };

            _ = this.mockLocalStorageService.Setup(x => x.SetItemAsync(LocalStorageKey.CollapsibleNavMenu, It.IsAny<Dictionary<string, bool>>(), It.IsAny<CancellationToken>()))
                .Returns(ValueTask.CompletedTask);

            // Act
            await layoutService.SetNavGroupExpanded(navGroupKey, true);

            // Assert
            _ = layoutService.CollapsibleNavMenu.Should().ContainKey(navGroupKey);
            _ = layoutService.CollapsibleNavMenu[navGroupKey].Should().BeTrue();
            this.mockRepository.VerifyAll();
        }
    }
}
