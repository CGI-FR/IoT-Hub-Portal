// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace AzureIoTHub.Portal.Tests.Unit.Client.Services
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Net;
    using System.Net.Http;
    using System.Threading.Tasks;
    using AutoFixture;
    using AzureIoTHub.Portal.Client.Services;
    using UnitTests.Bases;
    using UnitTests.Helpers;
    using FluentAssertions;
    using Microsoft.Extensions.DependencyInjection;
    using Models.v10;
    using NUnit.Framework;
    using RichardSzalay.MockHttp;

    [TestFixture]
    public class DeviceClientServiceTests : BlazorUnitTest
    {
        private IDeviceClientService deviceClientService;

        public override void Setup()
        {
            base.Setup();

            _ = Services.AddSingleton<IDeviceClientService, DeviceClientService>();

            this.deviceClientService = Services.GetRequiredService<IDeviceClientService>();
        }

        [Test]
        public async Task GetDevicesShouldReturnDevices()
        {
            // Arrange
            var uri = "/api/devices?pageSize=10&searchText=&searchStatus=&searchState=";

            var expectedDevices = new PaginationResult<DeviceListItem>
            {
                Items = new List<DeviceListItem>()
                {
                    new ()
                }
            };

            _ = MockHttpClient.When(HttpMethod.Get, uri)
                .RespondJson(expectedDevices);

            // Act
            var result = await this.deviceClientService.GetDevices(uri);

            // Assert
            _ = result.Should().BeEquivalentTo(expectedDevices);
            MockHttpClient.VerifyNoOutstandingRequest();
            MockHttpClient.VerifyNoOutstandingExpectation();
        }

        [Test]
        public async Task GetDeviceShouldReturnDevice()
        {
            // Arrange
            var deviceId = Fixture.Create<string>();

            var expectedDevice = new DeviceDetails
            {
                DeviceID = deviceId
            };

            _ = MockHttpClient.When(HttpMethod.Get, $"/api/devices/{deviceId}")
                .RespondJson(expectedDevice);

            // Act
            var result = await this.deviceClientService.GetDevice(deviceId);

            // Assert
            _ = result.Should().BeEquivalentTo(expectedDevice);
            MockHttpClient.VerifyNoOutstandingRequest();
            MockHttpClient.VerifyNoOutstandingExpectation();
        }

        [Test]
        public async Task CreateDeviceShouldCreateDevice()
        {
            // Arrange
            var device = new DeviceDetails();

            _ = MockHttpClient.When(HttpMethod.Post, "/api/devices")
                .With(m =>
                {
                    _ = m.Content.Should().BeAssignableTo<ObjectContent<DeviceDetails>>();
                    var body = m.Content as ObjectContent<DeviceDetails>;
                    _ = body.Value.Should().BeEquivalentTo(device);
                    return true;
                })
                .Respond(HttpStatusCode.Created);

            // Act
            await this.deviceClientService.CreateDevice(device);

            // Assert
            MockHttpClient.VerifyNoOutstandingRequest();
            MockHttpClient.VerifyNoOutstandingExpectation();
        }

        [Test]
        public async Task UpdateDeviceShouldUpdateDevice()
        {
            // Arrange
            var device = new DeviceDetails();

            _ = MockHttpClient.When(HttpMethod.Put, "/api/devices")
                .With(m =>
                {
                    _ = m.Content.Should().BeAssignableTo<ObjectContent<DeviceDetails>>();
                    var body = m.Content as ObjectContent<DeviceDetails>;
                    _ = body.Value.Should().BeEquivalentTo(device);
                    return true;
                })
                .Respond(HttpStatusCode.Created);

            // Act
            await this.deviceClientService.UpdateDevice(device);

            // Assert
            MockHttpClient.VerifyNoOutstandingRequest();
            MockHttpClient.VerifyNoOutstandingExpectation();
        }

        [Test]
        public async Task GetDevicePropertiesShouldReturnDeviceProperties()
        {
            // Arrange
            var deviceId = Fixture.Create<string>();
            var expectedDeviceProperties = Fixture.Build<DevicePropertyValue>().CreateMany(3).ToList();

            _ = MockHttpClient.When(HttpMethod.Get, $"/api/devices/{deviceId}/properties")
                .RespondJson(expectedDeviceProperties);

            // Act
            var result = await this.deviceClientService.GetDeviceProperties(deviceId);

            // Assert
            _ = result.Should().BeEquivalentTo(expectedDeviceProperties);
            MockHttpClient.VerifyNoOutstandingRequest();
            MockHttpClient.VerifyNoOutstandingExpectation();
        }

        [Test]
        public async Task SetDevicePropertiesShouldSetDeviceProperties()
        {
            // Arrange
            var deviceId = Fixture.Create<string>();
            var deviceProperties = Fixture.Build<DevicePropertyValue>().CreateMany(3).ToList();

            _ = MockHttpClient.When(HttpMethod.Post, $"/api/devices/{deviceId}/properties")
                .With(m =>
                {
                    _ = m.Content.Should().BeAssignableTo<ObjectContent<IList<DevicePropertyValue>>>();
                    var body = m.Content as ObjectContent<IList<DevicePropertyValue>>;
                    _ = body.Value.Should().BeEquivalentTo(deviceProperties);
                    return true;
                })
                .Respond(HttpStatusCode.Created);

            // Act
            await this.deviceClientService.SetDeviceProperties(deviceId, deviceProperties);

            // Assert
            MockHttpClient.VerifyNoOutstandingRequest();
            MockHttpClient.VerifyNoOutstandingExpectation();
        }

        [Test]
        public async Task GetEnrollmentCredentialsShouldReturnEnrollmentCredentials()
        {
            // Arrange
            var deviceId = Fixture.Create<string>();
            var expectedEnrollmentCredentials = Fixture.Create<EnrollmentCredentials>();

            _ = MockHttpClient.When(HttpMethod.Get, $"/api/devices/{deviceId}/credentials")
                .RespondJson(expectedEnrollmentCredentials);

            // Act
            var result = await this.deviceClientService.GetEnrollmentCredentials(deviceId);

            // Assert
            _ = result.Should().BeEquivalentTo(expectedEnrollmentCredentials);
            MockHttpClient.VerifyNoOutstandingRequest();
            MockHttpClient.VerifyNoOutstandingExpectation();
        }

        [Test]
        public async Task DeleteDeviceShouldDeleteDevice()
        {
            // Arrange
            var deviceId = Fixture.Create<string>();

            _ = MockHttpClient.When(HttpMethod.Delete, $"/api/devices/{deviceId}")
                .Respond(HttpStatusCode.NoContent);

            // Act
            await this.deviceClientService.DeleteDevice(deviceId);

            // Assert
            MockHttpClient.VerifyNoOutstandingRequest();
            MockHttpClient.VerifyNoOutstandingExpectation();
        }
    }
}
