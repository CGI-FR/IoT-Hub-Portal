// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace AzureIoTHub.Portal.Tests.Unit.Server.Factories
{
    using System;
    using AzureIoTHub.Portal.Server.Exceptions;
    using AzureIoTHub.Portal.Server.Factories;
    using FluentAssertions;
    using NUnit.Framework;

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

        [Test]
        public void GetEdgeModuleCommandsShouldThrowInternalServerErrorExceptionWhenAnIssueOccurs()
        {
            // Arrange
            var connectionString = Guid.NewGuid().ToString();
            var tableClientFactory = new TableClientFactory(connectionString);

            // Act
            var act = () => tableClientFactory.GetEdgeModuleCommands();

            // Assert
            _ = act.Should().Throw<InternalServerErrorException>();
        }
    }
}
