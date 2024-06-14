// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace IoTHub.Portal.Tests.Unit.Server.Controllers.v10
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using IoTHub.Portal.Application.Services;
    using IoTHub.Portal.Models.v10;
    using IoTHub.Portal.Server.Controllers.v10;
    using IoTHub.Portal.Shared.Models.v10.Filters;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using Moq;
    using NUnit.Framework;

    [TestFixture]
    public class EdgeModelsControllerTest
    {
        private MockRepository mockRepository;

        private Mock<IEdgeModelService> mockEdgeModelService;

        [SetUp]
        public void Setup()
        {
            this.mockRepository = new MockRepository(MockBehavior.Strict);

            this.mockEdgeModelService = this.mockRepository.Create<IEdgeModelService>();
        }

        private EdgeModelsController CreateController()
        {
            return new EdgeModelsController(this.mockEdgeModelService.Object);
        }

        [Test]
        public async Task GetEdgeModelListShouldReturnAList()
        {
            // Arrange
            var edgeModelController = CreateController();

            _ = this.mockEdgeModelService
                .Setup(x => x.GetEdgeModels(It.IsAny<EdgeModelFilter>()))
                .ReturnsAsync(new List<IoTEdgeModelListItem>()
                {
                    new IoTEdgeModelListItem()
                });

            // Act
            var response = await edgeModelController.GetEdgeModelList(new EdgeModelFilter());

            // Assert
            Assert.IsNotNull(response);
            Assert.IsAssignableFrom<OkObjectResult>(response.Result);

            if (response.Result is OkObjectResult okResponse)
            {
                Assert.AreEqual(200, okResponse.StatusCode);

                Assert.IsNotNull(okResponse.Value);
                if (okResponse.Value is List<IoTEdgeModelListItem> result)
                {
                    Assert.AreEqual(1, result.Count);
                }
            }
            else
            {
                Assert.Fail("Cannot inspect the result.");
            }

            this.mockRepository.VerifyAll();
        }

        [Test]
        public async Task GetEdgeDeviceModelShouldReturnEdgeModel()
        {
            // Arrange
            var edgeModelController = CreateController();

            var expectedEdgeModel = new IoTEdgeModel()
            {
                ModelId = Guid.NewGuid().ToString()
            };

            _ = this.mockEdgeModelService
                .Setup(x => x.GetEdgeModel(It.Is<string>(s => s.Equals(expectedEdgeModel.ModelId, StringComparison.Ordinal))))
                .ReturnsAsync(expectedEdgeModel);

            // Act
            var response = await edgeModelController.GetEdgeDeviceModel(expectedEdgeModel.ModelId);

            // Assert
            Assert.IsNotNull(response);
            Assert.IsAssignableFrom<OkObjectResult>(response.Result);

            if (response.Result is OkObjectResult okObjectResult)
            {
                Assert.IsNotNull(okObjectResult.Value);
                Assert.IsAssignableFrom<IoTEdgeModel>(okObjectResult.Value);
            }
            else
            {
                Assert.Fail("Cannot inspect the result.");
            }

            this.mockRepository.VerifyAll();
        }

        [Test]
        public async Task CreateEdgeModelShouldCreateEdgeModel()
        {
            // Arrange
            var edgeModelController = CreateController();

            var expectedEdgeModel = new IoTEdgeModel
            {
                ModelId = Guid.NewGuid().ToString()
            };

            _ = this.mockEdgeModelService
                .Setup(x => x.CreateEdgeModel(It.IsAny<IoTEdgeModel>()))
                .Returns(Task.CompletedTask);

            // Act
            var response = await edgeModelController.CreateEdgeModel(expectedEdgeModel);

            // Assert
            Assert.IsNotNull(response);

            this.mockRepository.VerifyAll();
        }

        [Test]
        public async Task UpdateEdgeModelShouldReturnOkResult()
        {
            // Arrange
            var edgeModelController = CreateController();

            var expectedEdgeModel = new IoTEdgeModel
            {
                ModelId = Guid.NewGuid().ToString()
            };

            _ = this.mockEdgeModelService
                .Setup(x => x.UpdateEdgeModel(It.IsAny<IoTEdgeModel>()))
                .Returns(Task.CompletedTask);

            // Act
            var response = await edgeModelController.UpdateEdgeModel(expectedEdgeModel);

            // Assert
            Assert.IsNotNull(response);

            this.mockRepository.VerifyAll();
        }

        [Test]
        public async Task DeleteModelAsyncShouldDelete()
        {
            // Arrange
            var edgeModelController = CreateController();

            var edgeModelId = Guid.NewGuid().ToString();
            _ = this.mockEdgeModelService
                .Setup(x => x.DeleteEdgeModel(It.Is<string>(s => s.Equals(edgeModelId, StringComparison.Ordinal))))
                .Returns(Task.CompletedTask);

            // Act
            var response = await edgeModelController.DeleteModelAsync(edgeModelId);

            // Assert
            Assert.IsNotNull(response);

            this.mockRepository.VerifyAll();
        }

        [Test]
        public async Task GetAvatarShouldReturnAvatarUrl()
        {
            // Arrange
            var edgeModelController = CreateController();

            var expectedUrl = Guid.NewGuid().ToString();

            _ = this.mockEdgeModelService
                .Setup(x => x.GetEdgeModelAvatar(It.IsAny<string>()))
                .ReturnsAsync(expectedUrl);

            // Act
            var response = await edgeModelController.GetAvatar(Guid.NewGuid().ToString());

            // Assert
            Assert.IsNotNull(response);
            Assert.IsAssignableFrom<OkObjectResult>(response.Result);

            if (response.Result is OkObjectResult okResponse)
            {
                Assert.AreEqual(200, okResponse.StatusCode);

                Assert.IsNotNull(okResponse.Value);
                Assert.AreEqual(expectedUrl, okResponse.Value.ToString());
            }

            this.mockRepository.VerifyAll();
        }

        [Test]
        public async Task ChangeAvatarShouldReturnAvatarUrl()
        {
            // Arrange
            var edgeModelController = CreateController();

            var expectedUrl = Guid.NewGuid().ToString();
            var mockFile = this.mockRepository.Create<IFormFile>();

            _ = this.mockEdgeModelService
                .Setup(x => x.UpdateEdgeModelAvatar(It.IsAny<string>(), It.IsAny<IFormFile>()))
                .ReturnsAsync(expectedUrl);

            // Act
            var response = await edgeModelController.ChangeAvatar(Guid.NewGuid().ToString(), mockFile.Object);

            // Assert
            Assert.IsNotNull(response);
            Assert.IsAssignableFrom<OkObjectResult>(response.Result);

            if (response.Result is OkObjectResult okResponse)
            {
                Assert.AreEqual(200, okResponse.StatusCode);

                Assert.IsNotNull(okResponse.Value);
                Assert.AreEqual(expectedUrl, okResponse.Value.ToString());
            }

            this.mockRepository.VerifyAll();
        }

        [Test]
        public async Task DeleteAvatarShouldDeleteAvatar()
        {
            // Arrange
            var edgeModelController = CreateController();

            _ = this.mockEdgeModelService
                .Setup(x => x.DeleteEdgeModelAvatar(It.IsAny<string>()))
                .Returns(Task.CompletedTask);

            // Act
            var response = await edgeModelController.DeleteAvatar(Guid.NewGuid().ToString());

            // Assert
            Assert.IsNotNull(response);

            this.mockRepository.VerifyAll();
        }

        [Test]
        public async Task GetPublicEdgeModules_GetPublicEdgeModules_EdgeModulesReturned()
        {
            // Arrange
            var edgeModelController = CreateController();
            var publicEdgeModules = new List<IoTEdgeModule>
            {
                new IoTEdgeModule{
                    Id = Guid.NewGuid().ToString()
                }
            };

            _ = this.mockEdgeModelService
                .Setup(x => x.GetPublicEdgeModules())
                .ReturnsAsync(publicEdgeModules);

            // Act
            var response = await edgeModelController.GetPublicEdgeModules();

            // Assert
            Assert.IsNotNull(response);
            Assert.IsAssignableFrom<OkObjectResult>(response.Result);

            if (response.Result is OkObjectResult okResponse)
            {
                Assert.AreEqual(200, okResponse.StatusCode);

                Assert.IsNotNull(okResponse.Value);
                if (okResponse.Value is List<IoTEdgeModule> result)
                {
                    Assert.AreEqual(1, result.Count);
                }
            }
            else
            {
                Assert.Fail("Cannot inspect the result.");
            }

            this.mockRepository.VerifyAll();
        }

    }
}
