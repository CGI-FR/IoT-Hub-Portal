// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace IoTHub.Portal.Tests.Unit.Client.Services
{
    using System.Linq;
    using System.Net.Http;
    using System.Threading.Tasks;
    using AutoFixture;
    using IoTHub.Portal.Client.Services;
    using UnitTests.Bases;
    using UnitTests.Helpers;
    using FluentAssertions;
    using Microsoft.Extensions.DependencyInjection;
    using NUnit.Framework;
    using RichardSzalay.MockHttp;
    using Shared.Models.v1._0;

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
            var expectedDeviceTags = Fixture.Build<DeviceTagDto>().CreateMany(3).ToList();

            _ = MockHttpClient.When(HttpMethod.Get, "/api/settings/device-tags")
                .RespondJson(expectedDeviceTags);

            // Act
            var result = await this.deviceTagSettingsClientService.GetDeviceTags();

            // Assert
            _ = result.Should().BeEquivalentTo(expectedDeviceTags);
            MockHttpClient.VerifyNoOutstandingRequest();
            MockHttpClient.VerifyNoOutstandingExpectation();
        }

        //[Test]
        //public async Task UpdateDeviceTagsShouldUpdateDeviceTags()
        //{
        //    // Arrange
        //    var expectedDeviceTags = Fixture.Build<DeviceTag>().CreateMany(3).ToList();

        //    _ = MockHttpClient.When(HttpMethod.Post, "/api/settings/device-tags")
        //        .With(m =>
        //        {
        //            _ = m.Content.Should().BeAssignableTo<ObjectContent<IList<DeviceTag>>>();
        //            var tags = m.Content as ObjectContent<IList<DeviceTag>>;
        //            _ = tags.Value.Should().BeEquivalentTo(expectedDeviceTags);
        //            return true;
        //        })
        //        .Respond(HttpStatusCode.Created);

        //    // Act
        //    await this.deviceTagSettingsClientService.UpdateDeviceTags(expectedDeviceTags);

        //    // Assert
        //    MockHttpClient.VerifyNoOutstandingRequest();
        //    MockHttpClient.VerifyNoOutstandingExpectation();
        //}
    }
}
