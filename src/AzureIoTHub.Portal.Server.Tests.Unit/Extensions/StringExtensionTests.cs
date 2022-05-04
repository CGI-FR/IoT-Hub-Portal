// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace AzureIoTHub.Portal.Server.Tests.Unit.Extensions
{
    using FluentAssertions;
    using NUnit.Framework;
    using Server.Extensions;

    [TestFixture]
    public class StringExtensionTests
    {
        [TestCase("/*-+&\"'(-_)=}]@^\\`|[{# ", "-----------------------")]
        [TestCase("éèàçîïâäç", "eeaciiaac")]
        public void RemoveDiacriticsStateUnderTestExpectedBehavior(string input, string expected)
        {
            // Act
            var result = input.RemoveDiacritics();

            // Assert
            Assert.AreEqual(expected, result);
        }

        [Test]
        public void WhenInputIsNullRemoveDiacriticsShouldReturnNull()
        {
            const string test = null;

            // Act
            var result = test.RemoveDiacritics();

            // Assert
            Assert.IsNull(result);
        }

        [TestCase("^")]
        [TestCase(".")]
        [TestCase("$")]
        [TestCase("#")]
        [TestCase(" ")]
        public void KeepAuthorizedCharactersMustReplaceProhibitedCharactersByEmpty(string prohibitedCharacter)
        {
            var input = $"firmware{prohibitedCharacter}url";
            var expectedInput = "firmwareurl";

            // Act
            var result = input.KeepAuthorizedCharacters();

            // Assert
            _ = result.Should().Be(expectedInput);
        }
    }
}
