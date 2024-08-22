// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace IoTHub.Portal.Tests.Unit.Client.Validators
{
    using System.Collections.Generic;
    using IoTHub.Portal.Client.Validators;
    using NUnit.Framework;
    using Shared.Models.v1._0;

    [TestFixture]
    internal class IoTEdgeRouteValidatorTests
    {
        [Test]
        public void ValidateValidIoTEdgeRoute()
        {
            // Arrange
            var edgeRouteValidator = new IoTEdgeRouteValidator();
            var edgeRouteList = new List<IoTEdgeRoute>()
            {
                new IoTEdgeRoute()
                {
                    Name = "Route1",
                    Value = "FROM source WHERE condition INTO sink",
                    Priority = 1,
                    TimeToLive = 7200
                }
            };

            // Act
            var edgeRouteValidation = edgeRouteValidator.Validate(edgeRouteList);

            // Assert
            Assert.IsTrue(edgeRouteValidation.IsValid);
            Assert.AreEqual(0, edgeRouteValidation.Errors.Count);
        }

        [Test]
        public void ValidateNullRouteShouldReturnError()
        {
            // Arrange
            var edgeRouteValidator = new IoTEdgeRouteValidator();
            var edgeRouteList = new List<IoTEdgeRoute>()
            {
                null
            };

            // Act
            var edgeRouteValidation = edgeRouteValidator.Validate(edgeRouteList);

            // Assert
            Assert.IsFalse(edgeRouteValidation.IsValid);
            Assert.AreEqual(1, edgeRouteValidation.Errors.Count);
            Assert.AreEqual(edgeRouteValidation.Errors[0].ErrorMessage, $"Route cannot be null.");
        }

        [Test]
        public void ValidateMissingRequiredFieldShouldReturnError()
        {
            // Arrange
            var edgeRouteValidator = new IoTEdgeRouteValidator();
            var edgeRouteList = new List<IoTEdgeRoute>()
            {
                new IoTEdgeRoute()
                {
                    Name = null,
                    Value = null,
                }
            };

            // Act
            var edgeRouteValidation = edgeRouteValidator.Validate(edgeRouteList);

            // Assert
            Assert.IsFalse(edgeRouteValidation.IsValid);
            Assert.AreEqual(2, edgeRouteValidation.Errors.Count);
            Assert.AreEqual(edgeRouteValidation.Errors[0].ErrorMessage, $"The route name is required.");
            Assert.AreEqual(edgeRouteValidation.Errors[1].ErrorMessage, $"The route value is required.");
        }

        [TestCase(-1)]
        [TestCase(15)]
        [TestCase(42)]
        public void ValidateOutOfRangePriorityShouldReturnError(int priority)
        {
            // Arrange
            var edgeRouteValidator = new IoTEdgeRouteValidator();
            var edgeRouteList = new List<IoTEdgeRoute>()
            {
                new IoTEdgeRoute()
                {
                    Name = "Route1",
                    Value = "FROM source WHERE condition INTO sink",
                    Priority = priority,
                    TimeToLive = 7200
                }
            };

            // Act
            var edgeRouteValidation = edgeRouteValidator.Validate(edgeRouteList);

            // Assert
            Assert.IsFalse(edgeRouteValidation.IsValid);
            Assert.AreEqual(1, edgeRouteValidation.Errors.Count);
            Assert.AreEqual(edgeRouteValidation.Errors[0].ErrorMessage, $"The priority should be between 0 and 9.");
        }

        [TestCase("123456789")]
        [TestCase("FROM /messages/modules/<moduleId>/*")]
        public void ValidateWrongRouteFormatShouldReturnError(string route)
        {
            // Arrange
            var edgeRouteValidator = new IoTEdgeRouteValidator();
            var edgeRouteList = new List<IoTEdgeRoute>()
            {
                new IoTEdgeRoute()
                {
                    Name = "Route1",
                    Value = route,
                    Priority = 0,
                    TimeToLive = 7200
                }
            };

            // Act
            var edgeRouteValidation = edgeRouteValidator.Validate(edgeRouteList);

            // Assert
            Assert.IsFalse(edgeRouteValidation.IsValid);
            Assert.AreEqual(1, edgeRouteValidation.Errors.Count);
            Assert.AreEqual(edgeRouteValidation.Errors[0].ErrorMessage, "Route should be 'FROM <source> (WHERE <condition>) INTO <sink>'.");
        }
    }
}
