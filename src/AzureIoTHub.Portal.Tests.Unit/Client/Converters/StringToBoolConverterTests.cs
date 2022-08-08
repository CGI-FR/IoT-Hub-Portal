// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace AzureIoTHub.Portal.Tests.Unit.Client.Converters
{
    using AzureIoTHub.Portal.Client.Converters;
    using Moq;
    using NUnit.Framework;

    [TestFixture]
    public class StringToBoolConverterTests
    {
        private MockRepository mockRepository;

        [SetUp]
        public void SetUp()
        {
            this.mockRepository = new MockRepository(MockBehavior.Strict);
        }

        private static StringToBoolConverter CreateStringToBoolConverter()
        {
            return new StringToBoolConverter();
        }

        [TestCase(true, "true")]
        [TestCase(false, "false")]
        [TestCase(null, null)]
        public void GetShouldReturnStringRepresentationOfBool(bool? value, string expected)
        {
            // Arrange
            var stringToBoolConverter = CreateStringToBoolConverter();

            // Act
            var result = stringToBoolConverter.Get(value);

            // Assert
            Assert.AreEqual(expected, result);
            this.mockRepository.VerifyAll();
        }

        [TestCase("true", true)]
        [TestCase("false", false)]
        [TestCase("True", true)]
        [TestCase("False", false)]
        [TestCase("TRUE", true)]
        [TestCase("FALSE", false)]
        [TestCase("1", null)]
        [TestCase(null, null)]
        public void SetShouldReturnStringRepresentationOfBool(string value, bool? expected)
        {
            // Arrange
            var stringToBoolConverter = CreateStringToBoolConverter();

            // Act
            var result = stringToBoolConverter.Set(value);

            // Assert
            Assert.AreEqual(expected, result);
            this.mockRepository.VerifyAll();
        }
    }
}
