// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace AzureIoTHub.Portal.Tests.Unit.Server.Controllers.v1._0
{
    using AutoFixture;
    using AzureIoTHub.Portal.Models.v10;
    using AzureIoTHub.Portal.Shared.Models.v1._0;
    using AzureIoTHub.Portal.Shared.Models.v10.Filters;
    using System.Threading.Tasks;
    using AzureIoTHub.Portal.Tests.Unit.UnitTests.Bases;
    using Microsoft.AspNetCore.Mvc;
    using NUnit.Framework;
    using AzureIoTHub.Portal.Application.Services;
    using Moq;
    using Microsoft.Extensions.DependencyInjection;
    using System.Linq;
    using AzureIoTHub.Portal.Server.Controllers.V10;
    using FluentAssertions;

    [TestFixture]
    public class DeviceModelControllerBaseTests : BackendUnitTest
    {
        private Mock<IDeviceModelService<DeviceModelDto, DeviceModelDto>> mockDeviceModelService;

        private DeviceModelsController deviceModelsController;

        public override void Setup()
        {
            base.Setup();

            this.mockDeviceModelService = MockRepository.Create<IDeviceModelService<DeviceModelDto, DeviceModelDto>>();

            _ = ServiceCollection.AddSingleton(this.mockDeviceModelService.Object);

            Services = ServiceCollection.BuildServiceProvider();

            this.deviceModelsController = new DeviceModelsController(Services.GetRequiredService<IDeviceModelService<DeviceModelDto, DeviceModelDto>>());
        }

        [Test]
        public async Task GetListGreaterThan10ShouldReturnAListAndNextPage()
        {
            // Arrange
            var expectedDeviceModels = Fixture.CreateMany<DeviceModelDto>(12).ToList();

            var filter = new DeviceModelFilter
            {
                SearchText = string.Empty,
                PageNumber = 1,
                PageSize = 10,
                OrderBy = new string[]
                {
                    null
                }
            };

            _ = this.mockDeviceModelService.Setup(service => service.GetDeviceModels(filter))
                .ReturnsAsync(new PaginatedResult<DeviceModelDto> { Data = expectedDeviceModels });

            // Act
            var response = await this.deviceModelsController.GetItems(filter);

            // Assert
            _ = ((OkObjectResult)response.Result)?.Value.Should().BeEquivalentTo(expectedDeviceModels);
            MockRepository.VerifyAll();
        }
    }
}
