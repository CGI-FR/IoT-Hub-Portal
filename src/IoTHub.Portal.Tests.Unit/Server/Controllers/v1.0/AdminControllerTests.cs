// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace IoTHub.Portal.Tests.Unit.Server.Controllers.v10
{
    using System;
    using System.IO;
    using System.Threading.Tasks;
    using IoTHub.Portal.Application.Managers;
    using IoTHub.Portal.Server.Controllers.V10;
    using IoTHub.Portal.Shared.Models.v10;
    using FluentAssertions;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using Moq;
    using NUnit.Framework;

    [TestFixture]
    public class AdminControllerTests
    {
        private MockRepository mockRepository;
        private Mock<IExportManager> mockExportManager;


        [SetUp]
        public void SetUp()
        {
            this.mockRepository = new MockRepository(MockBehavior.Strict);
            this.mockExportManager = this.mockRepository.Create<IExportManager>();
        }

        private AdminController CreateAdminController()
        {
            return new AdminController(
                this.mockExportManager.Object);
        }

        [Test]
        public async Task ExportDeviceListShouldReturnFileStreamResult()
        {
            var streamContent = Guid.NewGuid().ToString();

            _ = this.mockExportManager.Setup(x => x.ExportDeviceList(It.IsAny<MemoryStream>()))
                .Callback((Stream stream) =>
                    {
                        using var writer = new StreamWriter(stream, leaveOpen: true);
                        writer.Write(streamContent);
                    }
                )
            .Returns(Task.CompletedTask);

            var adminController = CreateAdminController();

            var result = await adminController.ExportDeviceList();
            Assert.IsNotNull(result);

            var response = result as FileStreamResult;
            Assert.IsNotNull(response);

            var stream = response.FileStream;
            var fileName = response.FileDownloadName;

            using var reader = new StreamReader(stream);
            var line = reader.ReadLine();
            _ = line.Should().Be(streamContent);
            _ = fileName.Should().StartWith("Devices_");

            this.mockRepository.VerifyAll();
        }

        [Test]
        public async Task ExportTemplateFileShouldReturnFileStreamResult()
        {
            var streamContent = Guid.NewGuid().ToString();

            _ = this.mockExportManager.Setup(x => x.ExportTemplateFile(It.IsAny<MemoryStream>()))
                .Callback((Stream stream) =>
                {
                    using var writer = new StreamWriter(stream, leaveOpen: true);
                    writer.Write(streamContent);
                }
                )
            .Returns(Task.CompletedTask);

            var adminController = CreateAdminController();

            var result = await adminController.ExportTemplateFile();
            Assert.IsNotNull(result);

            var response = result as FileStreamResult;
            Assert.IsNotNull(response);

            var stream = response.FileStream;
            var fileName = response.FileDownloadName;

            using var reader = new StreamReader(stream);
            var line = reader.ReadLine();
            _ = line.Should().Be(streamContent);
            _ = fileName.Should().Be("Devices_Template.csv");

            this.mockRepository.VerifyAll();
        }

        [Test]
        public async Task ImportDeviceListShouldReturnErrorReport()
        {
            // Arrange
            var expectedResult = Array.Empty<ImportResultLine>();
            using var stream = new MemoryStream();
            var file = new FormFile(stream,1,1,"a","a");

            _ = this.mockExportManager.Setup(x => x.ImportDeviceList(It.IsAny<Stream>()))
                .ReturnsAsync(expectedResult);

            var adminController = CreateAdminController();

            // Act
            var result = await adminController.ImportDeviceList(file);

            // Assert
            Assert.IsNotNull(result);

            _ = result.Value.Should().BeNullOrEmpty();

            this.mockRepository.VerifyAll();
        }
    }
}
