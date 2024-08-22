// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace IoTHub.Portal.Tests.Unit.Client.Validators
{
    using System;
    using IoTHub.Portal.Client.Validators;
    using NUnit.Framework;
    using Shared.Models.v1._0.LoRaWAN;

    [TestFixture]
    internal class LoRaDeviceModelCommandValidatorTests
    {
        [Test]
        public void ValidateValidCommand()
        {

            // Arrange
            var cmdValidator = new LoRaDeviceModelCommandValidator();
            var commmand = new DeviceModelCommandDto()
            {
                Name = Guid.NewGuid().ToString(),
                Frame = "ABCDEF1234567890",
                Confirmed = false,
                Port = 1,
                IsBuiltin = false
            };

            // Act
            var cmdValidation = cmdValidator.Validate(commmand);

            // Assert
            Assert.IsTrue(cmdValidation.IsValid);
            Assert.AreEqual(0, cmdValidation.Errors.Count);
        }

        [Test]
        public void ValidateMissingNameShouldReturnError()
        {
            // Arrange
            var cmdValidator = new LoRaDeviceModelCommandValidator();
            var commmand = new DeviceModelCommandDto()
            {
                Frame = "ABCDEF1234567890",
                Port = 1
            };

            // Act
            var cmdValidation = cmdValidator.Validate(commmand);

            // Assert
            Assert.IsFalse(cmdValidation.IsValid);
            Assert.GreaterOrEqual(cmdValidation.Errors.Count, 1);
            Assert.AreEqual(cmdValidation.Errors[0].ErrorMessage, "The command name is required.");
        }

        [Test]
        public void ValidateMissingFrameShouldReturnError()
        {
            // Arrange
            var cmdValidator = new LoRaDeviceModelCommandValidator();
            var commmand = new DeviceModelCommandDto()
            {
                Name = Guid.NewGuid().ToString(),
                Port = 1
            };

            // Act
            var cmdValidation = cmdValidator.Validate(commmand);

            // Assert
            Assert.IsFalse(cmdValidation.IsValid);
            Assert.GreaterOrEqual(cmdValidation.Errors.Count, 1);
            Assert.AreEqual(cmdValidation.Errors[0].ErrorMessage, "The frame is required.");
        }

        [Test]
        public void ValidateWrongLengthFrameShouldReturnError()
        {
            var frameValue = new string('A', 256);
            // Arrange
            var cmdValidator = new LoRaDeviceModelCommandValidator();
            var commmand = new DeviceModelCommandDto()
            {
                Name = Guid.NewGuid().ToString(),
                Frame = frameValue,
                Port = 1
            };

            // Act
            var cmdValidation = cmdValidator.Validate(commmand);

            // Assert
            Assert.IsFalse(cmdValidation.IsValid);
            Assert.GreaterOrEqual(cmdValidation.Errors.Count, 1);
            Assert.AreEqual(cmdValidation.Errors[0].ErrorMessage, "The frame should be up to 255 characters long.");
        }

        [Test]
        public void ValidateInvalidFrameShouldReturnError()
        {
            var frameValue = "AZERTY";
            // Arrange
            var cmdValidator = new LoRaDeviceModelCommandValidator();
            var commmand = new DeviceModelCommandDto()
            {
                Name = Guid.NewGuid().ToString(),
                Frame = frameValue,
                Port = 1
            };

            // Act
            var cmdValidation = cmdValidator.Validate(commmand);

            // Assert
            Assert.IsFalse(cmdValidation.IsValid);
            Assert.GreaterOrEqual(cmdValidation.Errors.Count, 1);
            Assert.AreEqual(cmdValidation.Errors[0].ErrorMessage, "The frame should only contain hexadecimal characters");
        }

        [Test]
        public void ValidateInvalidPortShouldReturnError()
        {
            // Arrange
            var cmdValidator = new LoRaDeviceModelCommandValidator();
            var commmand = new DeviceModelCommandDto()
            {
                Name = Guid.NewGuid().ToString(),
                Frame = "ABCDEF1234567890",
                Port = 224
            };

            // Act
            var cmdValidation = cmdValidator.Validate(commmand);

            // Assert
            Assert.IsFalse(cmdValidation.IsValid);
            Assert.GreaterOrEqual(cmdValidation.Errors.Count, 1);
            Assert.AreEqual(cmdValidation.Errors[0].ErrorMessage, "The port number should be between 1 and 223.");
        }
    }
}
