// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace AzureIoTHub.Portal.Tests.Unit.Client.Validators
{
    using AzureIoTHub.Portal.Client.Validators;
    using Models.v10.LoRaWAN;
    using NUnit.Framework;

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
