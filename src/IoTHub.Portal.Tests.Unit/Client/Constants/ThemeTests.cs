// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace IoTHub.Portal.Tests.Unit.Client.Constants
{
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
            _ = currentTheme.Palette.GetType().Should().Be(typeof(PaletteLight));
            _ = currentTheme.Palette.Background.Value.Should().Be(expectedPaletteBackground);
        }
    }
}
