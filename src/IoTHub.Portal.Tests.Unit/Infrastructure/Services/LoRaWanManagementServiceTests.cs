// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace IoTHub.Portal.Tests.Unit.Infrastructure.Services
{
    using System;
    using System.Net;
    using System.Net.Http;
    using System.Net.Http.Json;
    using System.Threading.Tasks;
    using AutoFixture;
    using IoTHub.Portal.Application.Services;
    using IoTHub.Portal.Infrastructure.Services;
    using FluentAssertions;
    using Microsoft.Extensions.DependencyInjection;
    using NUnit.Framework;
    using RichardSzalay.MockHttp;
    using UnitTests.Bases;
    using System.Linq;
    using Fare;
    using Shared.Models.v1._0.LoRaWAN;

    [TestFixture]
    public class LoRaWanManagementServiceTests : BackendUnitTest
    {
        private ILoRaWanManagementService loRaWanManagementService;

        public override void Setup()
        {
            base.Setup();

            _ = ServiceCollection.AddSingleton<ILoRaWanManagementService, LoRaWanManagementService>();

            Services = ServiceCollection.BuildServiceProvider();

            this.loRaWanManagementService = Services.GetRequiredService<ILoRaWanManagementService>();
        }

        [Test]
        public async Task ExecuteLoRaDeviceMessageThrowsArgumentNullExceptionWhenDeviceIdIsNull()
        {
            // Act
            var act = () => this.loRaWanManagementService.ExecuteLoRaDeviceMessage(null, null);

            // Assert
            _ = await act.Should().ThrowAsync<ArgumentNullException>();
        }

        [Test]
        public async Task ExecuteLoRaDeviceMessageThrowsArgumentNullExceptionWhenCommandIsNull()
        {
            // Arrange
            var deviceId = Fixture.Create<string>();

            // Act
            var act = () => this.loRaWanManagementService.ExecuteLoRaDeviceMessage(deviceId, null);

            // Assert
            _ = await act.Should().ThrowAsync<ArgumentNullException>();
        }

        [Test]
        public async Task ExecuteLoRaDeviceMessageMustBeSuccessfullWhenParametersAreProvided()
        {
            // Arrange
            var deviceId = Fixture.Create<string>();

            var regex = "[0-9A-F]{8,15}";

            var xeger = new Xeger(regex, new Random(0)); // Note zero in Random constructor

            var command = new DeviceModelCommandDto
            {
                Frame = xeger.Generate(),
                Confirmed = Fixture.Create<bool>(),
                Port = Fixture.Create<int>()
            };

            var expectedRawPayload = Convert.ToBase64String(Enumerable.Range(0, command.Frame.Length / 2).Select(x => Convert.ToByte(command.Frame.Substring(x * 2, 2), 16)).ToArray());

            _ = MockHttpClient.When(HttpMethod.Post, $"/api/cloudtodevicemessage/{deviceId}")
                .With(m =>
                {
                    _ = m.Content.Should().BeAssignableTo<JsonContent>();
                    var body = (JsonContent) m.Content;
                    var loRaCloudToDeviceMessage = (LoRaCloudToDeviceMessage)body?.Value;
                    _ = loRaCloudToDeviceMessage?.Should().NotBeNull();
                    _ = loRaCloudToDeviceMessage?.Fport.Should().Be(command.Port);
                    _ = loRaCloudToDeviceMessage?.Confirmed.Should().Be(command.Confirmed);
                    _ = loRaCloudToDeviceMessage?.RawPayload.Should().Be(expectedRawPayload);
                    return true;
                })
                .Respond(HttpStatusCode.Created);

            // Act
            var result = await this.loRaWanManagementService.ExecuteLoRaDeviceMessage(deviceId, command);

            // Assert
            _ = result.Should().NotBeNull();
            _ = result.IsSuccessStatusCode.Should().BeTrue();
            MockHttpClient.VerifyNoOutstandingRequest();
            MockHttpClient.VerifyNoOutstandingExpectation();
        }

        [Test]
        public async Task ExecuteLoRaDeviceMessageMustBeSuccessfullWhenParametersAndCommandAreProvided()
        {
            // Arrange
            var deviceId = Fixture.Create<string>();

            var commandHex = "0113007801680100640064";

            var command = new DeviceModelCommandDto
            {
                Frame = commandHex,
                Confirmed = Fixture.Create<bool>(),
                Port = Fixture.Create<int>()
            };

            var expectedRawPayload = "ARMAeAFoAQBkAGQ=";

            _ = MockHttpClient.When(HttpMethod.Post, $"/api/cloudtodevicemessage/{deviceId}")
                .With(m =>
                {
                    _ = m.Content.Should().BeAssignableTo<JsonContent>();
                    var body = (JsonContent) m.Content;
                    var loRaCloudToDeviceMessage = (LoRaCloudToDeviceMessage)body?.Value;
                    _ = loRaCloudToDeviceMessage?.Should().NotBeNull();
                    _ = loRaCloudToDeviceMessage?.Fport.Should().Be(command.Port);
                    _ = loRaCloudToDeviceMessage?.Confirmed.Should().Be(command.Confirmed);
                    _ = loRaCloudToDeviceMessage?.RawPayload.Should().Be(expectedRawPayload);
                    return true;
                })
                .Respond(HttpStatusCode.Created);

            // Act
            var result = await this.loRaWanManagementService.ExecuteLoRaDeviceMessage(deviceId, command);

            // Assert
            _ = result.Should().NotBeNull();
            _ = result.IsSuccessStatusCode.Should().BeTrue();
            MockHttpClient.VerifyNoOutstandingRequest();
            MockHttpClient.VerifyNoOutstandingExpectation();
        }

        [TestCase("CN_470_510_RP1")]
        [TestCase("CN_470_510_RP2")]
        [TestCase("EU_863_870")]
        [TestCase("US_902_928_FSB_1")]
        [TestCase("US_902_928_FSB_2")]
        [TestCase("US_902_928_FSB_3")]
        [TestCase("US_902_928_FSB_4")]
        [TestCase("US_902_928_FSB_5")]
        [TestCase("US_902_928_FSB_6")]
        [TestCase("US_902_928_FSB_7")]
        [TestCase("US_902_928_FSB_8")]
        public async Task GetRouterConfigStateUnderTestExpectedBehavior(string loraRegion)
        {
            // Act
            var result = await this.loRaWanManagementService.GetRouterConfig(loraRegion);

            // Assert
            Assert.IsNotNull(result);
        }
    }
}
