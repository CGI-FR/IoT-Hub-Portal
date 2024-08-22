// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace IoTHub.Portal.Tests.Unit.Client.Validators
{
    using System;
    using System.Collections.Generic;
    using IoTHub.Portal.Client.Validators;
    using NUnit.Framework;
    using Shared.Models;
    using Shared.Models.v1._0;

    internal class DevicePropertyValidatorTests
    {
        [Test]
        public void ValidateValidProperties()
        {

            // Arrange
            var propertiesValidator = new DevicePropertyValidator();
            var properties = new List<DeviceProperty>(){
                new DeviceProperty()
                {
                    DisplayName = Guid.NewGuid().ToString(),
                    Name = Guid.NewGuid().ToString(),
                    PropertyType = DevicePropertyType.Integer,
                    IsWritable = true
                },
                new DeviceProperty()
                {
                    DisplayName = Guid.NewGuid().ToString(),
                    Name = Guid.NewGuid().ToString(),
                    PropertyType = DevicePropertyType.Integer,
                    IsWritable = true
                }
            };

            // Act
            var propertiesValidation = propertiesValidator.Validate(properties);

            // Assert
            Assert.IsTrue(propertiesValidation.IsValid);
            Assert.AreEqual(0, propertiesValidation.Errors.Count);
        }

        [TestCase("DisplayName", "", "NameValue")]
        [TestCase("Name", "DisplayNameValue", "")]
        public void ValidateMissingFieldShouldReturnError(
            string testedValue,
            string DisplayNameValue,
            string NameValue)
        {

            // Arrange
            var propertiesValidator = new DevicePropertyValidator();
            var properties = new List<DeviceProperty>(){
                new DeviceProperty()
                {
                    DisplayName = DisplayNameValue,
                    Name = NameValue,
                    PropertyType = DevicePropertyType.Integer,
                    IsWritable = true
                }
            };

            // Act
            var propertiesValidation = propertiesValidator.Validate(properties);

            // Assert
            Assert.IsFalse(propertiesValidation.IsValid);
            Assert.AreEqual(1, propertiesValidation.Errors.Count);
            Assert.AreEqual(propertiesValidation.Errors[0].ErrorMessage, $"Property {testedValue} is required.");
        }

        [Test]
        public void ValidateAllFieldsEmptyShouldReturnError()
        {

            // Arrange
            var propertiesValidator = new DevicePropertyValidator();
            var properties = new List<DeviceProperty>(){
                new DeviceProperty(),
            };

            // Act
            var propertiesValidation = propertiesValidator.Validate(properties);

            // Assert
            Assert.IsFalse(propertiesValidation.IsValid);
            Assert.AreEqual(4, propertiesValidation.Errors.Count);
        }

        [Test]
        public void ValidateNullPropertyShouldReturnError()
        {

            // Arrange
            var propertiesValidator = new DevicePropertyValidator();
            var properties = new List<DeviceProperty>(){
                null,
            };

            // Act
            var propertiesValidation = propertiesValidator.Validate(properties);

            // Assert
            Assert.IsFalse(propertiesValidation.IsValid);
            Assert.AreEqual(1, propertiesValidation.Errors.Count);
            Assert.AreEqual(propertiesValidation.Errors[0].ErrorMessage, "Property cannot be null.");

        }

        [Test]
        public void ValidateDuplicateNamesShouldReturnError()
        {

            // Arrange
            var propertiesValidator = new DevicePropertyValidator();
            var properties = new List<DeviceProperty>(){
                new DeviceProperty()
                {
                    DisplayName = Guid.NewGuid().ToString(),
                    Name = "PropertyWithSameName",
                    PropertyType = DevicePropertyType.Integer,
                    IsWritable = true
                },
                new DeviceProperty()
                {
                    DisplayName = Guid.NewGuid().ToString(),
                    Name = "PropertyWithSameName",
                    PropertyType = DevicePropertyType.Integer,
                    IsWritable = true
                }
            };

            // Act
            var propertiesValidation = propertiesValidator.Validate(properties);

            // Assert
            Assert.IsFalse(propertiesValidation.IsValid);
            Assert.AreEqual(1, propertiesValidation.Errors.Count);
            Assert.AreEqual(propertiesValidation.Errors[0].ErrorMessage, "Properties should have unique name.");

        }
    }
}
