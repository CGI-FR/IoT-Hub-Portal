// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace IoTHub.Portal.Tests.Unit.Server.Controllers.v10.LoRaWAN
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;
    using AutoFixture;
    using IoTHub.Portal.Application.Services;
    using IoTHub.Portal.Models.v10.LoRaWAN;
    using IoTHub.Portal.Server.Controllers.V10.LoRaWAN;
    using IoTHub.Portal.Shared.Models.v10;
    using IoTHub.Portal.Shared.Models.v10.Filters;
    using IoTHub.Portal.Tests.Unit.UnitTests.Bases;
    using FluentAssertions;
    using Hellang.Middleware.ProblemDetails;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Mvc.Routing;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Logging;
    using Moq;
    using NUnit.Framework;

    [TestFixture]
    public class LoRaWANConcentratorsControllerTest : BackendUnitTest
    {
        //private MockRepository mockRepository;

        private Mock<ILogger<LoRaWANConcentratorsController>> mockLogger;
        private Mock<IUrlHelper> mockUrlHelper;
        private Mock<ILoRaWANConcentratorService> mockLoRaWANConcentratorService;

        [SetUp]
        public void SetUp()
        {
            base.Setup();

            this.mockLogger = this.MockRepository.Create<ILogger<LoRaWANConcentratorsController>>();
            this.mockUrlHelper = this.MockRepository.Create<IUrlHelper>();
            this.mockLoRaWANConcentratorService = this.MockRepository.Create<ILoRaWANConcentratorService>();

            _ = ServiceCollection.AddSingleton(this.mockLogger.Object);
            _ = ServiceCollection.AddSingleton(this.mockUrlHelper.Object);
            _ = ServiceCollection.AddSingleton(this.mockLoRaWANConcentratorService.Object);

            Services = ServiceCollection.BuildServiceProvider();

        }

        private LoRaWANConcentratorsController CreateLoRaWANConcentratorsController()
        {
            return new LoRaWANConcentratorsController(
                this.mockLogger.Object,
                this.mockLoRaWANConcentratorService.Object)
            {
                Url = this.mockUrlHelper.Object
            };
        }

        [Test]
        public async Task GetAllDeviceConcentratorShouldReturnList()
        {
            // Arrange
            var concentratorsController = CreateLoRaWANConcentratorsController();

            var expectedPaginatedConcentrator= new PaginatedResult<ConcentratorDto>()
            {
                Data = Enumerable.Range(0, 100).Select(x => new ConcentratorDto
                {
                    DeviceId = FormattableString.Invariant($"{x}")
                }).ToList(),
                TotalCount = 100
            };

            _ = this.mockLoRaWANConcentratorService.Setup(service => service.GetAllDeviceConcentrator(
                    It.IsAny<ConcentratorFilter>()))
                .ReturnsAsync(expectedPaginatedConcentrator);

            var locationUrl = "http://location/concentrators";

            _ = this.mockUrlHelper
                .Setup(x => x.RouteUrl(It.IsAny<UrlRouteContext>()))
                .Returns(locationUrl);

            // Act
            var result = await concentratorsController.GetAllDeviceConcentrator(new ConcentratorFilter()).ConfigureAwait(false);


            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(expectedPaginatedConcentrator.Data.Count, result.Value.Items.Count());
            this.MockRepository.VerifyAll();

        }

        [Test]
        public async Task GetDeviceConcentratorWithValidArgumentShouldReturnConcentrator()
        {
            // Arrange
            var expectedConcentrator = new ConcentratorDto
            {
                DeviceId = Fixture.Create<string>(),
                DeviceType = "LoRa Concentrator"
            };

            _ = this.mockLoRaWANConcentratorService.Setup(x => x.GetConcentrator(It.IsAny<string>()))
                .ReturnsAsync(expectedConcentrator);

            var concentratorController = CreateLoRaWANConcentratorsController();

            // Act
            var response = await concentratorController.GetDeviceConcentrator(expectedConcentrator.DeviceId);

            // Assert
            Assert.IsNotNull(response);
            Assert.IsAssignableFrom<OkObjectResult>(response.Result);

            var okObjectResult = response.Result as OkObjectResult;
            Assert.IsNotNull(okObjectResult);
            Assert.AreEqual(200, okObjectResult.StatusCode);
            Assert.IsNotNull(okObjectResult.Value);
            Assert.IsAssignableFrom<ConcentratorDto>(okObjectResult.Value);

            var device = okObjectResult.Value as ConcentratorDto;
            Assert.IsNotNull(device);
            Assert.AreEqual(expectedConcentrator.DeviceId, device.DeviceId);
            Assert.AreEqual("LoRa Concentrator", device.DeviceType);

            this.MockRepository.VerifyAll();
        }

        [Test]
        public async Task CreateDeviceAsyncWithValidArgumentShouldReturnOkResult()
        {
            // Arrange
            var expectedConcentrator = new ConcentratorDto
            {
                DeviceId = Fixture.Create<string>(),
                DeviceType = "LoRa Concentrator",
                LoraRegion = Fixture.Create<string>(),
                IsEnabled = true
            };

            _ = this.mockLoRaWANConcentratorService.Setup(c => c.CreateDeviceAsync(expectedConcentrator))
                .ReturnsAsync(expectedConcentrator);

            var concentratorController = CreateLoRaWANConcentratorsController();

            // Act
            var response = await concentratorController.CreateDeviceAsync(expectedConcentrator);

            // Assert
            Assert.IsNotNull(response);
            Assert.IsAssignableFrom<OkObjectResult>(response);

            var okObjectResult = response as OkObjectResult;
            Assert.IsNotNull(okObjectResult);
            Assert.AreEqual(200, okObjectResult.StatusCode);
            Assert.IsNotNull(okObjectResult.Value);
            Assert.IsAssignableFrom<ConcentratorDto>(okObjectResult.Value);

            var device = okObjectResult.Value as ConcentratorDto;
            Assert.IsNotNull(device);
            Assert.AreEqual(expectedConcentrator.DeviceId, device.DeviceId);
            Assert.AreEqual("LoRa Concentrator", device.DeviceType);

            this.MockRepository.VerifyAll();
        }

        [Test]
        public async Task CreateDeviceAsyncModelNotValidShouldThrowProblemDetailsException()
        {
            // Arrange
            var expectedConcentrator = new ConcentratorDto
            {
                DeviceId = Fixture.Create<string>(),
                DeviceType = "LoRa Concentrator",
                LoraRegion = Fixture.Create<string>(),
                IsEnabled = true
            };

            var concentratorController = CreateLoRaWANConcentratorsController();
            concentratorController.ModelState.AddModelError("Key", "Device model is invalid");

            // Act
            var result = () => concentratorController.CreateDeviceAsync(expectedConcentrator);

            // Assert
            _ = await result.Should().ThrowAsync<ProblemDetailsException>();
            this.MockRepository.VerifyAll();
        }

        [Test]
        public async Task CreateDeviceAsyncNullDeviceShouldThrowArgumentNullException()
        {
            // Arrange
            var concentratorController = CreateLoRaWANConcentratorsController();

            // Act
            var result = () => concentratorController.CreateDeviceAsync(null);

            // Assert
            _ = await result.Should().ThrowAsync<ArgumentNullException>();
            this.MockRepository.VerifyAll();
        }

        [Test]
        public async Task UpdateDeviceAsyncWithValidArgumentShouldReturnOkResult()
        {
            // Arrange
            var expectedConcentrator = new ConcentratorDto
            {
                DeviceId = Fixture.Create<string>(),
                DeviceType = "LoRa Concentrator",
                LoraRegion = Fixture.Create<string>(),
                IsEnabled = true
            };

            _ = this.mockLoRaWANConcentratorService.Setup(c => c.UpdateDeviceAsync(expectedConcentrator))
                .ReturnsAsync(expectedConcentrator);

            var concentratorController = CreateLoRaWANConcentratorsController();

            // Act
            var response = await concentratorController.UpdateDeviceAsync(expectedConcentrator);

            // Assert
            Assert.IsNotNull(response);
            Assert.IsAssignableFrom<OkObjectResult>(response);

            var okObjectResult = response as OkObjectResult;
            Assert.IsNotNull(okObjectResult);
            Assert.AreEqual(200, okObjectResult.StatusCode);
            Assert.IsNotNull(okObjectResult.Value);
            Assert.IsAssignableFrom<ConcentratorDto>(okObjectResult.Value);

            var device = okObjectResult.Value as ConcentratorDto;
            Assert.IsNotNull(device);
            Assert.AreEqual(expectedConcentrator.DeviceId, device.DeviceId);
            Assert.AreEqual("LoRa Concentrator", device.DeviceType);

            this.MockRepository.VerifyAll();
        }

        [Test]
        public async Task UpdateDeviceAsyncModelNotValidShouldThrowProblemDetailsException()
        {
            // Arrange
            var expectedConcentrator = new ConcentratorDto
            {
                DeviceId = Fixture.Create<string>(),
                DeviceType = "LoRa Concentrator",
                LoraRegion = Fixture.Create<string>(),
                IsEnabled = true
            };

            var concentratorController = CreateLoRaWANConcentratorsController();
            concentratorController.ModelState.AddModelError("Key", "Device model is invalid");

            // Act
            var result = () => concentratorController.UpdateDeviceAsync(expectedConcentrator);

            // Assert
            _ = await result.Should().ThrowAsync<ProblemDetailsException>();
            this.MockRepository.VerifyAll();
        }

        [Test]
        public async Task UpdateDeviceAsyncNullDeviceShouldThrowArgumentNullException()
        {
            // Arrange
            var concentratorController = CreateLoRaWANConcentratorsController();

            // Act
            var result = () => concentratorController.UpdateDeviceAsync(null);

            // Assert
            _ = await result.Should().ThrowAsync<ArgumentNullException>();
            this.MockRepository.VerifyAll();
        }

        [Test]
        public async Task DeleteDeviceAsyncExpectedBehaviorShouldReturnOkResult()
        {
            // Arrange
            var concentratorController = CreateLoRaWANConcentratorsController();
            var deviceId = Fixture.Create<string>();

            _ = this.mockLoRaWANConcentratorService.Setup(x => x.DeleteDeviceAsync(It.IsAny<string>()))
                .Returns(Task.CompletedTask);

            // Act
            _ = await concentratorController.Delete(deviceId);

            // Assert
            this.MockRepository.VerifyAll();
        }
    }
}
