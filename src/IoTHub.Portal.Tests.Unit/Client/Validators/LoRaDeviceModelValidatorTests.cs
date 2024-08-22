// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace IoTHub.Portal.Tests.Unit.Client.Validators
{
    using IoTHub.Portal.Client.Validators;
    using NUnit.Framework;
    using Shared.Models.v1._0.LoRaWAN;

    internal class LoRaDeviceModelValidatorTests
    {
        [Test]
        public void ValidateValidOTAAModel()
        {

            // Arrange
            var loraModelValidator = new LoRaDeviceModelValidator();
            var loraModel = new LoRaDeviceModelDto()
            {
                UseOTAA = true,
                //AppEUI = Guid.NewGuid().ToString(),
            };

            // Act
            var loraModelValidation = loraModelValidator.Validate(loraModel);

            // Assert
            Assert.IsTrue(loraModelValidation.IsValid);
            Assert.AreEqual(0, loraModelValidation.Errors.Count);
        }

        //[Test]
        //public void ValidateMissingAppEUIFieldShouldReturnError()
        //{

        //    // Arrange
        //    var loraModelValidator = new LoRaDeviceModelValidator();
        //    var loraModel = new LoRaDeviceModel()
        //    {
        //        UseOTAA = true,
        //        //AppEUI = "",
        //    };

        //    // Act
        //    var loraModelValidation = loraModelValidator.Validate(loraModel);

        //    // Assert
        //    Assert.IsFalse(loraModelValidation.IsValid);
        //    Assert.AreEqual(1, loraModelValidation.Errors.Count);
        //    Assert.AreEqual(loraModelValidation.Errors[0].ErrorMessage, "AppEUI is required.");

        //}
    }
}
