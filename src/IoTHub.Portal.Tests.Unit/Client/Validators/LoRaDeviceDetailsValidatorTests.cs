// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace IoTHub.Portal.Tests.Unit.Client.Validators
{
    using System;
    using IoTHub.Portal.Client.Validators;
    using NUnit.Framework;
    using Shared.Models.v1._0.LoRaWAN;

    [TestFixture]
    internal class LoRaDeviceDetailsValidatorTests
    {
        [Test]
        public void ValidateValidOTAADevice()
        {

            // Arrange
            var loraValidator = new LoRaDeviceDetailsValidator();
            var loraDevice = new LoRaDeviceDetails()
            {
                ModelId = Guid.NewGuid().ToString(),
                UseOTAA = true,
                AppEUI = Guid.NewGuid().ToString(),
                AppKey = Guid.NewGuid().ToString(),
                DeviceID = "0533AEC287B6E96B"
            };

            // Act
            var loraValidation = loraValidator.Validate(loraDevice);

            // Assert
            Assert.IsTrue(loraValidation.IsValid);
            Assert.AreEqual(0, loraValidation.Errors.Count);
        }

        [Test]
        public void ValidateValidABPDevice()
        {

            // Arrange
            var loraValidator = new LoRaDeviceDetailsValidator();
            var loraDevice = new LoRaDeviceDetails()
            {
                ModelId = Guid.NewGuid().ToString(),
                UseOTAA = false,
                AppSKey = Guid.NewGuid().ToString(),
                NwkSKey = Guid.NewGuid().ToString(),
                DevAddr = Guid.NewGuid().ToString(),
                DeviceID = "0533AEC287B6E96B"
            };

            // Act
            var loraValidation = loraValidator.Validate(loraDevice);

            // Assert
            Assert.IsTrue(loraValidation.IsValid);
            Assert.AreEqual(0, loraValidation.Errors.Count);
        }

        [Test]
        public void ValidateMissingModelIdShouldReturnError()
        {
            // Arrange
            var loraValidator = new LoRaDeviceDetailsValidator();
            var loraDevice = new LoRaDeviceDetails()
            {
                UseOTAA = false,
                AppSKey = Guid.NewGuid().ToString(),
                NwkSKey = Guid.NewGuid().ToString(),
                DevAddr = Guid.NewGuid().ToString(),
                DeviceID = "0533AEC287B6E96B"
            };

            // Act
            var loraValidation = loraValidator.Validate(loraDevice);

            // Assert
            Assert.IsFalse(loraValidation.IsValid);
            Assert.AreEqual(1, loraValidation.Errors.Count);
            Assert.AreEqual(loraValidation.Errors[0].ErrorMessage, "ModelId is required.");
        }

        [TestCase("AppSKey", "", "NwkSKeyValue", "DevAddrValue")]
        [TestCase("NwkSKey", "AppSKeyValue", "", "DevAddrValue")]
        [TestCase("DevAddr", "AppSKeyValue", "NwkSKeyValue", "")]
        public void ValidateMissingABPFieldShouldReturnError(
            string testedValue,
            string AppSkeyValue,
            string NwkSKeyValue,
            string DevAddrValue)
        {
            // Arrange
            var loraValidator = new LoRaDeviceDetailsValidator();
            var loraDevice = new LoRaDeviceDetails()
            {
                ModelId = Guid.NewGuid().ToString(),
                UseOTAA = false,
                AppSKey = AppSkeyValue,
                NwkSKey = NwkSKeyValue,
                DevAddr = DevAddrValue
            };

            // Act
            var loraValidation = loraValidator.Validate(loraDevice);

            // Assert
            Assert.IsFalse(loraValidation.IsValid);
            Assert.GreaterOrEqual(loraValidation.Errors.Count, 1);
            Assert.AreEqual(loraValidation.Errors[0].ErrorMessage, $"{testedValue} is required.");
        }

        [TestCase("AppEUI", "", "AppKeyValue")]
        [TestCase("AppKey", "AppEUIValue", "")]
        public void ValidateMissingOTAAFieldShouldReturnError(
            string testedValue,
            string AppEUIValue,
            string AppKeyValue)
        {
            // Arrange
            var loraValidator = new LoRaDeviceDetailsValidator();
            var loraDevice = new LoRaDeviceDetails()
            {
                ModelId = Guid.NewGuid().ToString(),
                UseOTAA = true,
                AppEUI = AppEUIValue,
                AppKey = AppKeyValue
            };

            // Act
            var loraValidation = loraValidator.Validate(loraDevice);

            // Assert
            Assert.IsFalse(loraValidation.IsValid);
            Assert.GreaterOrEqual(loraValidation.Errors.Count, 1);
            Assert.AreEqual(loraValidation.Errors[0].ErrorMessage, $"{testedValue} is required.");
        }
    }
}
