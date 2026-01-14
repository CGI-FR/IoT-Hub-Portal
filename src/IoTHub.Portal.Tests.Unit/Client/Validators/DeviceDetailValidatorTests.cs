// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace IoTHub.Portal.Tests.Unit.Client.Validators
{
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
                DeviceId = Guid.NewGuid().ToString(),
            };

            // Act
            var standardValidation = standardValidator.Validate(device);

            // Assert
            Assert.IsTrue(standardValidation.IsValid);
            Assert.AreEqual(0, standardValidation.Errors.Count);
        }

        [Test]
        public void ValidateValidAWSDevice()
        {

            // Arrange
            var standardValidator = new DeviceDetailsValidator(CloudProviders.Aws);
            var device = new DeviceDetails()
            {
                DeviceName = Guid.NewGuid().ToString(),
                ModelId = Guid.NewGuid().ToString()
            };

            // Act
            var standardValidation = standardValidator.Validate(device);

            // Assert
            Assert.IsTrue(standardValidation.IsValid);
            Assert.AreEqual(0, standardValidation.Errors.Count);
        }

        [TestCase("DeviceName", "", "ModelIdValue", "DeviceIDValue")]
        [TestCase("ModelId", "DeviceNameValue", "", "DeviceIDValue")]
        [TestCase("DeviceId", "DeviceNameValue", "ModelIdValue", "")]
        public void ValidateMissingFieldShouldReturnError(
            string testedValue,
            string DeviceNameValue,
            string ModelIdValue,
            string DeviceIDValue)
        {
            // Arrange
            var standardValidator = new DeviceDetailsValidator(CloudProviders.Azure);
            var device = new DeviceDetails()
            {
                DeviceName = DeviceNameValue,
                ModelId =ModelIdValue,
                DeviceId = DeviceIDValue,
            };

            // Act
            var standardValidation = standardValidator.Validate(device);

            // Assert
            Assert.IsFalse(standardValidation.IsValid);
            Assert.GreaterOrEqual(standardValidation.Errors.Count, 1);
            Assert.AreEqual(standardValidation.Errors[0].ErrorMessage, $"{testedValue} is required.");
        }

        [TestCase("DeviceName", "", "ModelIdValue", "DeviceIDValue")]
        [TestCase("ModelId", "DeviceNameValue", "", "DeviceIDValue")]
        public void ValidateMissingAWSFieldShouldReturnError(
            string testedValue,
            string DeviceNameValue,
            string ModelIdValue,
            string DeviceIDValue)
        {
            // Arrange
            var standardValidator = new DeviceDetailsValidator(CloudProviders.Aws);
            var device = new DeviceDetails()
            {
                DeviceName = DeviceNameValue,
                ModelId =ModelIdValue,
                DeviceId = DeviceIDValue,
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
            var standardValidator = new DeviceDetailsValidator(CloudProviders.Azure);
            var device = new DeviceDetails();

            // Act
            var standardValidation = standardValidator.Validate(device);

            // Assert
            Assert.IsFalse(standardValidation.IsValid);
            Assert.AreEqual(3, standardValidation.Errors.Count);
        }

        [Test]
        public void AWSValidateAllFieldsEmptyShouldReturnError()
        {
            // Arrange
            var standardValidator = new DeviceDetailsValidator(CloudProviders.Aws);
            var device = new DeviceDetails();

            // Act
            var standardValidation = standardValidator.Validate(device);

            // Assert
            Assert.IsFalse(standardValidation.IsValid);
            Assert.AreEqual(2, standardValidation.Errors.Count);
        }
    }
}
