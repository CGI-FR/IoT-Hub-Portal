// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace IoTHub.Portal.Tests.Unit.Server.Controllers.v10
{
    using AutoFixture;
    using IoTHub.Portal.Models.v10;
    using IoTHub.Portal.Shared.Models.v10;
    using IoTHub.Portal.Shared.Models.v10.Filters;
    using System.Threading.Tasks;
    using IoTHub.Portal.Tests.Unit.UnitTests.Bases;
    using Microsoft.AspNetCore.Mvc;
    using NUnit.Framework;
    using IoTHub.Portal.Application.Services;
    using Moq;
    using Microsoft.Extensions.DependencyInjection;
    using System.Linq;
    using IoTHub.Portal.Server.Controllers.V10;
    using FluentAssertions;
    using System.Collections.Generic;
    using Microsoft.AspNetCore.Mvc.Routing;

    [TestFixture]
    public class DeviceModelControllerBaseTests : BackendUnitTest
    {
        private Mock<IDeviceModelService<DeviceModelDto, DeviceModelDto>> mockDeviceModelService;
        private Mock<IUrlHelper> mockUrlHelper;

        private DeviceModelsController deviceModelsController;

        [SetUp]
        public override void Setup()
        {
            base.Setup();

            this.mockDeviceModelService = MockRepository.Create<IDeviceModelService<DeviceModelDto, DeviceModelDto>>();
            this.mockUrlHelper = MockRepository.Create<IUrlHelper>();

            _ = ServiceCollection.AddSingleton(this.mockDeviceModelService.Object);

            Services = ServiceCollection.BuildServiceProvider();

            this.deviceModelsController = new DeviceModelsController(Services.GetRequiredService<IDeviceModelService<DeviceModelDto, DeviceModelDto>>())
            {
                Url = this.mockUrlHelper.Object
            };
        }

        [Test]
        public async Task GetListGreaterThan10ShouldReturnAListAndNextPage()
        {
            // Arrange
            var expectedDeviceModels = Fixture.CreateMany<DeviceModelDto>(24).ToList();

            var filter = new DeviceModelFilter
            {
                SearchText = string.Empty,
                PageNumber = 0,
                PageSize = 10,
                OrderBy = new string[]
                {
                    null
                }
            };

            _ = this.mockDeviceModelService.Setup(service => service.GetDeviceModels(filter))
                .ReturnsAsync((DeviceModelFilter filter) => new PaginatedResult<DeviceModelDto>
                {
                    Data = expectedDeviceModels.Skip(filter.PageSize * filter.PageNumber).Take(filter.PageSize).ToList(),
                    TotalCount = expectedDeviceModels.Count
                });

            var locationUrl = "http://location/models";

            _ = this.mockUrlHelper
                .Setup(x => x.RouteUrl(It.IsAny<UrlRouteContext>()))
                .Returns(locationUrl);

            // Act
            var response = await this.deviceModelsController.GetItems(filter);

            // Assert
            _ = ((OkObjectResult)response.Result)?.Value.Should().BeAssignableTo<IEnumerable<DeviceModelDto>>();
            var results = (response.Value)?.TotalItems;
            _ = results.Should().HaveValue("10");
            MockRepository.VerifyAll();
        }
    }
}
