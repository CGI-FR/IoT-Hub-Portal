// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace AzureIoTHub.Portal.Server.Tests.Unit.Services
{
    using System;
    using System.Collections.Generic;
    using System.Net.Http;
    using System.Threading.Tasks;
    using AutoFixture;
    using AzureIoTHub.Portal.Client.Services;
    using Models.v10;
    using FluentAssertions;
    using Helpers;
    using Microsoft.Extensions.DependencyInjection;
    using NUnit.Framework;
    using RichardSzalay.MockHttp;
    using System.Net;

    [TestFixture]
    public class EdgeDeviceClientServiceTests : BlazorUnitTest
    {
        private IEdgeDeviceClientService edgeDeviceClientService;

        public override void Setup()
        {
            base.Setup();

            _ = Services.AddSingleton<IEdgeDeviceClientService, EdgeDeviceClientService>();

            this.edgeDeviceClientService = Services.GetRequiredService<IEdgeDeviceClientService>();
        }

        [Test]
        public async Task GetDevicesShouldReturnDevices()
        {
            // Arrange
            var expectedDevices = new PaginationResult<IoTEdgeListItem>
            {
                Items = new List<IoTEdgeListItem>()
                {
                    new ()
                }
            };

            var expectedUri = "/api/edge/devices?pageSize=10";

            _ = MockHttpClient.When(HttpMethod.Get, expectedUri)
                .RespondJson(expectedDevices);

            // Act
            var result = await this.edgeDeviceClientService.GetDevices(expectedUri);

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

            var expectedDevice = new IoTEdgeDevice
            {
                DeviceId = deviceId
            };

            _ = MockHttpClient.When(HttpMethod.Get, $"/api/edge/devices/{deviceId}")
                .RespondJson(expectedDevice);

            // Act
            var result = await this.edgeDeviceClientService.GetDevice(deviceId);

            // Assert
            _ = result.Should().BeEquivalentTo(expectedDevice);
            MockHttpClient.VerifyNoOutstandingRequest();
            MockHttpClient.VerifyNoOutstandingExpectation();
        }

        [Test]
        public async Task CreateDeviceShouldCreateDevice()
        {
            // Arrange
            var device = new IoTEdgeDevice
            {
                DeviceId = Fixture.Create<string>()
            };

            _ = MockHttpClient.When(HttpMethod.Post, "/api/edge/devices")
                .With(m =>
                {
                    _ = m.Content.Should().BeAssignableTo<ObjectContent<IoTEdgeDevice>>();
                    var body = m.Content as ObjectContent<IoTEdgeDevice>;
                    _ = body.Value.Should().BeEquivalentTo(device);
                    return true;
                })
                .Respond(HttpStatusCode.Created);

            // Act
            await this.edgeDeviceClientService.CreateDevice(device);

            // Assert
            MockHttpClient.VerifyNoOutstandingRequest();
            MockHttpClient.VerifyNoOutstandingExpectation();
        }

        [Test]
        public async Task UpdateDeviceShouldUpdateDevice()
        {
            // Arrange
            var device = new IoTEdgeDevice
            {
                DeviceId = Fixture.Create<string>()
            };

            _ = MockHttpClient.When(HttpMethod.Put, $"/api/edge/devices/{device.DeviceId}")
                .With(m =>
                {
                    _ = m.Content.Should().BeAssignableTo<ObjectContent<IoTEdgeDevice>>();
                    var body = m.Content as ObjectContent<IoTEdgeDevice>;
                    _ = body.Value.Should().BeEquivalentTo(device);
                    return true;
                })
                .Respond(HttpStatusCode.Created);

            // Act
            await this.edgeDeviceClientService.UpdateDevice(device);

            // Assert
            MockHttpClient.VerifyNoOutstandingRequest();
            MockHttpClient.VerifyNoOutstandingExpectation();
        }

        [Test]
        public async Task DeleteDeviceShouldDeleteDevice()
        {
            // Arrange
            var deviceId = Fixture.Create<string>();

            _ = MockHttpClient.When(HttpMethod.Delete, $"/api/edge/devices/{deviceId}")
                .Respond(HttpStatusCode.NoContent);

            // Act
            await this.edgeDeviceClientService.DeleteDevice(deviceId);

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

            _ = MockHttpClient.When(HttpMethod.Get, $"/api/edge/devices/{deviceId}/credentials")
                .RespondJson(expectedEnrollmentCredentials);

            // Act
            var result = await this.edgeDeviceClientService.GetEnrollmentCredentials(deviceId);

            // Assert
            _ = result.Should().BeEquivalentTo(expectedEnrollmentCredentials);
            MockHttpClient.VerifyNoOutstandingRequest();
            MockHttpClient.VerifyNoOutstandingExpectation();
        }

        [Test]
        public async Task GetEdgeDeviceLogsMustReturnLogsWhenNoError()
        {
            // Arrange
            var deviceId = Guid.NewGuid().ToString();

            var edgeModule = new IoTEdgeModule
            {
                Version = "1.0",
                ModuleName = Guid.NewGuid().ToString()
            };

            var expectedLog = new IoTEdgeDeviceLog
            {
                Id = deviceId,
                Text = Guid.NewGuid().ToString(),
                LogLevel = 1,
                TimeStamp = DateTime.UtcNow
            };

            var expectedLogs = new List<IoTEdgeDeviceLog>
            {
                expectedLog
            };

            _ = MockHttpClient.When(HttpMethod.Post, $"/api/edge/devices/{deviceId}/logs")
                .RespondJson(expectedLogs);

            // Act
            var result = await this.edgeDeviceClientService.GetEdgeDeviceLogs(deviceId, edgeModule);

            // Assert
            _ = result.Should().NotBeNull();

            _ = result.Count.Should().Be(1);
        }
    }
}
