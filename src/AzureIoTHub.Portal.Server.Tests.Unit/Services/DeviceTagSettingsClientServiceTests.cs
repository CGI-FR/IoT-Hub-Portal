// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace AzureIoTHub.Portal.Server.Tests.Unit.Services
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Net.Http;
    using System.Threading.Tasks;
    using AutoFixture;
    using AzureIoTHub.Portal.Client.Services;
    using FluentAssertions;
    using Helpers;
    using Microsoft.Extensions.DependencyInjection;
    using Models.v10;
    using NUnit.Framework;
    using RichardSzalay.MockHttp;

    [TestFixture]
    public class DeviceTagSettingsClientServiceTests : BlazorUnitTest
    {
        private IDeviceTagSettingsClientService deviceTagSettingsClientService;

        public override void Setup()
        {
            base.Setup();

            _ = Services.AddSingleton<IDeviceTagSettingsClientService, DeviceTagSettingsClientService>();

            this.deviceTagSettingsClientService = Services.GetRequiredService<IDeviceTagSettingsClientService>();
        }

        [Test]
        public async Task GetDeviceTagsShouldReturnDeviceTags()
        {
            // Arrange
            var expectedDeviceTags = Fixture.Build<DeviceTag>().CreateMany(3).ToList();

            _ = MockHttpClient.When(HttpMethod.Get, "/api/settings/device-tags")
                .RespondJson(expectedDeviceTags);

            // Act
            var result = await this.deviceTagSettingsClientService.GetDeviceTags();

            // Assert
            _ = result.Should().BeEquivalentTo(expectedDeviceTags);
            MockHttpClient.VerifyNoOutstandingRequest();
            MockHttpClient.VerifyNoOutstandingExpectation();
        }

        [Test]
        public async Task UpdateDeviceTagsShouldUpdateDeviceTags()
        {
            // Arrange
            var expectedDeviceTags = Fixture.Build<DeviceTag>().CreateMany(3).ToList();

            _ = MockHttpClient.When(HttpMethod.Get, "/api/settings/device-tags")
                .With(m =>
                {
                    _ = m.Content.Should().BeAssignableTo<ObjectContent<List<DeviceTag>>>();
                    var tags = m.Content as ObjectContent<List<DeviceTag>>;
                    _ = tags.Should().BeEquivalentTo(expectedDeviceTags);
                    return true;
                })
                .RespondJson(expectedDeviceTags);

            // Act
            await this.deviceTagSettingsClientService.UpdateDeviceTags(expectedDeviceTags);

            // Assert
            MockHttpClient.VerifyNoOutstandingRequest();
            MockHttpClient.VerifyNoOutstandingExpectation();
        }
    }
}
