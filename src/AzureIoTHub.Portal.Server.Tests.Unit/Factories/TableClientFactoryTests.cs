// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace AzureIoTHub.Portal.Server.Tests.Unit.Factories
{
    using System;
    using FluentAssertions;
    using NUnit.Framework;
    using Server.Exceptions;
    using Server.Factories;

    public class TableClientFactoryTests
    {
        [Test]
        public void GetDeviceTemplatesShouldThrowInternalServerErrorExceptionWhenAnIssueOccurs()
        {
            // Arrange
            var connectionString = Guid.NewGuid().ToString();
            var tableClientFactory = new TableClientFactory(connectionString);

            // Act
            var act = () => tableClientFactory.GetDeviceTemplates();

            // Assert
            _ = act.Should().Throw<InternalServerErrorException>();
        }
    }
}
