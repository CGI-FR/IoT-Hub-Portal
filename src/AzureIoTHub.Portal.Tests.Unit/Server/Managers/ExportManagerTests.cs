// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace AzureIoTHub.Portal.Tests.Unit.Server.Managers
{
    using System.Collections.Generic;
    using System.IO;
    using System.Threading.Tasks;
    using AzureIoTHub.Portal.Domain.Options;
    using AzureIoTHub.Portal.Models.v10;
    using AzureIoTHub.Portal.Models.v10.LoRaWAN;
    using AzureIoTHub.Portal.Server.Managers;
    using AzureIoTHub.Portal.Server.Services;
    using AzureIoTHub.Portal.Tests.Unit.UnitTests.Bases;
    using FluentAssertions;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Options;
    using Moq;
    using NUnit.Framework;

    [TestFixture]
    public class ExportManagerTests : BackendUnitTest
    {
        private IExportManager exportManager;
        private Mock<IExternalDeviceService> mockExternalDeviceService;
        private Mock<IDeviceService<DeviceDetails>> mockDeviceService;
        private Mock<IDeviceService<LoRaDeviceDetails>> mockLoraDeviceService;
        private Mock<IDeviceTagService> mockDeviceTagService;
        private Mock<IDeviceModelPropertiesService> mockDeviceModelPropertiesService;
        private Mock<IDevicePropertyService> mockDevicePropertyService;
        private Mock<IOptions<LoRaWANOptions>> mockLoRaWANOptions;

        public override void Setup()
        {
            base.Setup();

            this.mockExternalDeviceService = MockRepository.Create<IExternalDeviceService>();
            this.mockDeviceService = MockRepository.Create<IDeviceService<DeviceDetails>>();
            this.mockLoraDeviceService = MockRepository.Create<IDeviceService<LoRaDeviceDetails>>();
            this.mockDeviceTagService = MockRepository.Create<IDeviceTagService>();
            this.mockDeviceModelPropertiesService = MockRepository.Create<IDeviceModelPropertiesService>();
            this.mockDevicePropertyService = MockRepository.Create<IDevicePropertyService>();
            this.mockLoRaWANOptions = MockRepository.Create<IOptions<LoRaWANOptions>>();

            _ = ServiceCollection.AddSingleton(this.mockExternalDeviceService.Object);
            _ = ServiceCollection.AddSingleton(this.mockDeviceService.Object);
            _ = ServiceCollection.AddSingleton(this.mockLoraDeviceService.Object);
            _ = ServiceCollection.AddSingleton(this.mockDeviceTagService.Object);
            _ = ServiceCollection.AddSingleton(this.mockDeviceModelPropertiesService.Object);
            _ = ServiceCollection.AddSingleton(this.mockDevicePropertyService.Object);
            _ = ServiceCollection.AddSingleton(this.mockLoRaWANOptions.Object);

            _ = ServiceCollection.AddSingleton<IExportManager, ExportManager>();

            Services = ServiceCollection.BuildServiceProvider();

            this.exportManager = Services.GetRequiredService<IExportManager>();
        }

        [Test]
        public async Task ExportDeviceListLoRaDisabledShouldWriteStream()
        {
            _ = this.mockLoRaWANOptions.Setup(x => x.Value)
                .Returns(new LoRaWANOptions
                {
                    Enabled = false
                });

            using var fileStream = new MemoryStream();

            _ = this.mockExternalDeviceService.Setup(x => x.GetDevicesToExport())
            .ReturnsAsync(new List<string>()
                {
                    /*lang=json*/
                    "{ \"deviceId\": \"000001\", \"tags\": { \"deviceName\": \"DeviceExport01\", \"supportLoRaFeatures\": \"true\", \"modelId\": \"01a440ca-9a67-4334-84a8-0f39995612a4\", \"Tag1\": \"Tag1-1\"}, \"desired\": { \"Property1\": \"123\", \"Property2\": \"456\" }}",
                    /*lang=json*/
                    "{ \"deviceId\": \"000002\", \"tags\": { \"deviceName\": \"DeviceExport02\", \"supportLoRaFeatures\": \"true\", \"modelId\": \"01a440ca-9a67-4334-84a8-0f39995612a4\", \"Tag1\": \"Tag1-2\"}, \"desired\": { \"Property1\": \"789\", \"Property2\": \"000\" }}"
                });

            _ = this.mockDeviceTagService.Setup(x => x.GetAllTagsNames())
                .Returns(new List<string>() { "Tag1", "Tag2" });

            _ = this.mockDeviceModelPropertiesService.Setup(x => x.GetAllPropertiesNames())
                .Returns(new List<string>() { "Property1", "Property2" });

            // Act + Assert
            Assert.AreEqual(0, fileStream.Length);
            _ = this.exportManager.ExportDeviceList(fileStream);
            fileStream.Position = 0;
            Assert.AreNotEqual(0, fileStream.Length);

            using var reader = new StreamReader(fileStream);
            var header = reader.ReadLine();
            _ = header.Split(",").Length.Should().Be(7);
            var content = reader.ReadToEnd();
            _ = content.TrimEnd().Split("\r\n").Length.Should().Be(2);
        }

        [Test]
        public async Task ExportDeviceListLoRaEnabledShouldWriteStreamAndDisplayLoRaSpecificField()
        {
            _ = this.mockLoRaWANOptions.Setup(x => x.Value)
                .Returns(new LoRaWANOptions
                {
                    Enabled = true
                });

            using var fileStream = new MemoryStream();

            _ = this.mockExternalDeviceService.Setup(x => x.GetDevicesToExport())
            .ReturnsAsync(new List<string>()
                {
                    /*lang=json*/
                    "{ \"deviceId\": \"000001\", \"tags\": { \"deviceName\": \"DeviceExport01\", \"supportLoRaFeatures\": \"true\", \"modelId\": \"01a440ca-9a67-4334-84a8-0f39995612a4\", \"Tag1\": \"Tag1-1\"}, \"desired\": { \"Property1\": \"123\", \"Property2\": \"456\" }}",
                    /*lang=json*/
                    "{ \"deviceId\": \"000002\", \"tags\": { \"deviceName\": \"DeviceExport02\", \"supportLoRaFeatures\": \"true\", \"modelId\": \"01a440ca-9a67-4334-84a8-0f39995612a4\", \"Tag1\": \"Tag1-2\"}, \"desired\": { \"Property1\": \"789\", \"Property2\": \"000\" }}"
                });

            _ = this.mockDeviceTagService.Setup(x => x.GetAllTagsNames())
                .Returns(new List<string>() { "Tag1", "Tag2" });

            _ = this.mockDeviceModelPropertiesService.Setup(x => x.GetAllPropertiesNames())
                .Returns(new List<string>() { "Property1", "Property2" });

            // Act + Assert
            Assert.AreEqual(0, fileStream.Length);
            _ = this.exportManager.ExportDeviceList(fileStream);
            fileStream.Position = 0;
            Assert.AreNotEqual(0, fileStream.Length);

            using var reader = new StreamReader(fileStream);
            var header = reader.ReadLine();
            _ = header.Split(",").Length.Should().Be(28);
            var content = reader.ReadToEnd();
            _ = content.TrimEnd().Split("\r\n").Length.Should().Be(2);

            MockRepository.VerifyAll();
        }

        [Test]
        public void ExportTemplateFileLoRaDisabledShouldWriteStream()
        {
            _ = this.mockLoRaWANOptions.Setup(x => x.Value)
                .Returns(new LoRaWANOptions
                {
                    Enabled = false
                });

            using var fileStream = new MemoryStream();

            _ = this.mockDeviceTagService.Setup(x => x.GetAllTagsNames())
                .Returns(new List<string>() { "Tag1", "Tag2" });

            _ = this.mockDeviceModelPropertiesService.Setup(x => x.GetAllPropertiesNames())
                .Returns(new List<string>() { "Property1", "Property2" });

            // Act + Assert
            Assert.AreEqual(0, fileStream.Length);
            _ = this.exportManager.ExportTemplateFile(fileStream);
            fileStream.Position = 0;
            Assert.AreNotEqual(0, fileStream.Length);

            using var reader = new StreamReader(fileStream);
            var content = reader.ReadToEnd();
            _ = content.TrimEnd().Split("\r\n").Length.Should().Be(1);
            _ = content.Split(",").Length.Should().Be(7);
        }

        [Test]
        public void ExportTemplateFileLoRaEnabledShouldWriteStreamAndDisplayLoRaSpecificField()
        {
            _ = this.mockLoRaWANOptions.Setup(x => x.Value)
                .Returns(new LoRaWANOptions
                {
                    Enabled = true
                });

            using var fileStream = new MemoryStream();

            _ = this.mockDeviceTagService.Setup(x => x.GetAllTagsNames())
                .Returns(new List<string>() { "Tag1", "Tag2" });

            _ = this.mockDeviceModelPropertiesService.Setup(x => x.GetAllPropertiesNames())
                .Returns(new List<string>() { "Property1", "Property2" });

            // Act + Assert
            Assert.AreEqual(0, fileStream.Length);
            _ = this.exportManager.ExportTemplateFile(fileStream);
            fileStream.Position = 0;
            Assert.AreNotEqual(0, fileStream.Length);

            using var reader = new StreamReader(fileStream);
            var content = reader.ReadToEnd();
            _ = content.TrimEnd().Split("\r\n").Length.Should().Be(1);
            _ = content.Split(",").Length.Should().Be(27);

            MockRepository.VerifyAll();
        }
    }
}
