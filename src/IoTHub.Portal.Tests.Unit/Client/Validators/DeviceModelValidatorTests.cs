// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace IoTHub.Portal.Tests.Unit.Client.Validators
{
    using System;
    using IoTHub.Portal.Client.Validators;
    using NUnit.Framework;
    using Shared.Models.v1._0;

    internal class DeviceModelValidatorTests
    {
        [Test]
        public void ValidateValidModel()
        {

            // Arrange
            var standardModelValidator = new DeviceModelValidator();
            var deviceModel = new DeviceModelDto()
            {
                Name = Guid.NewGuid().ToString(),
            };

            // Act
            var standardModelValidation = standardModelValidator.Validate(deviceModel);

            // Assert
            Assert.IsTrue(standardModelValidation.IsValid);
            Assert.AreEqual(0, standardModelValidation.Errors.Count);
        }

        [Test]
        public void ValidateMissingNameShouldReturnError()
        {
            // Arrange
            var standardModelValidator = new DeviceModelValidator();
            var deviceModel = new DeviceModelDto();

            // Act
            var standardModelValidation = standardModelValidator.Validate(deviceModel);

            // Assert
            Assert.IsFalse(standardModelValidation.IsValid);
            Assert.AreEqual(standardModelValidation.Errors[0].ErrorMessage, $"Model name is required.");
        }
    }
}
