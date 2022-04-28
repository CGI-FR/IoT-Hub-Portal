// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace AzureIoTHub.Portal.Server.Tests.Unit.Services
{
    using System;
    using System.Collections.Generic;
    using System.Net.Http;
    using System.Threading.Tasks;
    using AzureIoTHub.Portal.Client.Services;
    using AzureIoTHub.Portal.Models.v10;
    using FluentAssertions;
    using Newtonsoft.Json;
    using NUnit.Framework;
    using RichardSzalay.MockHttp;
    using static System.Net.Mime.MediaTypeNames;

    [TestFixture]
    public class EdgeDeviceClientServiceTests
    {

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

            var expectedLogs = new List<IoTEdgeDeviceLog>()
            {
                expectedLog
            };

            var mockHttp = new MockHttpMessageHandler();

            _ = mockHttp.When(HttpMethod.Post, $"http://localhost/api/edge/devices/{deviceId}/logs")
                .Respond(Application.Json, JsonConvert.SerializeObject(expectedLogs));

            var client = new HttpClient(mockHttp)
            {
                BaseAddress = new Uri("http://localhost")
            };

            var edgeDeviceClientService = new EdgeDeviceClientService(client);

            // Act
            var result = await edgeDeviceClientService.GetEdgeDeviceLogs(deviceId, edgeModule);

            // Assert
            _ = result.Should().NotBeNull();

            _ = result.Count.Should().Be(1);
        }

        [Test]
        public async Task GetEdgeDeviceLogsMustThowHttpRequestExceptionWhenError()
        {
            // Arrange
            var deviceId = Guid.NewGuid().ToString();

            var edgeModule = new IoTEdgeModule
            {
                Version = "1.0",
                ModuleName = Guid.NewGuid().ToString()
            };

            var mockHttp = new MockHttpMessageHandler();

            _ = mockHttp.When(HttpMethod.Post, $"http://localhost/api/edge/devices/{deviceId}/logs")
                .Respond(System.Net.HttpStatusCode.BadRequest);

            var client = new HttpClient(mockHttp)
            {
                BaseAddress = new Uri("http://localhost")
            };

            var edgeDeviceClientService = new EdgeDeviceClientService(client);

            // Act
            var act = () => edgeDeviceClientService.GetEdgeDeviceLogs(deviceId, edgeModule);

            // Assert
            _ = await act.Should().ThrowAsync<HttpRequestException>();
        }
    }
}
