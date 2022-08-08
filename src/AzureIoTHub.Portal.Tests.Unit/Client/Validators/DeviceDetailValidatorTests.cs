// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace AzureIoTHub.Portal.Tests.Unit.Client.Validators
{
    using System;
    using AzureIoTHub.Portal.Client.Validators;
    using Models.v10;
    using NUnit.Framework;

    [TestFixture]
    internal class DeviceDetailsValidatorTests
    {
        [Test]
        public void ValidateValidDevice()
        {

            // Arrange
            var standardValidator = new DeviceDetailsValidator();
            var device = new DeviceDetails()
            {
                DeviceName = Guid.NewGuid().ToString(),
                ModelId = Guid.NewGuid().ToString(),
                DeviceID = Guid.NewGuid().ToString(),
            };

            // Act
            var standardValidation = standardValidator.Validate(device);

            // Assert
            Assert.IsTrue(standardValidation.IsValid);
            Assert.AreEqual(0, standardValidation.Errors.Count);
        }

        [TestCase("DeviceName", "", "ModelIdValue", "DeviceIDValue")]
        [TestCase("ModelId", "DeviceNameValue", "", "DeviceIDValue")]
        [TestCase("DeviceID", "DeviceNameValue", "ModelIdValue", "")]
        public void ValidateMissingFieldShouldReturnError(
            string testedValue,
            string DeviceNameValue,
            string ModelIdValue,
            string DeviceIDValue)
        {
            // Arrange
            var standardValidator = new DeviceDetailsValidator();
            var device = new DeviceDetails()
            {
                DeviceName = DeviceNameValue,
                ModelId =ModelIdValue,
                DeviceID = DeviceIDValue,
            };

            // Act
            var standardValidation = standardValidator.Validate(device);

            // Assert
            Assert.IsFalse(standardValidation.IsValid);
            Assert.GreaterOrEqual(standardValidation.Errors.Count, 1);
            Assert.AreEqual(standardValidation.Errors[0].ErrorMessage, $"{testedValue} is required.");
        }

        [Test]
        public void ValidateAllFieldsEmptyShouldReturnError()
        {
            // Arrange
            var standardValidator = new DeviceDetailsValidator();
            var device = new DeviceDetails();

            // Act
            var standardValidation = standardValidator.Validate(device);

            // Assert
            Assert.IsFalse(standardValidation.IsValid);
            Assert.AreEqual(3, standardValidation.Errors.Count);
        }
    }
}
