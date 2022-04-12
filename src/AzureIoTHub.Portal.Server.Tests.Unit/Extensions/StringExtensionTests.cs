// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace AzureIoTHub.Portal.Server.Tests.Unit.Extensions
{
    using NUnit.Framework;
    using Server.Extensions;

    [TestFixture]
    public class StringExtensionTests
    {
        [TestCase("/*-+&\"'(-_)=}]@^\\`|[{# ", "-----------------------")]
        [TestCase("éèàçîïâäç", "eeaciiaac")]
        public void RemoveDiacritics_StateUnderTest_ExpectedBehavior(string input, string expected)
        {
            // Act
            var result = input.RemoveDiacritics();

            // Assert
            Assert.AreEqual(expected, result);
        }
    }
}
