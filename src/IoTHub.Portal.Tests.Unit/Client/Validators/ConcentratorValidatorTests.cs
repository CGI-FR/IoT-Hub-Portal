// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace IoTHub.Portal.Tests.Unit.Client.Validators
{
    using System;
    using IoTHub.Portal.Client.Validators;
    using NUnit.Framework;
    using Shared.Models.v1._0.LoRaWAN;

    internal class ConcentratorValidatorTests
    {
        [Test]
        public void ValidateValidConcentrator()
        {
            // Arrange
            var concentratorValidator = new ConcentratorValidator();
            var concentrator = new ConcentratorDto()
            {
                DeviceId = "0123456789abcdef",
                DeviceName = Guid.NewGuid().ToString(),
                LoraRegion = Guid.NewGuid().ToString()
            };

            // Act
            var concentratorValidation = concentratorValidator.Validate(concentrator);

            // Assert
            Assert.IsTrue(concentratorValidation.IsValid);
            Assert.AreEqual(0, concentratorValidation.Errors.Count);
        }

        [TestCase("DeviceName", "", "LoraRegionValue")]
        [TestCase("LoraRegion", "DeviceNameValue", "")]
        public void ValidateMissingFieldShouldReturnError(
            string testedValue,
            string DeviceNameValue,
            string LoraRegionValue)
        {
            // Arrange
            var concentratorValidator = new ConcentratorValidator();
            var concentrator = new ConcentratorDto()
            {
                DeviceId = "0123456789abcdef",
                DeviceName =DeviceNameValue,
                LoraRegion = LoraRegionValue,
            };

            // Act
            var concentratorValidation = concentratorValidator.Validate(concentrator);

            // Assert
            Assert.IsFalse(concentratorValidation.IsValid);
            Assert.AreEqual(1, concentratorValidation.Errors.Count);
            Assert.AreEqual(concentratorValidation.Errors[0].ErrorMessage, $"{testedValue} is required.");
        }

        [Test]
        public void ValidateMissingDeviceIdShouldReturnError()
        {
            // Arrange
            var concentratorValidator = new ConcentratorValidator();
            var concentrator = new ConcentratorDto()
            {
                DeviceId = "",
                DeviceName = Guid.NewGuid().ToString(),
                LoraRegion = Guid.NewGuid().ToString()
            };

            // Act
            var concentratorValidation = concentratorValidator.Validate(concentrator);

            // Assert
            Assert.IsFalse(concentratorValidation.IsValid);
            Assert.AreEqual(2, concentratorValidation.Errors.Count);
            Assert.AreEqual(concentratorValidation.Errors[0].ErrorMessage, $"DeviceId is required.");
            Assert.AreEqual(concentratorValidation.Errors[1].ErrorMessage, $"DeviceID must contain 16 hexadecimal characters.");

        }

        [Test]
        public void ValidateBadFormatDeviceIdShouldReturnError()
        {
            // Arrange
            var concentratorValidator = new ConcentratorValidator();
            var concentrator = new ConcentratorDto()
            {
                DeviceId = Guid.NewGuid().ToString(),
                DeviceName = Guid.NewGuid().ToString(),
                LoraRegion = Guid.NewGuid().ToString()
            };

            // Act
            var concentratorValidation = concentratorValidator.Validate(concentrator);

            // Assert
            Assert.IsFalse(concentratorValidation.IsValid);
            Assert.AreEqual(1, concentratorValidation.Errors.Count);
            Assert.AreEqual(concentratorValidation.Errors[0].ErrorMessage, $"DeviceID must contain 16 hexadecimal characters.");
        }

        [Test]
        public void ValidateBadFormatClientThumbprintShouldReturnError()
        {
            // Arrange
            var concentratorValidator = new ConcentratorValidator();
            var concentrator = new ConcentratorDto()
            {
                DeviceId = "0123456789abcdef",
                DeviceName = Guid.NewGuid().ToString(),
                LoraRegion = Guid.NewGuid().ToString(),
                ClientThumbprint = Guid.NewGuid().ToString()
            };

            // Act
            var concentratorValidation = concentratorValidator.Validate(concentrator);

            // Assert
            Assert.IsFalse(concentratorValidation.IsValid);
            Assert.AreEqual(1, concentratorValidation.Errors.Count);
            Assert.AreEqual(concentratorValidation.Errors[0].ErrorMessage, $"ClientThumbprint must contain 40 hexadecimal characters.");
        }

        [Test]
        public void ValidateAllFieldsEmptyShouldReturnError()
        {
            // Arrange
            var concentratorValidator = new ConcentratorValidator();
            var concentrator = new ConcentratorDto();

            // Act
            var concentratorValidation = concentratorValidator.Validate(concentrator);

            // Assert
            Assert.IsFalse(concentratorValidation.IsValid);
            Assert.AreEqual(4, concentratorValidation.Errors.Count);
        }
    }
}
