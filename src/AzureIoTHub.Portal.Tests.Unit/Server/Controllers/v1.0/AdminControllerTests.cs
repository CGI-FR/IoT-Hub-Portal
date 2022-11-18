// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace AzureIoTHub.Portal.Tests.Unit.Server.Controllers.v1._0
{
    using System;
    using System.IO;
    using System.Threading.Tasks;
    using AzureIoTHub.Portal.Server.Controllers.V10;
    using AzureIoTHub.Portal.Server.Managers;
    using FluentAssertions;
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
            _ = result.Should().NotBeNull();

            var response = result as FileStreamResult;
            _ = response.Should().NotBeNull();

            var stream = response.FileStream;
            var fileName = response.FileDownloadName;

            using var reader = new StreamReader(stream);
            var line = reader.ReadLine();
            _ = line.Should().Be(streamContent);
            _ = fileName.Should().StartWith("Devices_");

            this.mockRepository.VerifyAll();
        }
    }
}
