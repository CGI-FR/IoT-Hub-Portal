using Azure;
using Azure.Data.Tables;
using AzureIoTHub.Portal.Server.Controllers.V10;
using AzureIoTHub.Portal.Server.Factories;
using AzureIoTHub.Portal.Server.Mappers;
using AzureIoTHub.Portal.Server.Services;
using AzureIoTHub.Portal.Shared.Models.V10.Device;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace AzureIoTHub.Portal.Server.Tests.Controllers.V10
{
    [TestFixture]
    public class DeviceTagSettingsControllerTest
    {
        /* private MockRepository mockRepository;

        //private Mock<IDeviceTagMapper> mockDeviceTagMapper;
        //private Mock<ITableClientFactory> mockTableClientFactory;
        //private Mock<TableClient> mockDeviceTagTableClient;
        private Mock<IDeviceTagService> mockDeviceTagService;
        private Mock<ILogger<DeviceTagSettingsController>> mockLogger;

        [SetUp]
        public void SetUp()
        {
            this.mockRepository = new MockRepository(MockBehavior.Strict);

            //this.mockDeviceTagMapper = this.mockRepository.Create<IDeviceTagMapper>();
            //this.mockTableClientFactory = this.mockRepository.Create<ITableClientFactory>();
            //this.mockDeviceTagTableClient = this.mockRepository.Create<TableClient>();
            this.mockDeviceTagService = this.mockRepository.Create<IDeviceTagService>();
            this.mockLogger = this.mockRepository.Create<ILogger<DeviceTagSettingsController>>();
        }

        private DeviceTagSettingsController CreateDeviceTagSettingsController()
        {
            return new DeviceTagSettingsController(
                this.mockLogger.Object,
                //this.mockDeviceTagMapper.Object,
                //this.mockTableClientFactory.Object,
                this.mockDeviceTagService.Object
           );
        }

        [Test]
        public async Task Post_Should_Create_New_Entity()
        {
            // Arrange
            var deviceTagSettingsController = this.CreateDeviceTagSettingsController();

            DeviceTag tag = new DeviceTag
            {
                Name = "testName",
                Label = "testLabel",
                Required = true,
                Searchable = true
            };

            var mockResponse = this.mockRepository.Create<Response>();

            this.mockDeviceTagService.Setup(c => c.UpdateTags(It.IsAny<List<DeviceTag>>()))
                .Returns(Task.CompletedTask);

            // Act
            var result = await deviceTagSettingsController.Post(new List<DeviceTag>(new[] { tag }));

            // Assert
            Assert.IsNotNull(result);
            Assert.IsAssignableFrom<OkResult>(result);
            this.mockDeviceTagService.VerifyAll();
            this.mockRepository.VerifyAll();
        }

        [Test]
        public void Get_Should_Return_A_List()
        {
            // Arrange
            var deviceTagSettingsController = this.CreateDeviceTagSettingsController();

            mockDeviceTagService.Setup(x => x.GetAllTags()).Returns(new DeviceTag[10].ToList());

            // Act
            var response = deviceTagSettingsController.Get();

            // Assert
            Assert.IsNotNull(response);
            Assert.IsAssignableFrom<OkObjectResult>(response.Result);
            var okResponse = response.Result as OkObjectResult;

            Assert.AreEqual(200, okResponse.StatusCode);

            Assert.IsNotNull(okResponse.Value);
            var result = okResponse.Value as IEnumerable<DeviceTag>;
            Assert.AreEqual(10, result.Count());

            this.mockDeviceTagService.VerifyAll();
            this.mockRepository.VerifyAll();
        }*/
    }
}
