// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace AzureIoTHub.Portal.Server.Tests.Unit.Pages.Shared
{
    using Bunit;
    using Client.Shared;
    using FluentAssertions;
    using Microsoft.Extensions.DependencyInjection;
    using Models.v10;
    using NUnit.Framework;

    [TestFixture]
    public class PortalFooterTests : BlazorUnitTest
    {
        [Test]
        public void PortalFooterShouldRenderCopyrightYearAndVersion()
        {
            // Arrange
            var portalSettings = new PortalSettings
            {
                CopyrightYear = "9999",
                Version = "1.2.3"
            };

            _ = Services.AddSingleton(portalSettings);

            var expectedCopyright = $"© {portalSettings.CopyrightYear} Copyright:  CGI France - {portalSettings.Version}";

            // Act
            var cut = RenderComponent<PortalFooter>();

            // Assert
            cut.WaitForAssertion(() => cut.FindAll("p").Count.Should().Be(1));
            cut.WaitForAssertion(() => cut.Find("p").TextContent.Should().Be(expectedCopyright));
        }
    }
}
