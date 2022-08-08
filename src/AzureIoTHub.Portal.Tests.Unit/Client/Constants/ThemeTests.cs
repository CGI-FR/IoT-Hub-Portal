// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace AzureIoTHub.Portal.Tests.Unit.Client.Constants
{
    using AzureIoTHub.Portal.Client.Constants;
    using FluentAssertions;
    using NUnit.Framework;

    [TestFixture]
    public class ThemeTests
    {
        [Test]
        public void PaletteBackgroundColorOfCurrentThemeShouldBeCorrect()
        {
            // Arrange
            const string expectedPaletteBackground = "#efefefff";

            // Act
            var currentTheme = Theme.CurrentTheme;

            // Assert
            _ = currentTheme.Palette.Background.ToString().Should().Be(expectedPaletteBackground);
        }
    }
}
