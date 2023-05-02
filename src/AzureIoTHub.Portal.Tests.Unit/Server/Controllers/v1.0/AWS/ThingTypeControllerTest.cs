// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace AzureIoTHub.Portal.Tests.Unit.Server.Controllers.v1._0.AWS
{
    using System;
    using System.Threading.Tasks;
    using AzureIoTHub.Portal.Application.Services.AWS;
    using AzureIoTHub.Portal.Domain.Repositories;
    using AzureIoTHub.Portal.Server.Controllers.v1._0.AWS;
    using AzureIoTHub.Portal.Models.v10.AWS;
    using Microsoft.AspNetCore.Mvc;
    using Moq;
    using NUnit.Framework;

    [TestFixture]
    public class ThingTypeControllerTest
    {
        private MockRepository mockRepository;

        private Mock<IThingTypeService> mockThingTypeService;
        private Mock<IThingTypeRepository> mockThingTypeRepository;
        private Mock<IUrlHelper> mockUrlHelper;

        [SetUp]
        public void SetUp()
        {

            this.mockRepository = new MockRepository(MockBehavior.Strict);

            this.mockThingTypeService = this.mockRepository.Create<IThingTypeService>();
            this.mockUrlHelper = this.mockRepository.Create<IUrlHelper>();
            this.mockThingTypeRepository = this.mockRepository.Create<IThingTypeRepository>();
        }

        private ThingTypeController CreateThingTypeController()
        {
            return new ThingTypeController(this.mockThingTypeService.Object)
            {
                Url = this.mockUrlHelper.Object
            };
        }

        [Test]
        public async Task CreateAThingTypeShouldReturnOK()
        {
            // Arrange
            var thingTypeController = CreateThingTypeController();

            var thingType = new ThingTypeDto()
            {
                ThingTypeID = Guid.NewGuid().ToString()
            };

            _ = this.mockThingTypeService
                .Setup(x => x.CreateThingType(It.Is<ThingTypeDto>(c => c.ThingTypeID.Equals(thingType.ThingTypeID, StringComparison.Ordinal))))
                .ReturnsAsync(thingType);

            // Act
            var result = await thingTypeController.CreateThingTypeAsync(thingType);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsAssignableFrom<OkObjectResult>(result);

            var okObjectResult = result as ObjectResult;

            Assert.IsNotNull(okObjectResult);
            Assert.AreEqual(200, okObjectResult.StatusCode);

            Assert.IsNotNull(okObjectResult.Value);
            Assert.IsAssignableFrom<ThingTypeDto>(okObjectResult.Value);

            var thingTypeObject = okObjectResult.Value as ThingTypeDto;
            Assert.IsNotNull(thingTypeObject);
            Assert.AreEqual(thingType.ThingTypeID, thingTypeObject.ThingTypeID);

            this.mockRepository.VerifyAll();
        }
    }
}
