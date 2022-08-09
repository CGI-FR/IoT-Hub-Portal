// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace AzureIoTHub.Portal.Tests.Unit.Server.Managers
{
    using System;
    using System.Net;
    using System.Net.Http;
    using System.Net.Http.Json;
    using System.Text;
    using System.Threading.Tasks;
    using AutoFixture;
    using Models.v10.LoRaWAN;
    using AzureIoTHub.Portal.Server.Managers;
    using UnitTests.Bases;
    using FluentAssertions;
    using Microsoft.Extensions.DependencyInjection;
    using NUnit.Framework;
    using RichardSzalay.MockHttp;

    [TestFixture]
    public class LoraDeviceMethodManagerTests : BackendUnitTest
    {
        private ILoraDeviceMethodManager loraDeviceMethodManager;

        public override void Setup()
        {
            base.Setup();

            _ = ServiceCollection.AddSingleton<ILoraDeviceMethodManager, LoraDeviceMethodManager>();

            Services = ServiceCollection.BuildServiceProvider();

            this.loraDeviceMethodManager = Services.GetRequiredService<ILoraDeviceMethodManager>();
        }

        [Test]
        public async Task ExecuteLoRaDeviceMessageThrowsArgumentNullExceptionWhenDeviceIdIsNull()
        {
            // Act
            var act = () => this.loraDeviceMethodManager.ExecuteLoRaDeviceMessage(null, null);

            // Assert
            _ = await act.Should().ThrowAsync<ArgumentNullException>();
        }

        [Test]
        public async Task ExecuteLoRaDeviceMessageThrowsArgumentNullExceptionWhenCommandIsNull()
        {
            // Arrange
            var deviceId = Fixture.Create<string>();

            // Act
            var act = () => this.loraDeviceMethodManager.ExecuteLoRaDeviceMessage(deviceId, null);

            // Assert
            _ = await act.Should().ThrowAsync<ArgumentNullException>();
        }

        [Test]
        public async Task ExecuteLoRaDeviceMessageMustBeSuccessfullWhenParametersAreProvided()
        {
            // Arrange
            var deviceId = Fixture.Create<string>();
            var command = new DeviceModelCommand
            {
                Frame = Fixture.Create<string>(),
                Confirmed = Fixture.Create<bool>(),
                Port = Fixture.Create<int>()
            };

            var expectedRawPayload = Convert.ToBase64String(Encoding.UTF8.GetBytes(command.Frame));

            _ = MockHttpClient.When(HttpMethod.Post, $"/api/cloudtodevicemessage/{deviceId}")
                .With(m =>
                {
                    _ = m.Content.Should().BeAssignableTo<JsonContent>();
                    var body = (JsonContent) m.Content;
                    var loRaCloudToDeviceMessage = (LoRaCloudToDeviceMessage)body?.Value;
                    _ = loRaCloudToDeviceMessage.Should().NotBeNull();
                    _ = loRaCloudToDeviceMessage?.Fport.Should().Be(command.Port);
                    _ = loRaCloudToDeviceMessage?.Confirmed.Should().Be(command.Confirmed);
                    _ = loRaCloudToDeviceMessage?.RawPayload.Should().Be(expectedRawPayload);
                    return true;
                })
                .Respond(HttpStatusCode.Created);

            // Act
            var result = await this.loraDeviceMethodManager.ExecuteLoRaDeviceMessage(deviceId, command);

            // Assert
            _ = result.Should().NotBeNull();
            _ = result.IsSuccessStatusCode.Should().BeTrue();
            MockHttpClient.VerifyNoOutstandingRequest();
            MockHttpClient.VerifyNoOutstandingExpectation();
        }
    }
}
