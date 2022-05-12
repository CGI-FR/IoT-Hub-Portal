// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace AzureIoTHub.Portal.Server.Tests.Unit.Extensions
{
    using System;
    using System.Linq;
    using System.Text;
    using AzureIoTHub.Portal.Models.v10.LoRaWAN;
    using FluentAssertions;
    using NUnit.Framework;
    using Server.Extensions;

    [TestFixture]
    public class DeviceModelCommandExtensionsTests
    {

        [Test]
        public void ToDynamicMustNotIncludeConfirmedPropertyWhenConfirmedIsFalse()
        {
            // Arrange
            var command = new DeviceModelCommand
            {
                Name = Guid.NewGuid().ToString(),
                Frame = Guid.NewGuid().ToString(),
                Port = 123,
                Confirmed = false,
                IsBuiltin = false
            };

            // Act
            var result = command.ToDynamic();

            // Assert
            _ = ((Type)result.GetType()).GetProperties()
                .Where(p => p.Name.Equals("confirmed", StringComparison.Ordinal))
                .Any()
                .Should()
                .BeFalse();

            _ = ((string)result.rawPayload).Should().Be(Convert.ToBase64String(Encoding.UTF8.GetBytes(command.Frame)));
            _ = ((int)result.fport).Should().Be(command.Port);
        }

        [Test]
        public void ToDynamicMustIncludeConfirmedPropertyWhenConfirmedIsTrue()
        {
            // Arrange
            var command = new DeviceModelCommand
            {
                Name = Guid.NewGuid().ToString(),
                Frame = Guid.NewGuid().ToString(),
                Port = 123,
                Confirmed = true,
                IsBuiltin = false
            };

            // Act
            var result = command.ToDynamic();

            // Assert
            _ = ((bool)result.confirmed).Should().BeTrue();
            _ = ((string)result.rawPayload).Should().Be(Convert.ToBase64String(Encoding.UTF8.GetBytes(command.Frame)));
            _ = ((int)result.fport).Should().Be(command.Port);
        }
    }
}
